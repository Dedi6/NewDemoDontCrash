using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private Slot[] inventory;
    public Inventory_Item item;
    public int amountOfItem = 1;


    void Start()
    {
        inventory = InventoryController.instance.playerInventory.slots;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
            HandleChecks();
    }

    private void HandleChecks()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (item.maxStackSize <= 1) break;

            Slot currentSlot = inventory[i];
            if (!currentSlot.isFree && !currentSlot.IsFull()) // there's an item there and it isn't fully stacked
            {
                if (currentSlot.IsTheSameItem(item.item))
                {
                    currentSlot.AddStack(this, amountOfItem);
                    Debug.Log("can add SFX for picking up items");
                    return;
                }
            }
        }
        for (int i = 0; i < inventory.Length; i++)
        {
            Slot currentSlot = inventory[i];
            if (currentSlot.isFree)
            {
                currentSlot.AddItemToSlot(this, amountOfItem, i);
                Debug.Log("can add SFX for picking up items");
                break;
            }
        }
    }

    public void Update_Pickup(Inventory_Item newItem, int newAmount)
    {
        item = newItem;
        amountOfItem = newAmount;
        GetComponent<SpriteRenderer>().sprite = item.artwork;
    }
 }