using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Editor tool that automates the 2D sprite-sheet import pipeline:
///   1. (Optional) slices a sprite sheet into a uniform grid.
///   2. Generates AnimationClips from user-defined frame ranges.
///   3. Generates an AnimatorController wiring up all clips.
///   4. (Optional) spawns a prefab-ready animated GameObject.
/// Lives under Tools/Sprite Sheet Animation Generator.
/// </summary>
public class SpriteSheetAnimationGenerator : EditorWindow
{
    // ---------------------------------------------------------------------
    // Data structures
    // ---------------------------------------------------------------------

    /// <summary>
    /// Serializable definition of a single animation built from a contiguous
    /// range of frames inside the sliced sprite sheet.
    /// </summary>
    [System.Serializable]
    private class AnimDef
    {
        public string name = "Idle";
        public int startFrame = 0;
        public int endFrame = 0;
        public int frameRate = 12;
    }

    private const string OutputRoot = "Assets/GeneratedAnimations";

    // ---------------------------------------------------------------------
    // Window state
    // ---------------------------------------------------------------------

    private Texture2D _spriteSheet;

    // Slicing options.
    private bool _autoSlice = true;
    private int _cellWidth = 32;
    private int _cellHeight = 32;
    private int _offsetX = 0;
    private int _offsetY = 0;
    private int _paddingX = 0;
    private int _paddingY = 0;

    // Animation definitions edited in the UI.
    private readonly List<AnimDef> _animations = new List<AnimDef>
    {
        new AnimDef { name = "Idle", startFrame = 0, endFrame = 5, frameRate = 12 }
    };

    // Optional GameObject creation.
    private bool _createGameObject = false;

    private Vector2 _scroll;

    // ---------------------------------------------------------------------
    // Menu / window lifecycle
    // ---------------------------------------------------------------------

