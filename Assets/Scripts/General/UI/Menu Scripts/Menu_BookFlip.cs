using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Febucci.UI;
using TMPro;

public class Menu_BookFlip : MonoBehaviour
{
    [SerializeField] private float openX_Position, open_MoveSpeed, flipSpeed = 0.5f, verticalOffset, testOffset, flipSeveralWait;
    [SerializeField] private List<RectTransform> pages;
    private int index = -1;
    private bool isRotatingNow = false, isNotebookOpen;
    [SerializeField] private Transform mainMenuObject;
    [SerializeField] private RectTransform notebookHolder;
    public int testIndex;
    private EventSystem evt;

    [Header("Selection")]
    [SerializeField]
    private GameObject currentSelectedObject, firstButton;
    [SerializeField]
    private RectTransform pointer;
    [SerializeField]
    private GameObject previous_SelectedObject;
    [SerializeField]
    private Febucci.UI.Effects.ShakeBehavior shakeSettings;

    private void Start()
    {
        evt = EventSystem.current;
    }

    private void Update()
    {
        if (!isNotebookOpen)
        {
            if (Input.anyKeyDown)
                OpenNotebook();

            return;
        }

        HanldeSelection();



        //if (Input.GetKeyDown(KeyCode.O)) ChangeIndexAndFlip(testIndex, true);
        if (Input.GetKeyDown(KeyCode.O)) RotateNext();
        //if (Input.GetKeyDown(KeyCode.P)) ChangeIndexAndFlip(testIndex, false);
        if (Input.GetKeyDown(KeyCode.P)) RotateBack();
        if (Input.GetKeyDown(KeyCode.L)) Rearrange_Array(6, 4);
        if (Input.GetKeyDown(KeyCode.Alpha0)) Rearrange_Array(4, 6);
        //if (Input.GetKeyDown(KeyCode.L)) FlipThroughAlot();
    }

    void OpenNotebook()
    {
        isNotebookOpen = true;
        //currentSelectedObject = pages[1].GetComponent<UI_FirstButton_Holder>().firstButton;
        StartCoroutine(MoveToPosition(notebookHolder));
    }

    private IEnumerator MoveToPosition(RectTransform coverTransform)
    {
        while (coverTransform.anchoredPosition.x < openX_Position)
        {
            coverTransform.anchoredPosition = new Vector2(coverTransform.anchoredPosition.x + open_MoveSpeed, coverTransform.anchoredPosition.y);

            yield return null;
        }

        coverTransform.anchoredPosition = new Vector2(openX_Position, coverTransform.anchoredPosition.y);

        RotateNext();
        // yield return new WaitForSeconds(0.01f);

        yield return null;
        RotateNext();
       // currentSelectedObject = pages[1].GetComponent<UI_FirstButton_Holder>().firstButton;
        Debug.Log(index);
        index = 1;
    }


    public void FlipThroughAlot(bool isForward)
    {
        StartCoroutine(FlipThroughSeveral(isForward));
    }

    private void HandlePreventDeselction(int IndexToSwap, bool isForward)
    {
        if (index == -1) return;

        //  int nextIndex = isForward ? index + 1 : index - 2;

        int isForwardInt = isForward ? +1 : -1;
        

        if (pages[IndexToSwap].GetChild(1).TryGetComponent<PreventDeselectionGroup>(out PreventDeselectionGroup _preventDG))
        {
            _preventDG.enabled = false;
        }
        if (pages[IndexToSwap + isForwardInt].GetChild(1).TryGetComponent<PreventDeselectionGroup>(out PreventDeselectionGroup _preventDG_previous))
        {
            _preventDG_previous.SelectPreviousButton();
            StartCoroutine(Re_Enable_preventDeselection(_preventDG_previous));
        }


        /*
        if (pages[IndexToSwap].GetChild(1).TryGetComponent<PreventDeselectionGroup>(out PreventDeselectionGroup _preventDG))
        {
            _preventDG.enabled = isEnabled;
            Debug.Log(isEnabled + "  " + _preventDG.gameObject);
            if (isEnabled) return;

            StartCoroutine(Re_Enable_preventDeselection(_preventDG));
        }*/

    }

    private IEnumerator Re_Enable_preventDeselection(PreventDeselectionGroup _preventD)
    {
        yield return new WaitForSeconds(0.5f);

        _preventD.enabled = true;
    }

    private IEnumerator FlipThroughSeveral(bool isForward)
    {
        if (isForward)
            RotateNext();
        else
            RotateBack();

        yield return new WaitForSeconds(flipSeveralWait);

        if (isForward)
            RotateNext();
        else
            RotateBack();
    }

    public void RotateNext()
    {
        Debug.Log(index);
        if(index == -1) Switch_ButtonInteractable(false, true);

        if ( index >= pages.Count - 1) return;
        //  HandlePreventDeselction(index, true);

        if (index > 0) SwitchSelectedButton(index + 1, true);

        index++;
        float angle = 180f;
        Switch_ButtonInteractable(false, false);
        pages[index].SetAsLastSibling();
        StartCoroutine(Rotate(angle, true, index));
        Switch_ButtonInteractable(true, false);  

    }

