using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Keybindings", menuName = "Keybindings")]
public class Keybindings : ScriptableObject
{
    [System.Serializable]
    public class KeysArray
    {
        public KeyList KeyFor;

        public KeyCode keyBinding;
    }

    public KeysArray[] arrayOfKeys;

    //testing joystick name changes
    public Dictionary<string, string> controllerKeys;

    public Keybindings()
    {
        controllerKeys = new Dictionary<string, string>();
        HandleKeyWords();
    }
    public enum KeyList
    {
        Jump,
        Attack, 
        Shoot,
        ResetBullet,
        Skill1,
        Skill2,
        SkillsMenuHotKey,
        PauseMenu,
        Up,
        Down,
        Left,
        Right,
        Heal,
    }
    public KeyCode CheckKey(KeyList key)
    {
        KeysArray k = GetKeyPressed(key);
        return k.keyBinding;
    }

    private KeysArray GetKeyPressed(KeyList key)
    {
        foreach (KeysArray currentKey in arrayOfKeys)
        {
            if (currentKey.KeyFor == key)
            {
                return currentKey;
            }
        }
        return null;
    }

    public void ChangeKey(KeyList key, KeyCode newKey)
    {
        foreach (KeysArray currentKey in arrayOfKeys)
        {
            if (currentKey.KeyFor == key)
            {
                currentKey.keyBinding = newKey;
            }
        }
        GameSaveManager.instance.SaveKeybindings();
    }

    public void HandleKeyWords()
    {
        controllerKeys.Add("JoystickButton0", "Square");
        controllerKeys.Add("JoystickButton1", "X");
        controllerKeys.Add("JoystickButton2", "Circle");
        controllerKeys.Add("JoystickButton3", "Triangle");
        controllerKeys.Add("JoystickButton4", "L1");
        controllerKeys.Add("JoystickButton5", "R1");
        controllerKeys.Add("JoystickButton6", "L2");
        controllerKeys.Add("JoystickButton7", "R2");
        controllerKeys.Add("JoystickButton8", "Share");
        controllerKeys.Add("JoystickButton9", "Options");
        controllerKeys.Add("JoystickButton10", "L3");
        controllerKeys.Add("JoystickButton11", "R3");
        controllerKeys.Add("JoystickButton12", "PS");
        controllerKeys.Add("JoystickButton13", "PadPress");
    }

    public Dictionary<string, string> Get_KeybindsDict()
    {
        Dictionary<string, string> _dict = new Dictionary<string, string>();

        for (int i = 0; i < arrayOfKeys.Length; i++)
        {
            string keyfor_Enum = arrayOfKeys[i].KeyFor.ToString();
            string binding_Enum = arrayOfKeys[i].keyBinding.ToString();

            _dict.Add(keyfor_Enum, binding_Enum);
        }

        return _dict;
    }

    public void Load_DictToBinds(Dictionary<string, string> _dict)
    {
        Debug.Log(_dict.Count);
      //  arrayOfKeys = new KeysArray[_dict.Count];

        for (int i = 0; i < arrayOfKeys.Length; i++)
        {
        }
        int pointer = 0;
        foreach (var Keys in _dict)
        {

            KeyList binding_Key = (KeyList)System.Enum.Parse(typeof(KeyList), Keys.Key);
            KeyCode keyfor_Value = (KeyCode)System.Enum.Parse(typeof(KeyCode), Keys.Value);

            arrayOfKeys[pointer].KeyFor = binding_Key;
            arrayOfKeys[pointer].keyBinding = keyfor_Value;

            pointer++;
        }
    }
}
