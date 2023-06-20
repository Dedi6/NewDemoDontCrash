using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;
    public GameObject playerInventory;
    public MouseItemData mouseHolder;
    public Slot[] slots;
   // public int maxStackSize;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            HandlePlayerInventory();
    }

    public void AddItemToNextSlot(Pickup itemHolder, int amount)
    {
        int nextFreeSlot = GetNextFreeSlot();
        if(nextFreeSlot != -1)
            slots[nextFreeSlot].AddItemToSlot(itemHolder, amount, nextFreeSlot);
    }

    public int GetNextFreeSlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].isFree)
                return i;
        }

        return -1;
    }


    public int GetSpecificItemSlot(Inventory_Item.Item itemToCheck)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetCurrentItem() == itemToCheck)
                return i;
        }
        return -1;
    }

    public void HandlePlayerInventory()
    {
        playerInventory.SetActive(!playerInventory.activeSelf);
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