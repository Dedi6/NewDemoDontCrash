using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public bool isTopDown;

    [HideInInspector]
    public Keybindings currentKeybindings;
    public Keybindings keyboardKeybinds, joyStickKeybind, defaultKeyboard;

    public Dictionary<string, string> joyStickNames;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
        DontDestroyOnLoad(this);

    }

    private void Start()
    {
       // SetKeyBindingsDefault();
        HandleStartingKeybinds();
    }


    public bool KeyDown(Keybindings.KeyList key)
    {
        if (Input.GetKeyDown(currentKeybindings.CheckKey(key)))
            return true;
        else
            return false;
    }

    public bool GetKey(Keybindings.KeyList key)
    {
        if (Input.GetKey(currentKeybindings.CheckKey(key)))
            return true;
        else
            return false;
    }

    public bool KeyUp(Keybindings.KeyList key)
    {
        if (Input.GetKeyUp(currentKeybindings.CheckKey(key)))
            return true;
        else
            return false;
    }

    public void ChangeKeybindings()
    {
        currentKeybindings = currentKeybindings == keyboardKeybinds ? joyStickKeybind : keyboardKeybinds;
        if (!PlayerPrefs.HasKey("UsingJoystick") && currentKeybindings == joyStickKeybind)
            PlayerPrefs.SetInt("UsingJoystick", 1);
        else if(PlayerPrefs.HasKey("UsingJoystick") && currentKeybindings == keyboardKeybinds)
            PlayerPrefs.DeleteKey("UsingJoystick");
        PlayerPrefs.Save();

        ChangeMovementInScript();
    }

    void ChangeMovementInScript()
    {
        if (GameMaster.instance.playerInstance == null) return;

        if (!isTopDown)
            GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>().SwitchToOrFromJoystick(); // change so it will have a playerprefs and movementplaftormer will determine what to use
        else
            GameMaster.instance.playerInstance.GetComponent<TopDownMovement>().SwitchToOrFromJoystick();
    }

    private void HandleStartingKeybinds()
    {
        if (PlayerPrefs.HasKey("UsingJoystick"))
        {
            currentKeybindings = joyStickKeybind;
            ChangeMovementInScript();
        }
        else
        {
            currentKeybindings = keyboardKeybinds;
        }
        if(GameSaveManager.instance.IsPlayerDataNull())
        {
            SetKeyBindingsDefault();
        }
        else
            GameSaveManager.instance.Load_Keybinds();
        HandleKeyWords();
    }

    public void HandleKeyWords()
    {
        joyStickNames = new Dictionary<string, string>    //○ ↑ ↓ → ← ■ ▲ ◯ ⚪ ⚫
        {
            { "JoystickButton0", "■" }, // triangle
            { "JoystickButton1", "X" }, //O
            { "JoystickButton2", "X" }, // X
            { "JoystickButton3", "■" }, // square
            { "JoystickButton4", "L1" },
            { "JoystickButton5", "R1" },
            { "JoystickButton6", "L2" },
            { "JoystickButton7", "R2" },
            { "JoystickButton8", "Share" },
            { "JoystickButton9", "Options" },
            { "JoystickButton10", "L3" },
            { "JoystickButton11", "R3" },
            { "JoystickButton12", "PS" },
            { "JoystickButton13", "PadPress" }
        };
    }

    public string GetControllerKeyWord(string name)
    {
        return joyStickNames[name];
    }

    public bool IsUsingKeyboard()
    {
        if (currentKeybindings == keyboardKeybinds)
            return true;
        else
            return false;
    }

    public void SetKeyBindingsDefault()
    {
        /*foreach (Keybindings.KeysArray key in keyboardKeybinds.arrayOfKeys)
        {
            key.keyBinding = defaultKeyboard.CheckKey(key.KeyFor);
            Debug.Log(key.keyBinding);
        }*/
        for (int i = 0; i < defaultKeyboard.arrayOfKeys.Length ; i++)
        {
            keyboardKeybinds.arrayOfKeys[i].KeyFor = defaultKeyboard.arrayOfKeys[i].KeyFor;
            keyboardKeybinds.arrayOfKeys[i].keyBinding = defaultKeyboard.arrayOfKeys[i].keyBinding;
        }
    }
}
