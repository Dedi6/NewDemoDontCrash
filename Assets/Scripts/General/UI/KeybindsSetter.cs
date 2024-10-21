using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class KeybindsSetter : MonoBehaviour
{
    public Transform menuPanel;
    [SerializeField] private Transform menuPanel2;
    Event keyEvent;
    TextMeshProUGUI buttonText;
    KeyCode newKey;
    [SerializeField]
    private Keybindings keys;
    private EventSystem evt;
    Coroutine lastCoroutine;


    [Separator("AddTextEffect")]
    [SerializeField]
    private bool addTextEffects;
    [SerializeField]
    [ConditionalField(nameof(addTextEffects))] private string effectText;

    bool waitingForKey;

    void Start()
    {
        waitingForKey = false;
        keys = InputManager.instance.currentKeybindings;
        evt = EventSystem.current;
        SetStartKeys();
    }

    private void OnGUI()
    {
        keyEvent = Event.current;

        if(keyEvent.isKey && waitingForKey)
        {
            newKey = keyEvent.keyCode;
            waitingForKey = false;
        }
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
            StopCoroutine(lastCoroutine);
            lastCoroutine = StartCoroutine(AssignKey(name));
        }
    }


    IEnumerator WaitForKey()
    {
        while (!keyEvent.isKey)
            yield return null;
    }

    public IEnumerator AssignKey(string keyName)
    {
        yield return new WaitForSecondsRealtime(0.1f);
        waitingForKey = true;

        
        yield return WaitForKey();


        string newKeyText = addTextEffects ? new string(effectText + newKey.ToString()) : newKey.ToString();


        switch(keyName) // the string needs to fit the button name in the editor you sweet fool
        {
            case "JumpKey":
                keys.ChangeKey(Keybindings.KeyList.Jump, newKey);
                buttonText.text = newKeyText;
                break;
            case "AttackKey":
                keys.ChangeKey(Keybindings.KeyList.Attack, newKey);
                buttonText.text = newKeyText;
                break;
            case "Shoot":
                keys.ChangeKey(Keybindings.KeyList.Shoot, newKey);
                Debug.Log(keyName + "  " + newKeyText + "  " + buttonText.text);
                buttonText.text = newKeyText;
                Debug.Log(keyName + "  " + newKeyText + "  " + buttonText.text);
                break;
            case "Skill1":
                keys.ChangeKey(Keybindings.KeyList.Skill1, newKey);
                buttonText.text = newKeyText;
                break;
            case "Skill2":
                keys.ChangeKey(Keybindings.KeyList.Skill2, newKey);
                buttonText.text = newKeyText;
                break;
            case "SkillsMenu":
                keys.ChangeKey(Keybindings.KeyList.SkillsMenuHotKey, newKey);
                buttonText.text = newKeyText;
                break;
            case "PauseMenu":
                keys.ChangeKey(Keybindings.KeyList.PauseMenu, newKey);
                buttonText.text = newKeyText;
                break;
            case "DestroyProjectile":
                keys.ChangeKey(Keybindings.KeyList.ResetBullet, newKey);
                buttonText.text = newKeyText;
                break;
            case "HealKey":
                keys.ChangeKey(Keybindings.KeyList.Heal, newKey);
                buttonText.text = newKeyText;
                break;
            case "UpKey":
                keys.ChangeKey(Keybindings.KeyList.Up, newKey);
                buttonText.text = newKeyText;
                break;
            case "DownKey":
                keys.ChangeKey(Keybindings.KeyList.Down, newKey);
                buttonText.text = newKeyText;
                break;
            case "RightKey":
                keys.ChangeKey(Keybindings.KeyList.Right, newKey);
                buttonText.text = newKeyText;
                break;
            case "LeftKey":
                keys.ChangeKey(Keybindings.KeyList.Left, newKey);
                buttonText.text = newKeyText;
                break;
        }  //assignKey

        yield return null;
    }   // when adding a new kew, add to set start too.
                                   //  Also, for the script to work, the button's parent need to be the same name as the string.
    public void SetStartKeys()
    {
        SetStartingButtons_Helper(menuPanel);
        SetStartingButtons_Helper(menuPanel2);
    }

    private void SetStartingButtons_Helper(Transform _menuPanel)
    {
        string _textEffect = addTextEffects ? effectText : "";

        for (int i = 0; i < _menuPanel.childCount; i++)
        {
            switch (_menuPanel.GetChild(i).name)
            {
                case "JumpKey":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.Jump).ToString();
                    break;
                case "AttackKey":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.Attack).ToString();
                    break;
                case "Shoot":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.Shoot).ToString();
                    break;
                case "Skill1":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.Skill1).ToString();
                    break;
                case "Skill2":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.Skill2).ToString();
                    break;
                case "SkillsMenu":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.SkillsMenuHotKey).ToString();
                    break;
                case "PauseMenu":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.PauseMenu).ToString();
                    break;
                case "DestroyProjectile":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.ResetBullet).ToString();
                    break;
                case "HealKey":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.Heal).ToString();
                    break;
                case "UpKey":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.Up).ToString();
                    break;
                case "DownKey":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.Down).ToString();
                    break;
                case "RightKey":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.Right).ToString();
                    break;
                case "LeftKey":
                    _menuPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = _textEffect + keys.CheckKey(Keybindings.KeyList.Left).ToString();
                    break;
            }
        }
    }
}
