using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory_Base : MonoBehaviour
{
    public Inventory_Item.StorageSpace storageType;
    public string savePath;
    public Slot[] slots;

    public int[] GetItemIntArray()
    {
        int[] intArray = new int[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].currentItem != null)
                intArray[i] = (int)slots[i].currentItem.item;
            else
                intArray[i] = -1;
        }

        return intArray;
    }

    public int[] GetAmountArray()
    {
        int[] intArray = new int[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            intArray[i] = slots[i].amount;
        }

        return intArray;
    }

    public bool[] GetIsFreeArray()
    {
        bool[] boolArray = new bool[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            boolArray[i] = slots[i].isFree;
        }

        return boolArray;
    }

    public Inventory_Item[] GetItemsArray()
    {
        Inventory_Item[] itemArray = new Inventory_Item[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            itemArray[i] = slots[i].currentItem;
        }

        return itemArray;
    }


    public void ClearAllSlots()
    {
        foreach (Slot currentSlot in slots)
        {
            currentSlot.ClearSlot();
        }
    }
}
