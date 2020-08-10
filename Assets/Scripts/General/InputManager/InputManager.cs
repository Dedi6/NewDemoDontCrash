using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [HideInInspector]
    public Keybindings keybindings;
    public Keybindings keyboardKeybinds, joyStickKeybind;

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
        else if(PlayerPrefs.HasKey("UsingJoystick") && keybindings == joyStickKeybind)
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
    }
}
