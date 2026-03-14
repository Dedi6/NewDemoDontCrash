using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input Manager using Unity's new Input System
/// Maintains backward compatibility with old interface
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public bool isTopDown;

    private Controls controls;
    private InputActionMap playerMap;
    private InputActionMap uiMap;
    
    // Cached action references for performance
    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction jumpAction;
    private InputAction shootAction;
    private InputAction resetBulletAction;
    private InputAction skill1Action;
    private InputAction skill2Action;
    private InputAction skillsMenuHotKeyAction;
    private InputAction pauseMenuAction;
    private InputAction upAction;
    private InputAction downAction;
    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction healAction;
    private InputAction cursorPositionAction;

    // Device detection
    private bool usingKeyboard = true;
    private string currentControlScheme = "Keyboard";

    // For backward compatibility - keep old Keybindings reference for UI
    [HideInInspector]
    public Keybindings currentKeybindings;
    public Keybindings keyboardKeybinds, joyStickKeybind, defaultKeyboard;
    public Dictionary<string, string> joyStickNames;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if (instance != this)
        {
            Destroy(this);
            return;
        }

        // Initialize Input System
        controls = new Controls();
        playerMap = controls.Player;
        
        // Try to get UI map - might not exist until Controls.cs is regenerated
        try
        {
            uiMap = controls.UI;
        }
        catch (System.Exception)
        {
            Debug.LogWarning("InputManager: UI action map not found. Please reimport Controls.inputactions in Unity.");
            uiMap = null;
        }
        
        CacheActions();
        EnableActions();
        
        // Detect initial input device
        DetectInputDevice();
        
        // Subscribe to device changes
        InputSystem.onDeviceChange += OnDeviceChange;
        
        // Initialize joyStickNames for compatibility
        HandleKeyWords();
    }

    void OnDestroy()
    {
        // Unsubscribe from device change event (safe to call even if not subscribed)
        InputSystem.onDeviceChange -= OnDeviceChange;
        
        if (controls != null)
        {
            controls.Disable();
            controls.Dispose();
        }
    }

    private void Start()
    {
        // For compatibility with old system
        if (PlayerPrefs.HasKey("UsingJoystick"))
        {
            usingKeyboard = false;
            currentControlScheme = "GamePad";
        }
        else
        {
            usingKeyboard = true;
            currentControlScheme = "Keyboard";
        }
        
        // Load Input System rebindings if GameSaveManager is available
        if (GameSaveManager.instance != null)
        {
            GameSaveManager.instance.LoadInputSystemRebindings();
        }
        
        ChangeMovementInScript();
    }

    private void Update()
    {
        // Continuously detect input device based on last input
        DetectInputDevice();
    }

    private void CacheActions()
    {
        // Cache all action references for performance
        moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
        attackAction = playerMap.FindAction("Attack", throwIfNotFound: false);
        jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);
        shootAction = playerMap.FindAction("Shoot", throwIfNotFound: true);
        resetBulletAction = playerMap.FindAction("ResetBullet", throwIfNotFound: false);
        skill1Action = playerMap.FindAction("Skill1", throwIfNotFound: false);
        skill2Action = playerMap.FindAction("Skill2", throwIfNotFound: false);
        skillsMenuHotKeyAction = playerMap.FindAction("SkillsMenuHotKey", throwIfNotFound: false);
        pauseMenuAction = playerMap.FindAction("PauseMenu", throwIfNotFound: false);
        upAction = playerMap.FindAction("Up", throwIfNotFound: false);
        downAction = playerMap.FindAction("Down", throwIfNotFound: false);
        leftAction = playerMap.FindAction("Left", throwIfNotFound: false);
        rightAction = playerMap.FindAction("Right", throwIfNotFound: false);
        healAction = playerMap.FindAction("Heal", throwIfNotFound: false);
        cursorPositionAction = playerMap.FindAction("CursorPosition", throwIfNotFound: false);
    }

    private void EnableActions()
    {
        playerMap.Enable();
        if (uiMap != null)
            uiMap.Enable();
    }

    // Get UI action map for EventSystem integration
    public InputActionMap GetUIActionMap()
    {
        return uiMap;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed)
        {
            DetectInputDevice();
        }
    }

    private void DetectInputDevice()
    {
        // Use the Input System's active control to detect which device is being used
        // This works with any controller type, not just standard gamepads
        
        // Check Move action for device detection (most commonly used)
        if (moveAction != null && moveAction.activeControl != null)
        {
            InputDevice device = moveAction.activeControl.device;
            bool isGamepadInput = device is Gamepad || device is Joystick;
            
            if (isGamepadInput && usingKeyboard)
            {
                usingKeyboard = false;
                currentControlScheme = "GamePad";
                ChangeMovementInScript();
            }
            else if (!isGamepadInput && !usingKeyboard)
            {
                usingKeyboard = true;
                currentControlScheme = "Keyboard";
                ChangeMovementInScript();
            }
        }
        
        // Also check for any button presses
        if (attackAction != null && attackAction.activeControl != null)
        {
            InputDevice device = attackAction.activeControl.device;
            bool isGamepadInput = device is Gamepad || device is Joystick;
            
            if (isGamepadInput && usingKeyboard)
            {
                usingKeyboard = false;
                currentControlScheme = "GamePad";
                ChangeMovementInScript();
            }
            else if (!isGamepadInput && !usingKeyboard)
            {
                usingKeyboard = true;
                currentControlScheme = "Keyboard";
                ChangeMovementInScript();
            }
        }
        
        // Fallback: check keyboard directly for switching back
        // EXCLUDE Enter and Space - these are used for menu navigation and shouldn't switch input mode
        // This allows players to use Enter/Space to navigate menus while staying in joystick mode
        if (Keyboard.current != null && IsGameplayKeyPressed())
        {
            if (!usingKeyboard)
            {
                usingKeyboard = true;
                currentControlScheme = "Keyboard";
                ChangeMovementInScript();
            }
        }
    }

    /// <summary>
    /// Check if a gameplay-related keyboard key is pressed (excludes menu navigation keys)
    /// </summary>
    private bool IsGameplayKeyPressed()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return false;
        
        // Check if any key is pressed
        if (!kb.anyKey.isPressed) return false;
        
        // Exclude menu navigation keys - these shouldn't switch to keyboard mode
        // This allows players to use keyboard to navigate menus while staying in joystick mode
        // Enter/Space = select, Escape = back/cancel
        if (kb.enterKey.isPressed || kb.numpadEnterKey.isPressed || 
            kb.spaceKey.isPressed || kb.escapeKey.isPressed)
            return false;
        
        // Some other key is pressed - this is a gameplay key
        return true;
    }

    // Public interface - maintains compatibility with old InputManager
    public bool KeyDown(Keybindings.KeyList key)
    {
        InputAction action = GetActionForKey(key);
        if (action != null)
            return action.WasPressedThisFrame();
        return false;
    }

    public bool GetKey(Keybindings.KeyList key)
    {
        InputAction action = GetActionForKey(key);
        if (action != null)
            return action.IsPressed();
        return false;
    }

    public bool KeyUp(Keybindings.KeyList key)
    {
        InputAction action = GetActionForKey(key);
        if (action != null)
            return action.WasReleasedThisFrame();
        return false;
    }

    // New methods for Input System
    public Vector2 GetMoveInput()
    {
        if (moveAction != null)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            // For keyboard, normalize to discrete values (-1, 0, 1)
            // For gamepad, keep analog values
            if (usingKeyboard)
            {
                input.x = Mathf.RoundToInt(input.x);
                input.y = Mathf.RoundToInt(input.y);
            }
            return input;
        }
        return Vector2.zero;
    }

    public Vector2 GetCursorPosition()
    {
        if (cursorPositionAction != null)
        {
            if (usingKeyboard)
            {
                // Return mouse position in world coordinates
                if (Camera.main != null)
                    return Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                // Return right stick input
                return cursorPositionAction.ReadValue<Vector2>();
            }
        }
        return Vector2.zero;
    }

    private InputAction GetActionForKey(Keybindings.KeyList key)
    {
        switch (key)
        {
            case Keybindings.KeyList.Jump:
                return jumpAction;
            case Keybindings.KeyList.Attack:
                return attackAction;
            case Keybindings.KeyList.Shoot:
                return shootAction;
            case Keybindings.KeyList.ResetBullet:
                return resetBulletAction;
            case Keybindings.KeyList.Skill1:
                return skill1Action;
            case Keybindings.KeyList.Skill2:
                return skill2Action;
            case Keybindings.KeyList.SkillsMenuHotKey:
                return skillsMenuHotKeyAction;
            case Keybindings.KeyList.PauseMenu:
                return pauseMenuAction;
            case Keybindings.KeyList.Up:
                return upAction;
            case Keybindings.KeyList.Down:
                return downAction;
            case Keybindings.KeyList.Left:
                return leftAction;
            case Keybindings.KeyList.Right:
                return rightAction;
            case Keybindings.KeyList.Heal:
                return healAction;
            default:
                Debug.LogWarning($"InputManager: Action for {key} not found!");
                return null;
        }
    }

    // For backward compatibility
    public void ChangeKeybindings()
    {
        // Toggle between keyboard and gamepad
        usingKeyboard = !usingKeyboard;
        currentControlScheme = usingKeyboard ? "Keyboard" : "GamePad";
        
        if (!PlayerPrefs.HasKey("UsingJoystick") && !usingKeyboard)
            PlayerPrefs.SetInt("UsingJoystick", 1);
        else if (PlayerPrefs.HasKey("UsingJoystick") && usingKeyboard)
            PlayerPrefs.DeleteKey("UsingJoystick");
        PlayerPrefs.Save();
        
        ChangeMovementInScript();
    }

    void ChangeMovementInScript()
    {
        if (GameMaster.instance == null || GameMaster.instance.playerInstance == null) return;

        if (!isTopDown)
        {
            MovementPlatformer mp = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
            if (mp != null)
                mp.SwitchToOrFromJoystick();
        }
        else
        {
            TopDownMovement tdm = GameMaster.instance.playerInstance.GetComponent<TopDownMovement>();
            if (tdm != null)
                tdm.SwitchToOrFromJoystick();
        }
    }

    public bool IsUsingKeyboard()
    {
        return usingKeyboard;
    }

    public void HandleKeyWords()
    {
        joyStickNames = new Dictionary<string, string>
        {
            { "JoystickButton0", "■" },
            { "JoystickButton1", "X" },
            { "JoystickButton2", "○" },
            { "JoystickButton3", "■" },
            { "JoystickButton4", "L1" },
            { "JoystickButton5", "R1" },
            { "JoystickButton6", "L2" },
            { "JoystickButton7", "R2" },
            { "JoystickButton8", "Share" },
            { "JoystickButton9", "Options" },
            { "JoystickButton10", "L3" },
            { "JoystickButton11", "R3" },
            { "JoystickButton12", "PS" },
            { "JoystickButton13", "PadPress" },
            { "JoystickButton15", "'Down'" }
        };
    }

    public string GetControllerKeyWord(string name)
    {
        if (joyStickNames != null && joyStickNames.ContainsKey(name))
            return joyStickNames[name];
        return name;
    }

    // Get InputAction for rebinding
    public InputAction GetInputAction(Keybindings.KeyList key)
    {
        return GetActionForKey(key);
    }

    // Get the Controls asset for rebinding operations
    public Controls GetControlsAsset()
    {
        return controls;
    }

    // For compatibility - these methods may be called but don't do anything in new system
    public void SetKeyBindingsDefault()
    {
        // Input System handles defaults through the .inputactions file
        Debug.Log("SetKeyBindingsDefault: Defaults are handled by Input System asset");
    }
}
