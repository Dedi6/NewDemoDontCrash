using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [HideInInspector]
    public Keybindings keybindings;
    public Keybindings keyboardKeybinds, joyStickKeybind, defaultKeyboard;

    public Dictionary<string, string> joyStickNames;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
        DontDestroyOnLoad(this);

        HandleStartingKeybinds();
    }


    public bool KeyDown(Keybindings.KeyList key)
    {
        if (Input.GetKeyDown(keybindings.CheckKey(key)))
            return true;
        else
            return false;
    }

    public bool GetKey(Keybindings.KeyList key)
    {
        if (Input.GetKey(keybindings.CheckKey(key)))
            return true;
        else
            return false;
    }

    public bool KeyUp(Keybindings.KeyList key)
    {
        if (Input.GetKeyUp(keybindings.CheckKey(key)))
            return true;
        else
            return false;
    }

    public void ChangeKeybindings()
    {
        keybindings = keybindings == keyboardKeybinds ? joyStickKeybind : keyboardKeybinds;
        if (!PlayerPrefs.HasKey("UsingJoystick") && keybindings == joyStickKeybind)
            PlayerPrefs.SetInt("UsingJoystick", 1);
        else if(PlayerPrefs.HasKey("UsingJoystick") && keybindings == keyboardKeybinds)
            PlayerPrefs.DeleteKey("UsingJoystick");
        PlayerPrefs.Save();

        GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>().SwitchToOrFromJoystick();
    }

    private void HandleStartingKeybinds()
    {
        if (PlayerPrefs.HasKey("UsingJoystick"))
        {
            keybindings = joyStickKeybind;
            GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>().SwitchToOrFromJoystick();
        }
        else
        {
            keybindings = keyboardKeybinds;
        }
        HandleKeyWords();
    }

    public void HandleKeyWords()
    {
        joyStickNames = new Dictionary<string, string>    //○ ↑ ↓ → ← ■ ▲ ◯ ⚪ ⚫
        {
            { "JoystickButton0", "▲" },
            { "JoystickButton1", "○" },
            { "JoystickButton2", "X" },
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
            { "JoystickButton13", "PadPress" }
        };
    }

    public string GetControllerKeyWord(string name)
    {
        return joyStickNames[name];
    }

    public bool IsUsingKeyboard()
    {
        if (keybindings == keyboardKeybinds)
            return true;
        else
            return false;
    }

    public void SetKeyBindingsDefault()
    {
        foreach (Keybindings.KeysArray key in keyboardKeybinds.arrayOfKeys)
        {
            key.keyBinding = defaultKeyboard.CheckKey(key.KeyFor);
        }
    }
}