    [MenuItem("Tools/Sprite Sheet Animation Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<SpriteSheetAnimationGenerator>("Sprite Sheet Anim Gen");
        window.minSize = new Vector2(380, 420);
    }

    // ---------------------------------------------------------------------
    // GUI
    // ---------------------------------------------------------------------

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.LabelField("Sprite Sheet Animation Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // --- Sprite sheet (drag-and-drop via ObjectField) ----------------
        _spriteSheet = (Texture2D)EditorGUILayout.ObjectField(
            "Sprite Sheet", _spriteSheet, typeof(Texture2D), false);

        EditorGUILayout.Space();

        // --- Slicing options ---------------------------------------------
        _autoSlice = EditorGUILayout.ToggleLeft("Auto Slice Sprite Sheet", _autoSlice);
        if (_autoSlice)
        {
            EditorGUI.indentLevel++;
            _cellWidth = Mathf.Max(1, EditorGUILayout.IntField("Cell Width", _cellWidth));
            _cellHeight = Mathf.Max(1, EditorGUILayout.IntField("Cell Height", _cellHeight));

            // Optional padding/offset support.
            _offsetX = Mathf.Max(0, EditorGUILayout.IntField("Offset X", _offsetX));
            _offsetY = Mathf.Max(0, EditorGUILayout.IntField("Offset Y", _offsetY));
            _paddingX = Mathf.Max(0, EditorGUILayout.IntField("Padding X", _paddingX));
            _paddingY = Mathf.Max(0, EditorGUILayout.IntField("Padding Y", _paddingY));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // --- Animation list ----------------------------------------------
        EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
        DrawAnimationListHeader();

        int removeIndex = -1;
        for (int i = 0; i < _animations.Count; i++)
        {
            AnimDef def = _animations[i];
            EditorGUILayout.BeginHorizontal();
            def.name = EditorGUILayout.TextField(def.name);
            def.startFrame = EditorGUILayout.IntField(def.startFrame, GUILayout.Width(50));
            def.endFrame = EditorGUILayout.IntField(def.endFrame, GUILayout.Width(50));
            def.frameRate = EditorGUILayout.IntField(def.frameRate, GUILayout.Width(50));
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0)
        {
            _animations.RemoveAt(removeIndex);
        }

        if (GUILayout.Button("Add Animation", GUILayout.Height(32)))
        {
            // New row continues where the previous animation ended so frame
            // ranges stay contiguous by default.
            var newDef = new AnimDef();
            if (_animations.Count > 0)
            {
                AnimDef last = _animations[_animations.Count - 1];
                newDef.startFrame = last.endFrame + 1;
                newDef.endFrame = newDef.startFrame;
                newDef.frameRate = last.frameRate;
            }
            _animations.Add(newDef);
        }
        if (GUILayout.Button("Clear All"))
        {
            _animations.Clear();
        }

        EditorGUILayout.Space();

        // --- Optional GameObject -----------------------------------------
        _createGameObject = EditorGUILayout.ToggleLeft("Create Animated GameObject", _createGameObject);

        EditorGUILayout.Space();

        // --- Generate ----------------------------------------------------
        GUI.enabled = _spriteSheet != null;
        if (GUILayout.Button("Generate", GUILayout.Height(32)))
        {
            Generate();
        }
        GUI.enabled = true;

        if (_spriteSheet == null)
        {
            EditorGUILayout.HelpBox("Assign a Texture2D sprite sheet to begin.", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    private static void DrawAnimationListHeader()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("Start", EditorStyles.miniBoldLabel, GUILayout.Width(50));
        EditorGUILayout.LabelField("End", EditorStyles.miniBoldLabel, GUILayout.Width(50));
        EditorGUILayout.LabelField("FPS", EditorStyles.miniBoldLabel, GUILayout.Width(50));
        GUILayout.Space(26);
        EditorGUILayout.EndHorizontal();
    }

    // ---------------------------------------------------------------------
    // Generation pipeline
    // ---------------------------------------------------------------------

    private void Generate()
    {
        // --- Validation: sprite sheet ------------------------------------
        if (_spriteSheet == null)
        {
            Fail("No sprite sheet assigned.");
            return;
        }

        string sheetPath = AssetDatabase.GetAssetPath(_spriteSheet);
        if (string.IsNullOrEmpty(sheetPath))
        {
            Fail("The sprite sheet must be an asset inside the project.");
            return;
        }

        string sheetName = Path.GetFileNameWithoutExtension(sheetPath);

        // --- Validation: animation list ----------------------------------
        if (_animations.Count == 0)
        {
            Fail("The animation list is empty. Add at least one animation.");
            return;
        }

        for (int i = 0; i < _animations.Count; i++)
        {
            AnimDef def = _animations[i];
            if (string.IsNullOrWhiteSpace(def.name))
            {
                Fail($"Animation #{i + 1} has an empty name.");
                return;
            }
            if (def.startFrame < 0 || def.endFrame < def.startFrame)
            {
                Fail($"Animation '{def.name}' has an invalid frame range ({def.startFrame}..{def.endFrame}).");
                return;
            }
            if (def.frameRate <= 0)
            {
                Fail($"Animation '{def.name}' has an invalid frame rate ({def.frameRate}).");
                return;
            }
        }

        // --- Slicing -----------------------------------------------------
        if (_autoSlice && !SliceSpriteSheet(sheetPath))
        {
            // SliceSpriteSheet reports its own errors.
            return;
        }

        // --- Load sliced sprites -----------------------------------------
        Sprite[] sprites = LoadSprites(sheetPath);
        if (sprites.Length == 0)
        {
            Fail("No sprites were found on the sheet. Enable Auto Slice or slice the texture manually first.");
            return;
        }

        // --- Output folder -----------------------------------------------
        string outputFolder = EnsureOutputFolder(sheetName);

        // --- Create clips ------------------------------------------------
        var clips = new List<AnimationClip>();
        foreach (AnimDef def in _animations)
        {
            // Guard against ranges that exceed the available sprite count.
            if (def.endFrame >= sprites.Length)
            {
                Fail($"Animation '{def.name}' references frame {def.endFrame}, but only {sprites.Length} sprites exist (0..{sprites.Length - 1}).");
                return;
            }

            AnimationClip clip = CreateClip(def, sprites, sheetName, outputFolder);
            if (clip != null)
            {
                clips.Add(clip);
            }
        }

        if (clips.Count == 0)
        {
            Fail("No animation clips were created.");
            return;
        }

        // --- Animator controller -----------------------------------------
        AnimatorController controller = CreateController(clips, sheetName, outputFolder);

        // --- Optional GameObject -----------------------------------------
        if (_createGameObject)
        {
            CreateAnimatedGameObject(sheetName, sprites[0], controller);
        }

        // --- Finalize ----------------------------------------------------
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[SpriteSheetAnimationGenerator] Generated {clips.Count} clip(s) and 1 controller for '{sheetName}' in {outputFolder}.");
        EditorUtility.DisplayDialog("Sprite Sheet Animation Generator",
            $"Done!\n\nCreated {clips.Count} animation clip(s) and an Animator Controller for '{sheetName}'.", "OK");
    }

    // ---------------------------------------------------------------------
    // Slicing
    // ---------------------------------------------------------------------

    /// <summary>
    /// Configures the texture importer as a Multiple-mode sprite and slices it
    /// into a uniform grid, then reimports. Returns false on failure.
    /// </summary>
    private bool SliceSpriteSheet(string sheetPath)
    {
        var importer = AssetImporter.GetAtPath(sheetPath) as TextureImporter;
        if (importer == null)
        {
            Fail("Could not get a TextureImporter for the sprite sheet.");
            return false;
        }

        try
        {
            // Configure importer for sprite slicing.
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            // We need the real (source) pixel dimensions to build the grid.
            GetSourceTextureSize(importer, sheetPath, out int texWidth, out int texHeight);

            List<SpriteMetaData> metas = BuildGrid(texWidth, texHeight);
            if (metas.Count == 0)
            {
                Fail("Slicing produced no cells. Check the cell size against the texture dimensions.");
                return false;
            }

#pragma warning disable 618 // SpriteMetaData/spritesheet is the simplest reliable API in this Unity version.
            importer.spritesheet = metas.ToArray();
#pragma warning restore 618

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            return true;
        }
        catch (System.Exception e)
        {
            Fail($"Slicing failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Builds a row-major grid of sprite cells using a top-left origin (so the
    /// first sprite is the top-left cell, matching how artists read frames).
    /// Unity sprite rects use a bottom-left origin, so Y is flipped.
    /// </summary>
    private List<SpriteMetaData> BuildGrid(int texWidth, int texHeight)
    {
        var metas = new List<SpriteMetaData>();

        int columns = (texWidth - _offsetX + _paddingX) / (_cellWidth + _paddingX);
        int rows = (texHeight - _offsetY + _paddingY) / (_cellHeight + _paddingY);

        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int x = _offsetX + col * (_cellWidth + _paddingX);
                // Flip Y: top row first.
                int y = texHeight - _offsetY - (row + 1) * _cellHeight - row * _paddingY;
                if (x + _cellWidth > texWidth || y < 0)
                {
                    continue;
                }

                var meta = new SpriteMetaData
                {
                    name = $"{index}",
                    rect = new Rect(x, y, _cellWidth, _cellHeight),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
                metas.Add(meta);
                index++;
            }
        }

        return metas;
    }

    /// <summary>
    /// Reads the true source size of the texture (independent of import scaling).
    /// </summary>
    private static void GetSourceTextureSize(TextureImporter importer, string path, out int width, out int height)
    {
        width = 0;
        height = 0;

        // TextureImporter exposes the original size via reflection-free helper.
        var method = typeof(TextureImporter).GetMethod("GetWidthAndHeight",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method != null)
        {
            object[] args = { width, height };
            method.Invoke(importer, args);
            width = (int)args[0];
            height = (int)args[1];
        }

        // Fallback: load the texture and read its dimensions.
        if (width == 0 || height == 0)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                width = tex.width;
                height = tex.height;
            }
        }
    }

    // ---------------------------------------------------------------------
    // Sprite loading
    // ---------------------------------------------------------------------

    /// <summary>
    /// Loads all sliced sprites from the sheet, ordered by their numeric/name
    /// index so frame indices line up with the grid order.
    /// </summary>
    private static Sprite[] LoadSprites(string sheetPath)
    {
        return AssetDatabase.LoadAllAssetsAtPath(sheetPath)
            .OfType<Sprite>()
            .OrderBy(s => s.name, new NaturalSpriteNameComparer())
            .ToArray();
    }

    /// <summary>
    /// Comparer that sorts sprite names numerically when possible (e.g. so that
    /// "2" comes before "10") and alphabetically otherwise.
    /// </summary>
    private class NaturalSpriteNameComparer : IComparer<string>
    {
        public int Compare(string a, string b)
        {
            bool aNum = TryExtractTrailingNumber(a, out int an);
            bool bNum = TryExtractTrailingNumber(b, out int bn);
            if (aNum && bNum)
            {
                return an.CompareTo(bn);
            }
            return string.Compare(a, b, System.StringComparison.Ordinal);
        }

        private static bool TryExtractTrailingNumber(string s, out int number)
        {
            number = 0;
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            int end = s.Length;
            int start = end;
            while (start > 0 && char.IsDigit(s[start - 1]))
            {
                start--;
            }
            if (start == end)
            {
                return false;
            }
            return int.TryParse(s.Substring(start, end - start), out number);
        }
    }

    // ---------------------------------------------------------------------
    // Clip creation
    // ---------------------------------------------------------------------

    /// <summary>
    /// Builds a single AnimationClip from a frame range using a PPtr object
    /// reference curve bound to SpriteRenderer.sprite, then saves it as an asset.
    /// </summary>
    private static AnimationClip CreateClip(AnimDef def, Sprite[] sprites, string sheetName, string outputFolder)
    {
        var clip = new AnimationClip { frameRate = def.frameRate };

        // Loop by default (idle/run/etc. usually loop). This is configurable
        // per-clip later via AnimationClipSettings.
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Binding: SpriteRenderer.m_Sprite is the sprite swap track.
        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = string.Empty,
            propertyName = "m_Sprite"
        };

        int frameCount = def.endFrame - def.startFrame + 1;
        var keyframes = new ObjectReferenceKeyframe[frameCount];
        float frameDuration = 1f / def.frameRate;

        for (int i = 0; i < frameCount; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameDuration,
                value = sprites[def.startFrame + i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        // Naming convention: [spritesheetname]_[animationname]
        string clipName = $"{sheetName}_{def.name}";
        string clipPath = AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/{clipName}.anim");
        AssetDatabase.CreateAsset(clip, clipPath);

        return clip;
    }

    // ---------------------------------------------------------------------
    // Animator controller
    // ---------------------------------------------------------------------

    /// <summary>
    /// Creates an AnimatorController with one state per clip and marks the first
    /// clip's state as the default.
    /// </summary>
    private static AnimatorController CreateController(List<AnimationClip> clips, string sheetName, string outputFolder)
    {
        string controllerPath = AssetDatabase.GenerateUniqueAssetPath(
            $"{outputFolder}/{sheetName}_AnimationController.controller");

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        for (int i = 0; i < clips.Count; i++)
        {
            AnimatorState state = stateMachine.AddState(clips[i].name);
            state.motion = clips[i];

            // First clip becomes the default state.
            if (i == 0)
            {
                stateMachine.defaultState = state;
            }
        }

        return controller;
    }

    // ---------------------------------------------------------------------
    // Optional GameObject
    // ---------------------------------------------------------------------

    /// <summary>
    /// Creates a scene GameObject with a SpriteRenderer (first sprite) and an
    /// Animator (generated controller), named after the sprite sheet.
    /// </summary>
    private static void CreateAnimatedGameObject(string sheetName, Sprite firstSprite, AnimatorController controller)
    {
        var go = new GameObject(sheetName);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = firstSprite;

        var animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        Undo.RegisterCreatedObjectUndo(go, "Create Animated GameObject");
        Selection.activeGameObject = go;
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    /// <summary>
    /// Ensures Assets/GeneratedAnimations/[SheetName]/ exists and returns it.
    /// </summary>
    private static string EnsureOutputFolder(string sheetName)
    {
        if (!AssetDatabase.IsValidFolder(OutputRoot))
        {
            AssetDatabase.CreateFolder("Assets", "GeneratedAnimations");
        }

        string target = $"{OutputRoot}/{sheetName}";
        if (!AssetDatabase.IsValidFolder(target))
        {
            AssetDatabase.CreateFolder(OutputRoot, sheetName);
        }

        return target;
    }

    private static void Fail(string message)
    {
        Debug.LogError($"[SpriteSheetAnimationGenerator] {message}");
        EditorUtility.DisplayDialog("Sprite Sheet Animation Generator", message, "OK");
    }
}
