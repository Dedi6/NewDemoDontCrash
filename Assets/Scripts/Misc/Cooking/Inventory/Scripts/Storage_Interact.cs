using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage_Interact : MonoBehaviour
{
    private bool isPlayerNear;
    public Inventory_Item.StorageSpace storageType;
    [SerializeField]
    private GameObject dynamicInventory_prefab;
    private GameObject inventoryHolder;
    public string savePath;

    void Start()
    {
        //    inventoryHolder = transform.GetChild(0).gameObject;
        savePath = GetComponent<UniqueID>()._id;
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
            inventoryHolder.SetActive(!inventoryHolder.activeSelf);


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

    private void HandleDynamicInventory()
    {

    }

    public void Save(GameDataWriter writer)
    {
        writer.Write((int)storageType);
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
