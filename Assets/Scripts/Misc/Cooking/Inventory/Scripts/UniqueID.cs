using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
//[ExecuteInEditMode]
public class UniqueID : MonoBehaviour
{
    [HideInInspector] public string _id = Guid.NewGuid().ToString();


    private void OnValidate()
    {
        if (InventoryController.storage_List.Contains(_id)) Generate();
        else InventoryController.storage_List.Add(_id);
    }

    private void OnDestroy()
    {
        if (InventoryController.storage_List.Contains(_id)) InventoryController.storage_List.Remove(_id);
    }

    private void Generate()
    {
        _id = Guid.NewGuid().ToString();
        InventoryController.storage_List.Add(_id);
    }
}
