using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class MouseItemData : MonoBehaviour
{
    public Image itemSprite;
    public TextMeshProUGUI itemCount;
    public Slot inventorySlot;
    [HideInInspector]
    public int currentAmount;
    [HideInInspector]
    public Inventory_Item currentItem;

    private void Awake()
    {
        itemSprite.color = Color.clear;
        itemCount.text = " ";
    }

    public void UpdateMouseSlot(Slot invSlot)
    {
        inventorySlot = invSlot;
        currentItem = invSlot.currentItem;
        itemSprite.sprite = currentItem.artwork;
        UpdateAmount(invSlot.amount);
        itemSprite.color = Color.white;
    }

    public void UpdateAmount(int newAmount)
    {
        currentAmount = newAmount;
        if (currentAmount > 1)
            itemCount.text = currentAmount.ToString();
        else
            itemCount.text = " ";
    }

    private void Update()
    {
        if (inventorySlot == null)
            return;

        transform.position = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUIObject())   // dropping - touching somewhere with no UI
            ClearSlot();
    }

    public void ClearSlot()
    {
     //   inventorySlot.ClearSlot();
        itemCount.text = " ";
        itemSprite.color = Color.clear;
        itemSprite.sprite = null;
        inventorySlot = null;
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Mouse.current.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
