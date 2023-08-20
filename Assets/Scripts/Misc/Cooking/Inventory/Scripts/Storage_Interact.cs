using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage_Interact : MonoBehaviour
{
    private bool isPlayerNear;
    public Inventory_Item.StorageSpace storageType;
    
    private GameObject inventoryHolder;
    public string savePath;

    private Inventory_Base slotsBase;

    void Start()
    {
        inventoryHolder = InventoryController.instance.GetStorageHolder(storageType);
    }

    void HandleSavePaths()
    {
        if (storageType == Inventory_Item.StorageSpace.Fridge)
        {
            slotsBase = inventoryHolder.GetComponentInChildren<Inventory_Base>();
            slotsBase.savePath = savePath;
            return;
        }
        inventoryHolder.GetComponentInChildren<Dynamic_Inventory>().savePath = savePath;
    }

    void Update()
    {
        if (!isPlayerNear)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            HandleStorage();

        if (Input.GetKeyDown(KeyCode.Escape) && inventoryHolder.activeSelf) // works only if active
            HandleStorage();
    }


    void HandleStorage()
    {
        Debug.Log("can add SFX opening/ closing storage. Can be dependent on type of storage");

        inventoryHolder.SetActive(!inventoryHolder.activeSelf);
        HandleOpeningOrClosing(inventoryHolder.activeSelf);

        InventoryController.instance.HandleStorage_Interact(inventoryHolder.activeSelf, storageType);

        
        if (storageType != Inventory_Item.StorageSpace.Fridge) return;  // code from here applys for fridge

        GameSaveManager saveManager = GameSaveManager.instance;

        if(inventoryHolder.activeSelf)  // opening fridge
        {
            saveManager.Load_Storage_Independent(savePath, slotsBase.slots);
        }
        else if(!inventoryHolder.activeSelf)  // closing
        {
            saveManager.Save_Inventory(slotsBase.savePath, slotsBase);
            slotsBase.savePath = savePath;
            saveManager.Load_Storage_Independent(savePath, slotsBase.slots);
            InventoryController.instance.Revert_FridgeButtons(0);   
        }
    }

    void HandleOpeningOrClosing(bool isStorageOpen)
    {
        if (isStorageOpen)
        {
            HandleSavePaths();
            GameMaster.instance.playerInstance.GetComponent<TopDownMovement>().StartIgnoreInput();
        }
        else
            GameMaster.instance.playerInstance.GetComponent<TopDownMovement>().EndIgnoreInput();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // here you can add an animation for interact key or anything that happens when the player is need
            isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // player leaving the cooking station area
            isPlayerNear = false;
    }

    public void Save(GameDataWriter writer)
    {
        writer.Write((int)storageType); // CHANGE TO FURNITURE TYPE
        writer.Write(savePath);
        writer.Write(transform.localPosition);
        writer.Write(transform.localRotation);

    }

    public void Load(GameDataReader reader)
    {
        storageType = (Inventory_Item.StorageSpace)reader.ReadInt();
        savePath = reader.ReadString();
        transform.localPosition = reader.ReadVector3();
        transform.localRotation = reader.ReadQuaternion();
    }

}
