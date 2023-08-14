using UnityEngine;
using System.Collections.Generic;
using System;
using MyBox;

[System.Serializable]
public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;

    public GameObject playerInventoryHolder;
    public MouseItemData mouseHolder;
    public Inventory_Base playerInventory;

    public RectTransform _playerInventory_Placement;    // UI position for when interacting with a storage
    private Vector3 original_RectPosition;
    public GameObject fridgeHolder, chestHolder, closetHolder;

    public Item_Helper[] itemList;
    public static List<string> storage_List = new List<string>();
    private Inventory_Item.StorageSpace currentOpenedStorage;

    [System.Serializable]
    public class Item_Helper
    {
        public string name;
        [SearchableEnum]
        public Inventory_Item.Item itemFor;
        public Inventory_Item baseItem;
    }


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
        LoadPlayerInventory();
        original_RectPosition = GetComponent<RectTransform>().position;
        fridgeHolder.GetComponentInChildren<Inventory_Base>().LoadInventory();

        InitiateStorageList();
    }

    void LoadPlayerInventory()
    {
        GameSaveManager.instance.Load_Storage_Independent(playerInventory.savePath, playerInventory.slots);
    }

    private void InitiateStorageList()
    {
        if (GameSaveManager.instance.Get_Storage_List("storageList") != null)
        {
            storage_List = GameSaveManager.instance.Get_Storage_List("storageList");
            foreach (string currentId in storage_List)
            {
                Debug.Log(currentId);
            }
            return;
        }
        else
            storage_List = new List<string>();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            HandlePlayerInventory();

        if (Input.GetKeyDown(KeyCode.O))
            GameSaveManager.instance.Save_Storage_List(storage_List, "storageList");
    }


    public void AddItemToNextSlot(Pickup itemHolder, int amount)
    {
        int nextFreeSlot = GetNextFreeSlot(playerInventory.slots);
        if(nextFreeSlot != -1)
            playerInventory.slots[nextFreeSlot].AddItemToSlot(itemHolder, amount, nextFreeSlot);
    }

    public void Add_ItemToInventory(Inventory_Item itemToAdd, int amount)
    {
        Debug.Log(itemToAdd.item);
        int pointer = Get_NotFull_ItemSlot(itemToAdd.item, playerInventory);
        if(pointer != -1)
        {
            //check if full 
            bool isFull = playerInventory.slots[pointer].IsFull();
            if (!isFull)
            {
                playerInventory.slots[pointer].AddAmount(amount);
                return;
            }
        }

        int nextFreeSlot = GetNextFreeSlot(playerInventory.slots);
        if (nextFreeSlot != -1)
            playerInventory.slots[nextFreeSlot].AssignItem(itemToAdd, amount);
    }

    public int GetNextFreeSlot(Slot[] slotsToCheck) // of player Inventory
    {
        for (int i = 0; i < slotsToCheck.Length; i++)
        {
            if (slotsToCheck[i].isFree)
                return i;
        }

        return -1;  // -1 means it's full
    }

    public bool Is_PlayerInventory_Full()
    {
        return GetNextFreeSlot(playerInventory.slots) == -1;
    }


    public int GetSpecificItemSlot(Inventory_Item.Item itemToCheck, Inventory_Base inventoryToCheck)
    {
        for (int i = 0; i < inventoryToCheck.slots.Length; i++)
        {
            if (!inventoryToCheck.slots[i].isFree 
                && inventoryToCheck.slots[i].GetCurrentItem() == itemToCheck)
                return i;
        }
        return -1;  // means item doesn't exist
    }

    public bool Is_ItemInInventoryOrFridge(Inventory_Item.Item itemToCheck, int amountToCheck)
    {
        int currentAmount = 0;

        for (int i = 0; i < playerInventory.slots.Length; i++)
        {
            if (!playerInventory.slots[i].isFree
                && playerInventory.slots[i].GetCurrentItem() == itemToCheck)
                currentAmount += playerInventory.slots[i].amount;

            if (currentAmount >= amountToCheck) return true;
        }

        for (int i = 1; i <= 5; i++) // 5 is hardcoded for the amount of pages in the fridge. There is a way to do it dynamically if needed.
        {
            if (i != 1) Fridge_ChangePage(i);
            Inventory_Base slotsBase = fridgeHolder.GetComponentInChildren<Inventory_Base>();

            for (int t = 0; t < slotsBase.slots.Length; t++)
            {
                if (!slotsBase.slots[t].isFree
                    && slotsBase.slots[t].GetCurrentItem() == itemToCheck)
                    currentAmount += slotsBase.slots[t].amount;
            }
            if(currentAmount >= amountToCheck) return true;
        }

        Fridge_ChangePage(1);
        return currentAmount >= amountToCheck;
    }

    public void Remove_Items(Inventory_Item.Item itemToRemove, int amountToRemove) // ask sonreir if to remove items from player inventory or fridge first
    {
        // check if item exist before calling
        for (int i = 0; i < playerInventory.slots.Length; i++)   // cycle through player inventory
        {
            Slot currentSlot = playerInventory.slots[i];
            if (!currentSlot.isFree
                && currentSlot.GetCurrentItem() == itemToRemove)
            {
                if(amountToRemove - currentSlot.amount > 0)
                {
                    amountToRemove -= currentSlot.amount;
                    currentSlot.ClearSlot();
                }
                else
                {
                    currentSlot.RemoveFromStack(amountToRemove);
                    return;
                }
            }
        }

        for (int i = 1; i <= 5; i++) // 5 is hardcoded for the amount of pages in the fridge. There is a way to do it dynamically if needed.
        {
            if(i != 1) Fridge_ChangePage(i);
            Inventory_Base slotsBase = fridgeHolder.GetComponentInChildren<Inventory_Base>();

            for (int t = 0; t < slotsBase.slots.Length; t++)
            {
                Slot currentSlot = slotsBase.slots[t];

                if (!currentSlot.isFree
                    && currentSlot.GetCurrentItem() == itemToRemove)
                {
                    if (amountToRemove - currentSlot.amount > 0)
                    {
                        amountToRemove -= currentSlot.amount;
                        currentSlot.ClearSlot();
                    }
                    else
                    {
                        currentSlot.RemoveFromStack(amountToRemove);
                        Fridge_ChangePage(1);
                        return;
                    }
                }
            }
        }
    }

    public int Get_NotFull_ItemSlot(Inventory_Item.Item itemToCheck, Inventory_Base inventoryToCheck)
    {
        for (int i = 0; i < inventoryToCheck.slots.Length; i++)
        {
            if (!inventoryToCheck.slots[i].isFree && !inventoryToCheck.slots[i].IsFull()
                && inventoryToCheck.slots[i].GetCurrentItem() == itemToCheck)
                return i;
        }
        return -1;  // means item doesn't exist
    }

    public void HandlePlayerInventory()
    {
        if (IsStorageOpen())
            return;

        GameSaveManager.instance.Save_Inventory(playerInventory.savePath, playerInventory);
        playerInventoryHolder.SetActive(!playerInventoryHolder.activeSelf);
    }

    public void HandleStorage_Interact(bool isOpening, Inventory_Item.StorageSpace storageSpace)
    {
        RectTransform inventoryRect = GetComponent<RectTransform>();
        if (isOpening)
        {
            inventoryRect.position = _playerInventory_Placement.position;
            currentOpenedStorage = storageSpace;

            if (playerInventoryHolder.activeSelf == false)
                playerInventoryHolder.SetActive(true);
        }
        else
        {
            inventoryRect.position = original_RectPosition;
            HandlePlayerInventory();
        }
    }

    public bool IsStorageOpen()    // if the player inventory is in it's original position - the storage is closed
    {
        return GetComponent<RectTransform>().position != original_RectPosition;
    }

    public GameObject GetStorageHolder(Inventory_Item.StorageSpace storageType)
    {
        switch(storageType)     // add other holders if wanting to have different type of storage spaces.
        {
            case Inventory_Item.StorageSpace.Fridge:
                return fridgeHolder;
            case Inventory_Item.StorageSpace.Chest:
                return chestHolder;
            case Inventory_Item.StorageSpace.Closet:
                return closetHolder;
        }

        return chestHolder;
    }

    public Inventory_Item GetItem_Base(Inventory_Item.Item item)
    {
        foreach (Item_Helper currentItem in itemList)
        {
            if (currentItem.itemFor == item)
                return currentItem.baseItem;
        }
        return null;
    }

    private bool CheckIfSlotInPlayerInventory(Slot slotToCheck)
    {
        foreach (Slot currentSlot in playerInventory.slots)
        {
            if (currentSlot.Equals(slotToCheck))
                return true;
        }

        return false;
    }

    public int CtrlTrasnfer_ReturnPointer(Slot slotToCheck, out Slot[] slotsToTransferTo)
    {
        if (!CheckIfSlotInPlayerInventory(slotToCheck)) // slot clicked in storage - transfer to player inventory
        {
            slotsToTransferTo = playerInventory.slots;
            return GetNextFreeSlot(playerInventory.slots);
        }        
        else
        {
            if (!slotToCheck.currentItem.storageIn.HasFlag(currentOpenedStorage))
            {
                slotsToTransferTo = null;
                return -1;
            }

            Slot[] currentStorageSlots = GetStorageHolder(currentOpenedStorage).GetComponentInChildren<Inventory_Base>().slots;
            slotsToTransferTo = currentStorageSlots;
            return GetNextFreeSlot(currentStorageSlots);
        }
    }

    public int Get_Pointer_HalfStackedSlot(Slot[] slotsToCheck, Inventory_Item.Item itemToCheck, out int amountLeft)
    {
        for (int i = 0; i < slotsToCheck.Length; i++)
        {
            if (slotsToCheck[i].currentItem != null && !slotsToCheck[i].IsFull() && slotsToCheck[i].GetCurrentItem() == itemToCheck)
            {
                amountLeft = slotsToCheck[i].Get_AmountLeftInSlot();
                return i;
            }
        }

        amountLeft = -1;
        return -1; // doesn't exist
    }


    public bool Can_ItemBeStoredInStorage(Slot slotClicked, Inventory_Item.StorageSpace canBeStoredAt)
    {
        if (CheckIfSlotInPlayerInventory(slotClicked)) return true;

        return canBeStoredAt.HasFlag(currentOpenedStorage);
    }

    public void Fridge_ChangePage(int selectedPage)
    {
        Inventory_Base slotsBase = fridgeHolder.GetComponentInChildren<Inventory_Base>();
        string currentSaveString = slotsBase.savePath;
        Debug.Log(selectedPage + "  v " + currentSaveString);
        GameSaveManager.instance.Save_Inventory(currentSaveString, slotsBase);   // saving current page
        slotsBase.ClearAllSlots();

        string newPagePath =
            currentSaveString.Substring(0, currentSaveString.Length - 1) + selectedPage.ToString();

        Debug.Log(currentSaveString + " v " + newPagePath);
        slotsBase.savePath = newPagePath;
        GameSaveManager.instance.Load_Storage_Independent(newPagePath, slotsBase.slots);
    }
    
}

