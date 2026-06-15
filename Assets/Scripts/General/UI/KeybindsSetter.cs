using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class KeybindsSetter : MonoBehaviour
{
    public Transform menuPanel;
    [SerializeField] private Transform menuPanel2;
    TextMeshProUGUI buttonText;
    private EventSystem evt;
    Coroutine lastCoroutine;

    [Separator("AddTextEffect")]
    [SerializeField]
    private bool addTextEffects;
    [SerializeField]
    [ConditionalField(nameof(addTextEffects))] private string effectText;

    bool waitingForKey;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    private bool isRebindingActive = false;
    private InputAction actionToRebind;
    private bool isUsingKeyboard = true;

    void Start()
    {
        waitingForKey = false;
        evt = EventSystem.current;
        
        // Detect control scheme and populate UI
        RefreshControlScheme();
    }

    void OnEnable()
    {
        // Refresh when menu opens to show correct bindings for current device
        if (InputManager.instance != null)
        {
            RefreshControlScheme();
        }
    }

    void Update()
    {
        // Only check for device switch when not rebinding
        if (!waitingForKey && InputManager.instance != null)
        {
            bool currentlyUsingKeyboard = InputManager.instance.IsUsingKeyboard();
            if (currentlyUsingKeyboard != isUsingKeyboard)
            {
                RefreshControlScheme();
            }
        }
    }

    private void RefreshControlScheme()
    {
        isUsingKeyboard = InputManager.instance.IsUsingKeyboard();
        SetStartKeys();
    }

    public void StartAssignment()
    {
        GameObject button = evt.currentSelectedGameObject;
        string name = button.transform.parent.name;
        buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        
        if (!waitingForKey)
            lastCoroutine = StartCoroutine(AssignKey(name));
        else
        {
            CancelRebinding();
            lastCoroutine = StartCoroutine(AssignKey(name));
        }
    }

    public IEnumerator AssignKey(string keyName)
    {
        yield return new WaitForSecondsRealtime(0.1f);
        
        // Get the InputAction for this key
        Keybindings.KeyList? keyListNullable = GetKeyListFromName(keyName);
        if (!keyListNullable.HasValue)
        {
            Debug.LogWarning($"KeybindsSetter: Unknown key name '{keyName}'");
            yield break;
        }
        Keybindings.KeyList keyList = keyListNullable.Value;

        InputAction action = InputManager.instance.GetInputAction(keyList);
        if (action == null)
        {
            Debug.LogWarning($"KeybindsSetter: No InputAction found for {keyList}");
            yield break;
        }

        actionToRebind = action;
        waitingForKey = true;
        
        // Update button text to show waiting state
        string originalText = buttonText.text;
        buttonText.text = isUsingKeyboard ? "Press any key..." : "Press any button...";

        // Find the binding index for current control scheme
        int bindingIndex = GetBindingIndexForCurrentDevice(action);

        // Disable just the action being rebound (not the whole map)
        // This avoids corrupting other actions like the Move composite
        action.Disable();

        // Start rebinding operation
        isRebindingActive = true;
        var rebindOp = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => OnRebindingComplete(operation, keyName, originalText))
            .OnCancel(operation => OnRebindingCanceled(operation, originalText));

        // Configure based on current device
        if (isUsingKeyboard)
        {
            // For keyboard, exclude gamepad/joystick input, cancel with Escape
            rebindOp = rebindOp
                .WithControlsExcluding("<Gamepad>")
                .WithControlsExcluding("<Joystick>")
                .WithCancelingThrough("<Keyboard>/escape");
        }
        else
        {
            // For controller/joystick, exclude keyboard input
            rebindOp = rebindOp
                .WithControlsExcluding("<Keyboard>")
                .WithCancelingThrough("<Gamepad>/start");
        }

        rebindingOperation = rebindOp.Start();

        // Wait for rebinding to complete or cancel
        while (waitingForKey)
            yield return null;
    }

    private void OnRebindingComplete(InputActionRebindingExtensions.RebindingOperation operation, string keyName, string originalText)
    {
        waitingForKey = false;
        isRebindingActive = false;
        
        // Get the new binding path from the operation parameter (not the field!)
        string bindingPath = operation.selectedControl.path;
        string displayText = GetDisplayText(bindingPath);
        
        string newKeyText = addTextEffects ? effectText + displayText : displayText;
        buttonText.text = newKeyText;
        
        // Dispose the operation first
        operation.Dispose();
        
        // Re-enable the action and ensure nothing is left disabled
        ReenableActions();
        
        // Save the rebinding
        SaveRebinding();
        
        // Start a safety coroutine to ensure actions stay enabled
        StartCoroutine(EnsureActionsEnabled());
    }
    
    /// <summary>
    /// Safety coroutine that re-enables actions over the next few frames
    /// </summary>
    private IEnumerator EnsureActionsEnabled()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return null;
            
            if (InputManager.instance != null)
            {
                var controls = InputManager.instance.GetControlsAsset();
                if (controls != null)
                {
                    if (!controls.Player.enabled)
                        controls.Player.Enable();
                    
                    if (!controls.Player.Move.enabled)
                        controls.Player.Move.Enable();
                    
                    if (actionToRebind != null && !actionToRebind.enabled)
                        actionToRebind.Enable();
                }
            }
        }
    }

    private void OnRebindingCanceled(InputActionRebindingExtensions.RebindingOperation operation, string originalText)
    {
        waitingForKey = false;
        isRebindingActive = false;
        buttonText.text = originalText;
        
        // Dispose the operation first
        operation.Dispose();
        
        // Re-enable the action
        ReenableActions();
        
        // Start safety coroutine
        StartCoroutine(EnsureActionsEnabled());
    }
    
    private void ReenableActions()
    {
        // Re-enable just the action that was disabled
        if (actionToRebind != null)
        {
            actionToRebind.Enable();
        }
        
        // Safety check: ensure Player map and Move action are enabled
        if (InputManager.instance != null)
        {
            var controls = InputManager.instance.GetControlsAsset();
            if (controls != null)
            {
                if (!controls.Player.enabled)
                    controls.Player.Enable();
                
                if (!controls.Player.Move.enabled)
                    controls.Player.Move.Enable();
                
                try 
                { 
                    if (!controls.UI.enabled)
                        controls.UI.Enable(); 
                } 
                catch { }
            }
        }
    }

    private void CancelRebinding()
    {
        if (isRebindingActive && rebindingOperation != null)
        {
            try
            {
                rebindingOperation.Cancel();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"KeybindsSetter: Error canceling rebind: {e.Message}");
                ReenableActions();
            }
            isRebindingActive = false;
        }
        else
        {
            ReenableActions();
        }
        waitingForKey = false;
    }

    private Keybindings.KeyList? GetKeyListFromName(string keyName)
    {
        switch (keyName)
        {
            case "JumpKey": return Keybindings.KeyList.Jump;
            case "AttackKey": return Keybindings.KeyList.Attack;
            case "Shoot": return Keybindings.KeyList.Shoot;
            case "Skill1": return Keybindings.KeyList.Skill1;
            case "Skill2": return Keybindings.KeyList.Skill2;
            case "SkillsMenu": return Keybindings.KeyList.SkillsMenuHotKey;
            case "PauseMenu": return Keybindings.KeyList.PauseMenu;
            case "DestroyProjectile": return Keybindings.KeyList.ResetBullet;
            case "HealKey": return Keybindings.KeyList.Heal;
            case "UpKey": return Keybindings.KeyList.Up;
            case "DownKey": return Keybindings.KeyList.Down;
            case "RightKey": return Keybindings.KeyList.Right;
            case "LeftKey": return Keybindings.KeyList.Left;
            default: return null;
        }
    }

    private string GetDisplayText(string bindingPath)
    {
        // Convert Input System binding path to human-readable text
        string displayText = InputControlPath.ToHumanReadableString(
            bindingPath,
            InputControlPath.HumanReadableStringOptions.OmitDevice | 
            InputControlPath.HumanReadableStringOptions.UseShortNames
        );
        
        // Handle gamepad buttons - show controller-specific names
        if (bindingPath.Contains("<Gamepad>"))
        {
            if (bindingPath.Contains("buttonSouth")) return "A";
            if (bindingPath.Contains("buttonEast")) return "B";
            if (bindingPath.Contains("buttonWest")) return "X";
            if (bindingPath.Contains("buttonNorth")) return "Y";
            if (bindingPath.Contains("leftShoulder")) return "LB";
            if (bindingPath.Contains("rightShoulder")) return "RB";
            if (bindingPath.Contains("leftTrigger")) return "LT";
            if (bindingPath.Contains("rightTrigger")) return "RT";
            if (bindingPath.Contains("leftStickPress")) return "LS";
            if (bindingPath.Contains("rightStickPress")) return "RS";
            if (bindingPath.Contains("start")) return "Start";
            if (bindingPath.Contains("select")) return "Select";
        }
        
        // Handle joystick buttons - show friendly button numbers
        if (bindingPath.Contains("<Joystick>"))
        {
            if (bindingPath.Contains("/stick")) return "Stick";
            if (bindingPath.Contains("button0")) return "Button 1";
            if (bindingPath.Contains("button1")) return "Button 2";
            if (bindingPath.Contains("button2")) return "Button 3";
            if (bindingPath.Contains("button3")) return "Button 4";
            if (bindingPath.Contains("button4")) return "Button 5";
            if (bindingPath.Contains("button5")) return "Button 6";
            if (bindingPath.Contains("button6")) return "Button 7";
            if (bindingPath.Contains("button7")) return "Button 8";
            if (bindingPath.Contains("button8")) return "Button 9";
            if (bindingPath.Contains("button9")) return "Button 10";
            if (bindingPath.Contains("button10")) return "Button 11";
            if (bindingPath.Contains("button11")) return "Button 12";
        }
        
        // Uppercase single letter keys (e.g., 'a' -> 'A', 'z' -> 'Z')
        if (displayText.Length == 1 && char.IsLetter(displayText[0]))
        {
            return displayText.ToUpper();
        }
        
        // Capitalize first letter of multi-word display names
        if (!string.IsNullOrEmpty(displayText) && char.IsLower(displayText[0]))
        {
            return char.ToUpper(displayText[0]) + displayText.Substring(1);
        }
        
        return displayText;
    }

    /// <summary>
    /// Find the binding index for the current device (keyboard or gamepad/joystick)
    /// </summary>
    private int GetBindingIndexForCurrentDevice(InputAction action)
    {
        string targetGroup = isUsingKeyboard ? "Keyboard" : "GamePad";
        
        // For joystick: do a two-pass search
        // First pass: look specifically for <Joystick> bindings
        if (!isUsingKeyboard && Joystick.current != null)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                if (binding.isComposite || binding.isPartOfComposite)
                    continue;
                
                if (binding.groups.Contains(targetGroup) && binding.path.Contains("<Joystick>"))
                    return i;
            }
        }
        
        // Second pass (or primary pass for keyboard/gamepad): find matching group binding
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (binding.isComposite || binding.isPartOfComposite)
                continue;
            
            if (binding.groups.Contains(targetGroup))
            {
                if (isUsingKeyboard)
                    return i;
                
                if (Gamepad.current != null && binding.path.Contains("<Gamepad>"))
                    return i;
                
                if (!binding.path.Contains("<Keyboard>"))
                    return i;
            }
        }
        
        // Fallback: try to find any non-composite binding
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite)
                return i;
        }
        
        return 0;
    }

    public void SetStartKeys()
    {
        SetStartingButtons_Helper(menuPanel);
        if (menuPanel2 != null)
            SetStartingButtons_Helper(menuPanel2);
    }

    private void SetStartingButtons_Helper(Transform _menuPanel)
    {
        if (_menuPanel == null) return;
        
        string _textEffect = addTextEffects ? effectText : "";

        for (int i = 0; i < _menuPanel.childCount; i++)
        {
            string childName = _menuPanel.GetChild(i).name;
            Keybindings.KeyList? keyListNullable = GetKeyListFromName(childName);
            
            if (!keyListNullable.HasValue)
                continue;
            Keybindings.KeyList keyList = keyListNullable.Value;

            InputAction action = InputManager.instance.GetInputAction(keyList);
            if (action == null)
                continue;

            string bindingText = GetCurrentBindingText(action);
            
            TextMeshProUGUI textComponent = _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = _textEffect + bindingText;
            }
        }
    }

    private string GetCurrentBindingText(InputAction action)
    {
        int bindingIndex = GetBindingIndexForCurrentDevice(action);

        if (bindingIndex >= 0 && bindingIndex < action.bindings.Count)
        {
            InputBinding selectedBinding = action.bindings[bindingIndex];
            string path = selectedBinding.effectivePath;
            if (string.IsNullOrEmpty(path))
                path = selectedBinding.path;
            return GetDisplayText(path);
        }
        
        return "???";
    }

    private void SaveRebinding()
    {
        if (GameSaveManager.instance != null)
        {
            GameSaveManager.instance.SaveInputSystemRebindings();
        }
    }

    public void LoadRebindings()
    {
        if (GameSaveManager.instance != null)
        {
            GameSaveManager.instance.LoadInputSystemRebindings();
        }
        SetStartKeys();
    }

    void OnDisable()
    {
        CancelRebinding();
    }

    void OnDestroy()
    {
        CancelRebinding();
    }
}
