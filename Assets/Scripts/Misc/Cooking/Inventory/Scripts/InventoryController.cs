using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;
    public GameObject playerInventoryHolder;
    public MouseItemData mouseHolder;
    public Inventory_Base playerInventory;
    public Slot[] slots;
    // public int maxStackSize;
    public Dictionary<string, Storage_Interact> storageDictionary;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            HandlePlayerInventory();

        if (Input.GetKeyDown(KeyCode.O))
            playerInventory.slots[3].ClearSlot();
    }


    public void TryToLoadPlz()
    {
        playerInventory.slots = GameSaveManager.instance.Load_Inventory("main");
      /*  if (playerInventory.slots == null)
            return;

        for (int i = 0; i < playerInventory.slots.Length; i++)
        {
           // playerInventory.slots[i].AssignItem()
        }*/

    }

    public void AddItemToNextSlot(Pickup itemHolder, int amount)
    {
        int nextFreeSlot = GetNextFreeSlot();
        if(nextFreeSlot != -1)
            playerInventory.slots[nextFreeSlot].AddItemToSlot(itemHolder, amount, nextFreeSlot);
    }

    public int GetNextFreeSlot()
    {
        for (int i = 0; i < playerInventory.slots.Length; i++)
        {
            if (playerInventory.slots[i].isFree)
                return i;
        }

        return -1;
    }


    public int GetSpecificItemSlot(Inventory_Item.Item itemToCheck, Inventory_Base inventoryToCheck)
    {
        for (int i = 0; i < inventoryToCheck.slots.Length; i++)
        {
            if (inventoryToCheck.slots[i].GetCurrentItem() == itemToCheck)
                return i;
        }
        return -1;
    }

    public void HandlePlayerInventory()
    {
        playerInventoryHolder.SetActive(!playerInventoryHolder.activeSelf);
        
        GameSaveManager.instance.Save_Inventory("main", playerInventory);
    }
}

   /* public bool IsInventoryFull()
    {
        int numOfFulls = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].isFree)
                numOfFulls++;
        }

        return numOfFulls == slots.Length;
    }*/