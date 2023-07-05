using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dynamic_Inventory : MonoBehaviour
{
    public Inventory_Item.StorageSpace storageType;
    [SerializeField]
    private Vector2 slotsArraySize;
    [SerializeField]
    private GameObject slotPrefab, border;
    [SerializeField]
    private Vector2 borderOffset;

    private Inventory_Base slotsBase;

    [SerializeField]
    private string savePath;

    void Start()
    {
        int slotsAmount = (int)(slotsArraySize.x * slotsArraySize.y);
        slotsBase = gameObject.AddComponent<Inventory_Base>();
        slotsBase.slots = new Slot[slotsAmount];

        for (int i = 0; i < slotsAmount; i++)
        {
            GameObject dynamicSlot = Instantiate(slotPrefab, transform);
            slotsBase.slots[i] = dynamicSlot.GetComponent<Slot>();
        }

        slotsBase.savePath = savePath;
        HandleBorderDimensions();
    }

    void HandleBorderDimensions()
    {
        RectTransform borderRect = border.GetComponent<RectTransform>();
        RectTransform slotsRect = GetComponent<RectTransform>();
        float borderScale = borderRect.localScale.x;
        float slotsScale = slotsRect.localScale.x;
        float difference = slotsScale / borderScale;

        float cellSize = GetComponent<GridLayoutGroup>().cellSize.x;
        Vector2 slotsSize = new Vector2(cellSize * slotsArraySize.x, cellSize * slotsArraySize.y);
        slotsRect.sizeDelta = slotsSize;

        borderRect.sizeDelta = new Vector2((slotsSize.x * difference) + borderOffset.x, (slotsSize.y * difference) + borderOffset.y);
    }
    
    public void SetNewInventory()
    {

    }

    public void ClearCurrentInventory()
    {
        GameSaveManager.instance.Save_Inventory(savePath, slotsBase);
        slotsBase.ClearAllSlots();
    }
}
