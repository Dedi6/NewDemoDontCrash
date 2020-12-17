using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueKeys : MonoBehaviour
{
    EventSystem evt;
    DialogueManager dialogueM;
    InputManager input;
    void Start()
    {
        dialogueM = GameMaster.instance.GetComponent<DialogueManager>();
        input = InputManager.instance;
        evt = EventSystem.current;
    }

    void Update()
    {
        if(input.KeyDown(Keybindings.KeyList.Jump))
        {
            if (evt.currentSelectedGameObject != null)
                evt.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            else
                dialogueM.SkipSentence();
        }
    }
}
