using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Inventory_Item : ScriptableObject
{
    public string itemName;

    public Sprite artwork;
    
    [SearchableEnum]
    public Item item;
    public StorageSpace storageIn;

    public int maxStackSize = 1;
    public bool isItemUsable;

    [Header("Attributes")]
    public int energyRestored;
    public int sellValue;

    public enum Item
    {
        Rice,
        Seaweed,
        Veggies,
        Beef,
        Pork,
        Tuna,
        Tofu,
        Fishing_Rod,
        Gimbap_Beef,
        Gimbap_Pork,
        Gimbap_Tuna,
        Gimbap_Tofu,
    }

    [System.Flags]
    public enum StorageSpace
    {
        PlayerInventory = 1 << 1,
        Fridge = 1 << 2,
        Closet = 1 << 3, 
        Chest = 1 << 4,
    }
}