    public void RotateBack()
    {
        Debug.Log("called");
        // if on main menu return 
        if (index == 0) Switch_ButtonInteractable(true, true);

        if ( index <= -1) return;

        SwitchSelectedButton(index, false);

        float angle = 0f;
        int indexToFlip = isRotatingNow ? index - 1 : index;
        
        StartCoroutine(Rotate(angle, false, indexToFlip));
   //     Switch_ButtonInteractable(false, false);
      //  if (index > 0) pages[index - 1].SetAsLastSibling();
        pages[indexToFlip].SetAsLastSibling();
     //   Switch_ButtonInteractable(true, false);
    }

    private IEnumerator Rotate(float angle, bool isForward, int currentIndex)
    {

        Debug.Log(index);
        float value = 0;
        if(!isForward && index != 0)
        {
            Debug.Log("look here");
            pages[currentIndex - 1].GetChild(1).gameObject.SetActive(true);
            //HandlePreventDeselction(currentIndex, false);
            //HandlePreventDeselction(currentIndex - 1, true);
        }

        GameObject menuButtonsHolder = pages[currentIndex].GetChild(1).gameObject;
        bool test = false;

        while (true)
        {
            isRotatingNow = true;
            Quaternion targetRotation = Quaternion.Euler(0f, angle, 0f);
            value += Time.deltaTime * flipSpeed;
            pages[currentIndex].rotation = Quaternion.Slerp(pages[currentIndex].rotation, targetRotation, value);
            float angle1 = Quaternion.Angle(pages[currentIndex].rotation, targetRotation);
            int positiveMulitplier = isForward ? -1 : 1;



            if (angle1 < 0.1f)
            {
                if (!isForward)
                {
                    // pages[currentIndex].anchoredPosition = Vector3.zero;
                    pages[currentIndex].anchoredPosition = new Vector2(pages[currentIndex].anchoredPosition.x, 0f);
                    index--;
                }
                else if(isForward && currentIndex != 0)
                    pages[currentIndex - 1].GetChild(1).gameObject.SetActive(false);
                isRotatingNow = false;

                if (index == 1)
                    Switch_ButtonInteractable(true, false);

                break;
            }
            if(isForward && angle1 < 90f)
            {
                Vector3 currentPos = pages[currentIndex].transform.position;
                pages[currentIndex].transform.position = new Vector3(currentPos.x, currentPos.y + positiveMulitplier * verticalOffset, currentPos.z);
            }
            else if(!isForward && angle1 > 90f)
            {
                Vector3 currentPos = pages[currentIndex].transform.position;
                pages[currentIndex].transform.position = new Vector3(currentPos.x, currentPos.y + positiveMulitplier * verticalOffset * testOffset, currentPos.z);
            }

            if(!test && angle1 < 90)
            {
                test = true;
                HandlePageObjects(menuButtonsHolder, isForward, angle1);
            }
            yield return null;
        }
    }

    void HandlePageObjects(GameObject currentPage, bool isForward, float angle)
    {
        if (angle > 90)
            return;
        Debug.Log("tea");
        if ((isForward && currentPage.activeSelf) || (!isForward && !currentPage.activeSelf)) return;


        if (isForward)
            currentPage.SetActive(true);
        else
            currentPage.SetActive(false);
    }

    private void Switch_ButtonInteractable(bool isInteractable, bool isMainMenu)
    {
        Transform parentObject;
        if (!isMainMenu)
            parentObject = transform.GetChild(transform.childCount - 1);
        else
            parentObject = mainMenuObject;

        Button[] buttonsArray = parentObject.GetComponentsInChildren<Button>();
        foreach (Button currentButton in buttonsArray)
        {
            currentButton.interactable = isInteractable;
            //Debug.Log("buttons");
        }

        Debug.Log(parentObject + " " + isInteractable);
    }

    public void GoTo_SettingsMenu()
    {

    }

    public void EnterGame(bool isStartingNew)
    {
        if(isStartingNew)
        {

        }
        else
        {

        }
    }

    public void ChangeIndexAndFlip(int indexToFlip, bool _isForward)
    {
        
        if(_isForward)
        {
            index = indexToFlip - 1;
            RotateNext();
        }
        else
        {
          //  RotateBack();
            StartCoroutine(Rotate(0, false, index));
            pages[index].SetAsLastSibling();
            index = indexToFlip;
        }
    }


    public void Rearrange_Array(int indexToChange, int new_Index)
    {
        var page = pages[indexToChange];

        pages.RemoveAt(indexToChange);

        pages.Insert(new_Index, page);
    }


    public void SwitchPages_AndFlipNext(string indexToChange_Comma_newIndex)
    {
        /* string[] split = indexToChange_Comma_newIndex.Split(","[0]);
         int indexToChange = int.Parse(split[0]);
         int new_Index = int.Parse(split[1]);

         Rearrange_Array(indexToChange, new_Index);

         RotateNext();*/

        StartCoroutine(Switch_And_Flip(true, indexToChange_Comma_newIndex));
    }

