using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class PreventDeselectionGroup : MonoBehaviour
{
    EventSystem evt;
    public GameObject firstButton;
    GameObject sel;

    private void Start()
    {
        evt = EventSystem.current;
    }


    private void Update()
    {
        if (evt.currentSelectedGameObject != null && evt.currentSelectedGameObject != sel)
            sel = evt.currentSelectedGameObject;
        else if (sel != null && evt.currentSelectedGameObject == null)
            evt.SetSelectedGameObject(sel);
        
        if (InputManager.instance.KeyDown(Keybindings.KeyList.Jump))
        {
            Button b = evt.currentSelectedGameObject.GetComponent<Button>();
            Toggle t = evt.currentSelectedGameObject.GetComponent<Toggle>();
            if (b != null)
                b.onClick.Invoke();
            else if (t != null)
                t.isOn = !t.isOn;
        }
    }
    private void OnEnable()
    {
        sel = firstButton;
    }

    private void OnDisable()
    {
        evt.SetSelectedGameObject(null);
    }

    public void SelectFirstButton()
    {
        sel = firstButton;
    }
}
