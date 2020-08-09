using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class VerticalMenuNavigation : MonoBehaviour
{
    public float moveStep;
    public RectTransform maskArea;
    public GameObject menuAssets;
    private EventSystem evt;

    private void Start()
    {
        evt = EventSystem.current;
    }
    private void OnGUI()
    {
        if (Input.GetAxisRaw("Vertical") > 0  && !checkIfButtonIsInMask())
            MoveUI(-moveStep);
        else if (Input.GetAxisRaw("Vertical") < 0  && !checkIfButtonIsInMask())
            MoveUI(moveStep);
    }
    private void MoveUI(float step)
    {
        menuAssets.transform.position = new Vector2(menuAssets.transform.position.x, menuAssets.transform.position.y + step);
    }

    private bool checkIfButtonIsInMask()
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(maskArea, evt.currentSelectedGameObject.transform.position))
            return false;
        else
            return true;
    }

}
