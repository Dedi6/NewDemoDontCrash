using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class Storage_Interact : MonoBehaviour
{
    private bool isPlayerNear;
    public Inventory_Item.StorageSpace storageType;
    /*[ConditionalField(("storageType"), true, Inventory_Item.StorageSpace.Fridge)]   // only display size if not fridge
    public Vector2 sizeOfBox;*/
    
    [SerializeField]
    private GameObject dynamicInventory_prefab;
    private GameObject inventoryHolder;
    public string savePath;

    private Inventory_Base slotsBase;

    void Start()
    {
        //    inventoryHolder = transform.GetChild(0).gameObject;
       // savePath = GetComponent<UniqueID>()._id;
        inventoryHolder = InventoryController.instance.GetStorageHolder(storageType);
      /*  slotsBase = inventoryHolder.GetComponentInChildren<Inventory_Base>();
        if(storageType == Inventory_Item.StorageSpace.Fridge) 
            slotsBase.savePath = savePath;*/
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
        if (Input.GetKeyDown(KeyCode.K))
            GameSaveManager.instance.Save_Inventory_Storage(this, savePath);
        if (Input.GetKeyDown(KeyCode.J))
            GameSaveManager.instance.Load_Inventory_Storage(this, savePath);

        if (!isPlayerNear)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            HandleStorage();

        if (Input.GetKeyDown(KeyCode.Escape) && inventoryHolder.activeSelf) // works only if active
            HandleStorage();
    }


    void HandleStorage()
    {
        inventoryHolder.SetActive(!inventoryHolder.activeSelf);
        if(inventoryHolder.activeSelf)  HandleSavePaths();

        InventoryController.instance.HandleStorage_Interact(inventoryHolder.activeSelf, storageType);

        if (storageType != Inventory_Item.StorageSpace.Fridge) return;

       // Inventory_Base slotsBase = inventoryHolder.GetComponentInChildren<Inventory_Base>();

        if(inventoryHolder.activeSelf)
        {
            GameSaveManager.instance.Load_Storage_Independent(savePath, slotsBase.slots);
        }
        else if(!inventoryHolder.activeSelf)
        {
            GameSaveManager.instance.Save_Inventory(slotsBase.savePath, slotsBase);
            slotsBase.savePath = savePath;
            GameSaveManager.instance.Load_Storage_Independent(savePath, slotsBase.slots);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // here you can add an animation for interact key or anything that happens when the player is need
            isPlayerNear = true;
        }
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
