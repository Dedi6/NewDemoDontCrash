using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_BookFlip : MonoBehaviour
{
    [SerializeField] private float flipSpeed = 0.5f, verticalOffset, testOffset, flipSeveralWait;
    [SerializeField] private List<RectTransform> pages;
    private int index = -1;
    private bool isRotatingNow = false;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) RotateNext();
        if (Input.GetKeyDown(KeyCode.P)) RotateBack();
        if (Input.GetKeyDown(KeyCode.L)) FlipThroughAlot();
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
        if (isRotatingNow || index >= pages.Count - 1) return;
        index++;
        float angle = 180f;
        pages[index].SetAsLastSibling();
        StartCoroutine(Rotate(angle, true));
    }

    public void RotateBack()
    {
        if (isRotatingNow || index <= -1) return;
        float angle = 0f;
        StartCoroutine(Rotate(angle, false));
        pages[index].SetAsLastSibling();
    }

    private IEnumerator Rotate(float angle, bool isForward)
    {
        int test = 0;
        float value = 0;

        while(true)
        {
            isRotatingNow = true;
            Quaternion targetRotation = Quaternion.Euler(0f, angle, 0f);
            value += Time.deltaTime * flipSpeed;
            pages[index].rotation = Quaternion.Slerp(pages[index].rotation, targetRotation, value);
            float angle1 = Quaternion.Angle(pages[index].rotation, targetRotation);
            int positiveMulitplier = isForward ? -1 : 1;

            if(angle1 < 0.1f)
            {
                if (!isForward)
                {
                    pages[index].anchoredPosition = Vector3.zero;
                    index--;
                }
                isRotatingNow = false;
                break;
            }
            if(isForward && angle1 < 90f)
            {
                test++;
                Vector3 currentPos = pages[index].transform.position;
                pages[index].transform.position = new Vector3(currentPos.x, currentPos.y + positiveMulitplier * verticalOffset, currentPos.z);
            }
            else if(!isForward && angle1 > 90f)
            {
                test++;
                Vector3 currentPos = pages[index].transform.position;
                pages[index].transform.position = new Vector3(currentPos.x, currentPos.y + positiveMulitplier * verticalOffset * testOffset, currentPos.z);
            }

            Debug.Log(test);
            yield return null;
        }
    }
}
