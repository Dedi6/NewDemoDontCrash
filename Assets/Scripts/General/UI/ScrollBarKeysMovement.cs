using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ScrollBarKeysMovement : MonoBehaviour
{
    public float moveStep;
    public Scrollbar scrollBar;
    public RectTransform maskArea;
    private EventSystem evt;

    private void Start()
    {
        evt = EventSystem.current;
    }
    private void OnGUI()
    {
        if (Input.GetAxisRaw("Vertical") > 0 && scrollBar.value != 1 && !checkIfButtonIsInMask())
            MoveScrollBar(moveStep);
        else if (Input.GetAxisRaw("Vertical") < 0 && scrollBar.value != 0 && !checkIfButtonIsInMask())
            MoveScrollBar(-moveStep);
    }
    private void MoveScrollBar(float step)
    {
        scrollBar.value += step;
    }

    private bool checkIfButtonIsInMask()
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(maskArea, evt.currentSelectedGameObject.transform.position))
            return false;
        else
            return true;
    }
}
