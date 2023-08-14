using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotBar_Handler : MonoBehaviour
{
    private int currentSelected;
    private RectTransform _rectTransform;
    private Vector3[] posArray;
    public RectTransform check;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        StartCoroutine(AssignHotbars());
    }

    void Update()
    {
        if (Input.anyKeyDown)
            CheckForInputs();
    }

    void CheckForInputs()
    {
        if (Input.GetMouseButtonDown(1)) // right click
        {
            UseHotBar();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            MoveHotbar(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            MoveHotbar(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            MoveHotbar(2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            MoveHotbar(3);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            MoveHotbar(4);
    }

    private IEnumerator AssignHotbars()
    {
        yield return 0;

        posArray = new Vector3[5]; 

        InventoryController _invCont = InventoryController.instance;
        for (int i = 0; i < 5; i++)
        {
            posArray[i] = _invCont.playerInventory.slots[i].GetComponent<RectTransform>().position;
        }
    }

    void MoveHotbar(int moveTo)
    {
        _rectTransform.position = posArray[moveTo];
        currentSelected = moveTo;
    }

    void UseHotBar()
    {
        if(!MouseItemData.IsPointerOverUIObject())
            InventoryController.instance.playerInventory.slots[currentSelected].UseItem();
    }
}
