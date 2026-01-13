using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MyBox;
public class PreventDeselectionGroup : MonoBehaviour
{
    EventSystem evt;
    public GameObject firstButton;
    GameObject sel;
    [SerializeField]
    private bool isUsingPointer, isMainMenu;
    [ConditionalField("isUsingPointer")] public RectTransform pointer;

    private void Start()
    {
        evt = EventSystem.current;
    }


    private void Update()
    {
        if(sel == null) sel = firstButton;

        if (evt.currentSelectedGameObject != null && evt.currentSelectedGameObject != sel)
        {
            if (isMainMenu)
            {
                sel.GetComponentInChildren<Add_TextEffectWhen>().ChangeAnimator(false);
                sel = evt.currentSelectedGameObject;
                sel.GetComponentInChildren<Add_TextEffectWhen>().ChangeAnimator(true);
            }
            else
                sel = evt.currentSelectedGameObject;

        }
        else if (sel != null && evt.currentSelectedGameObject == null)
            evt.SetSelectedGameObject(sel);
       

        if(isUsingPointer && Input.anyKeyDown)
        {
            float buttonY = sel.GetComponent<RectTransform>().position.y;
            if (pointer.position.y != buttonY)
                pointer.position = new Vector3(pointer.position.x, buttonY, pointer.position.x);
        }
        
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
        if (isMainMenu) StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        yield return new WaitForSeconds(0.5f);
        sel.GetComponentInChildren<Add_TextEffectWhen>().ChangeAnimator(true);
        Debug.Log(sel);
    }

    public void SelectPreviousButton()
    {
        sel = firstButton;
    }

    private void OnDisable()
    {
        if(!isMainMenu)
            evt.SetSelectedGameObject(null);
    }

    public void SelectFirstButton()
    {
        sel = firstButton;
    }
}
