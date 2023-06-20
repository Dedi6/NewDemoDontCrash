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
    [SearchableEnum]
    public StorageSpace storageIn;

    public int maxStackSize = 1;
    public bool isItemUsable;

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
    }

    public enum StorageSpace
    {
        Fridge,
        Closet, 
        Chest,
    }
}
