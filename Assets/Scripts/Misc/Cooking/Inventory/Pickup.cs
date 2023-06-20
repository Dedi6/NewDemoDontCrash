using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private InventoryController inventory;
    public GameObject itemButton;
    public Inventory_Item item;
    public int amountOfItem = 1;

    public string itemName;

    void Start()
    {
        inventory = InventoryController.instance;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
            HandleChecks();
    }

    private void HandleChecks()
    {
        for (int i = 0; i < inventory.slots.Length; i++)
        {
            if (item.maxStackSize <= 1) break;

            Slot currentSlot = inventory.slots[i];
            if (!currentSlot.isFree && !currentSlot.IsFull()) // there's an item there and it isn't fully stacked
            {
                if (currentSlot.IsTheSameItem(item.item))
                {
                    currentSlot.AddStack(this, amountOfItem);
                    return;
                }
            }
        }
        for (int i = 0; i < inventory.slots.Length; i++)
        {
            Slot currentSlot = inventory.slots[i];
            if (currentSlot.isFree)
            {
                currentSlot.AddItemToSlot(this, amountOfItem, i);
                break;
            }
        }
    }

 }