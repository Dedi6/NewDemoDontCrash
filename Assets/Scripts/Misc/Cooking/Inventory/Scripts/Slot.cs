using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Slot : MonoBehaviour
{
    public TextMeshProUGUI amountText;
    public int amount;
    public bool isFree = true;
    [HideInInspector]
    public Inventory_Item currentItem;
    private int pointer;


    public bool IsFull()
    {
        return amount >= currentItem.maxStackSize;
    }

    public void AddStack(Pickup itemHolder, int amountToAdd)
    {
        itemHolder.amountOfItem = amountToAdd + amount - currentItem.maxStackSize;
       // itemHolder.amountOfItem = amountToAdd + amount - InventoryController.instance.maxStackSize;
        amount += amountToAdd;
        GetComponentInChildren<TextMeshProUGUI>().text = (amount).ToString();
        CheckIfFull(itemHolder);
    }

    private void RemoveFromStack(int amountToRemove)
    {
        amount -= amountToRemove;
        HandleAmountText();
        if (amount <= 0)
            ClearSlot();
    }

    public void AddItemToSlot(Pickup itemHolder, int amountToAdd, int _pointer)
    {
        AssignItem(itemHolder.item, amountToAdd);
        pointer = _pointer;
        //itemHolder.amountOfItem -= InventoryController.instance.maxStackSize;
        itemHolder.amountOfItem -= currentItem.maxStackSize;
        CheckIfFull(itemHolder);
    }

    private void AssignItem(Inventory_Item item, int amountToAdd)
    {
        isFree = false;
        currentItem = item;
        amount += amountToAdd;
        currentItem.item = item.item;
        Image _image = transform.GetChild(0).GetComponent<Image>();
        _image.sprite = currentItem.artwork;
        _image.color = Color.white;

        if (amount > 1 && amount <= currentItem.maxStackSize)
            GetComponentInChildren<TextMeshProUGUI>().text = (amount).ToString();
    }

    public void UpdateSlot(Inventory_Item item, int amountToAdd)
    {
        ClearSlot();
        AssignItem(item, amountToAdd);
    }

    private void CheckIfFull(Pickup itemHolder)
    {
        if (IsFull())
            FillAmount(itemHolder);
        else
            Destroy(itemHolder.gameObject);
    }

    
    public bool IsTheSameItem(Inventory_Item.Item itemToCheck)
    {
        return currentItem.item == itemToCheck;
    }

    private void FillAmount(Pickup itemHolder)
    {
        int excessAmount = amount - currentItem.maxStackSize;
        amount = currentItem.maxStackSize;
        HandleAmountText();

        if(excessAmount > 0)
            InventoryController.instance.AddItemToNextSlot(itemHolder, excessAmount);
        else
            Destroy(itemHolder.gameObject);
    }

    void HandleAmountText()
    {
        TextMeshProUGUI amountText = GetComponentInChildren<TextMeshProUGUI>();
        if (amount > 1)
            amountText.text = (amount).ToString();
        else
            amountText.text = " ";
    }

    public Inventory_Item.Item GetCurrentItem()
    {
        return currentItem.item;
    }


    public void ClearSlot()
    {
        currentItem = null;
        isFree = true;
        GetComponentInChildren<TextMeshProUGUI>().text = " ";
        amount = 0;
        transform.GetChild(0).GetComponent<Image>().color = Color.clear;
    }

    public void MergeItems(Slot invSlot)
    {
        //if(currentItem == invSlot.currentItem) AddStack()
    }


    public void SlotClicked()
    {
        MouseItemData mouseHolder = InventoryController.instance.mouseHolder;
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);

        if (currentItem != null && mouseHolder.inventorySlot == null)   // slot occuiped, mouse empty
        {
            if(isShiftPressed && amount > 1)
            {
                SplitStack(mouseHolder);
                return;
            }   

            mouseHolder.UpdateMouseSlot(this);
            ClearSlot();
            return;
        }

        if (currentItem == null && mouseHolder.inventorySlot != null)   // slot empty, mouse occupied
        {
            AssignItem(mouseHolder.currentItem, mouseHolder.currentAmount);
            mouseHolder.ClearSlot();

            if(!isShiftPressed)
                return;
            else if(amount > 1)
                SplitStack(mouseHolder);
            return;
        }

        if (currentItem != null && mouseHolder.inventorySlot != null)   // both have items
        {
            if(currentItem.item == mouseHolder.currentItem.item)     // same item 
            {
                if (IsFull())
                {
                    SwapSlots(mouseHolder);
                    return;
                }
                else
                {
                    int sum = amount + mouseHolder.currentAmount;
                    if (sum > currentItem.maxStackSize)
                    {
                        amount = currentItem.maxStackSize;
                        GetComponentInChildren<TextMeshProUGUI>().text = (amount).ToString();
                        int mouseUpdateInt = sum - currentItem.maxStackSize;
                        mouseHolder.UpdateAmount(mouseUpdateInt);
                    }
                    else
                    {
                        amount = sum;
                        HandleAmountText();
                        mouseHolder.ClearSlot();
                    }
                }
            }

            if (currentItem.item != mouseHolder.currentItem.item)   // not the same item
                SwapSlots(mouseHolder);
        }
    }

    void SplitStack(MouseItemData mouseHolder)
    {
        int halfStack = Mathf.RoundToInt(amount / 2);
        RemoveFromStack(halfStack);
        mouseHolder.UpdateMouseSlot(this);
        mouseHolder.UpdateAmount(halfStack);
    }

    private void SwapSlots(MouseItemData mouseHolder)
    {
        Inventory_Item mouseItem = mouseHolder.currentItem;
        int mouseAmount = mouseHolder.currentAmount;
        mouseHolder.UpdateMouseSlot(this);
        ClearSlot();
        AssignItem(mouseItem, mouseAmount);
    }

}