    public void SwitchPages_AndFlip_Back(string indexToChange_Comma_newIndex)
    {

      /*  string[] split = indexToChange_Comma_newIndex.Split(","[0]);
        int indexToChange = int.Parse(split[0]);
        int new_Index = int.Parse(split[1]);

        Rearrange_Array(indexToChange, new_Index);
        RotateBack();*/

        StartCoroutine(Switch_And_Flip(false, indexToChange_Comma_newIndex));
    }

    private IEnumerator Switch_And_Flip(bool isForward, string indexToChange_Comma_newIndex)
    {
        if (isForward)
            RotateNext();
        else
            RotateBack();

        yield return new WaitForSeconds(flipSeveralWait);

        string[] split = indexToChange_Comma_newIndex.Split(","[0]);
        int indexToChange = int.Parse(split[0]);
        int new_Index = int.Parse(split[1]);

        Rearrange_Array(indexToChange, new_Index);

        if (isForward)
            RotateNext();
        else
            RotateBack();
    }

    public void DisableWithDelay(GameObject objectToDisable)
    {
        StartCoroutine(WaitAndDisable(objectToDisable));
    }

    private IEnumerator WaitAndDisable(GameObject objectToDisable)
    {
        yield return new WaitForSeconds(flipSeveralWait);

        objectToDisable.SetActive(false);
    }

    private void HanldeSelection()
    {
        if (currentSelectedObject == null) currentSelectedObject = pages[1].GetComponent<UI_FirstButton_Holder>().firstButton;

        if (evt.currentSelectedGameObject != null && evt.currentSelectedGameObject != currentSelectedObject)
        {
              currentSelectedObject.GetComponentInChildren<Add_TextEffectWhen>().ChangeAnimator(false);
              currentSelectedObject = evt.currentSelectedGameObject;

              currentSelectedObject.GetComponentInChildren<Add_TextEffectWhen>().ChangeAnimator(true);
         //   else
   //     currentSelectedObject = evt.currentSelectedGameObject;

        }
        else if (currentSelectedObject != null && evt.currentSelectedGameObject == null)
            evt.SetSelectedGameObject(currentSelectedObject);


        if (Input.anyKey)
        {
            /* float buttonY = currentSelectedObject.GetComponent<RectTransform>().position.y;
             if (pointer.position.y != buttonY)
                 pointer.position = new Vector3(pointer.position.x, buttonY, pointer.position.x);
                 */

            if(Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                Vector3 buttonPos = currentSelectedObject.GetComponent<RectTransform>().position;
                if (pointer.position.y != buttonPos.y)
                    pointer.position = buttonPos;
            }

            if (InputManager.instance.KeyDown(Keybindings.KeyList.Jump))
            {
                Button _button = evt.currentSelectedGameObject.GetComponent<Button>();
                Toggle _toggle = evt.currentSelectedGameObject.GetComponent<Toggle>();
                if (_button != null)
                    _button.onClick.Invoke();
                else if (_toggle != null)
                    _toggle.isOn = !_toggle.isOn;
            }

            if(InputManager.instance.KeyDown(Keybindings.KeyList.Shoot) || InputManager.instance.KeyDown(Keybindings.KeyList.PauseMenu))
            {
                // go back
            }
        }

    }

    private void SwitchSelectedButton(int newIndex, bool isForward)
    {
        if (isForward)
        {
            GameObject newSelected = pages[newIndex].GetComponent<UI_FirstButton_Holder>().firstButton;
            if (newSelected == currentSelectedObject)
                return;

            previous_SelectedObject = currentSelectedObject;

            currentSelectedObject = newSelected;
            evt.SetSelectedGameObject(currentSelectedObject);
            Debug.Log(currentSelectedObject);
            currentSelectedObject.GetComponentInChildren<TextAnimator_TMP>().enabled = true;

            Cancel_TextAnimator(pages[index + 2].GetChild(1));
        }
        else
        {
            currentSelectedObject = previous_SelectedObject;
            evt.SetSelectedGameObject(currentSelectedObject);
        }
    }

    private void Cancel_TextAnimator(Transform pageParent)
    {
        Debug.Log("WARTADTGSDGSTAASDSAYTG" + pageParent);
        TextAnimator_TMP[] testArray = pageParent.GetComponentsInChildren<TextAnimator_TMP>();
        foreach (TextAnimator_TMP item in testArray)
        {
            Debug.Log(item.transform.parent + " here herherer erher");
            if (item.transform.parent.gameObject == currentSelectedObject)
                item.enabled = true;
            else
                item.SetText(item.GetComponent<TextMeshProUGUI>().text);
        }


       /* foreach(Transform child in pageParent)
        {
            if (child.gameObject == currentSelectedObject)
                return;

            Debug.Log(child);
            child.GetComponentInChildren<TextAnimator_TMP>().enabled = false;
        }*/
    }

    private IEnumerator Test(TextAnimator_TMP _textAnimator)
    {
      
        yield return new WaitForSeconds(0.5f);

        _textAnimator.enabled = false;
    }

}
