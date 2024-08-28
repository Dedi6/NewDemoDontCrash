using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu_BookFlip : MonoBehaviour
{
    [SerializeField] private float openX_Position, open_MoveSpeed, flipSpeed = 0.5f, verticalOffset, testOffset, flipSeveralWait;
    [SerializeField] private List<RectTransform> pages;
    private int index = -1;
    private bool isRotatingNow = false, isNotebookOpen;
    [SerializeField] private Transform mainMenuObject;
    [SerializeField] private RectTransform notebookHolder;
    public int testIndex;


    private void Update()
    {
        if (!isNotebookOpen)
        {
            if (Input.anyKeyDown)
                OpenNotebook();

            return;
        }


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
        StartCoroutine(MoveToPosition(notebookHolder));
    }

    private IEnumerator MoveToPosition(RectTransform coverTransform)
    {
        Debug.Log("dsddss");
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
        Debug.Log(index);
        index = 1;
    }


    public void FlipThroughAlot()
    {
        StartCoroutine(FlipThroughSeveral());
    }

    private IEnumerator FlipThroughSeveral()
    {
        RotateNext();

        yield return new WaitForSeconds(flipSeveralWait);

        RotateNext();

        yield return new WaitForSeconds(flipSeveralWait);

        RotateNext();
    }

    public void RotateNext()
    {
        Debug.Log(index);
        if(index == -1) Switch_ButtonInteractable(false, true);

        if ( index >= pages.Count - 1) return;
        index++;
        float angle = 180f;
        Switch_ButtonInteractable(false, false);
        pages[index].SetAsLastSibling();
        StartCoroutine(Rotate(angle, true, index));
        Switch_ButtonInteractable(true, false);
    }

    public void RotateBack()
    {
        // if on main menu return 
        if (index == 0) Switch_ButtonInteractable(true, true);

        if ( index <= -1) return;
        float angle = 0f;
        StartCoroutine(Rotate(angle, false, index));
   //     Switch_ButtonInteractable(false, false);
      //  if (index > 0) pages[index - 1].SetAsLastSibling();
        pages[index].SetAsLastSibling();
     //   Switch_ButtonInteractable(true, false);
    }

    private IEnumerator Rotate(float angle, bool isForward, int currentIndex)
    {

        Debug.Log(index);
        float value = 0;
        if(!isForward && index != 0)
            pages[currentIndex - 1].GetChild(1).gameObject.SetActive(true);

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
         string[] split = indexToChange_Comma_newIndex.Split(","[0]);
         int indexToChange = int.Parse(split[0]);
         int new_Index = int.Parse(split[1]);

         Rearrange_Array(indexToChange, new_Index);

         RotateNext();
    }

}
