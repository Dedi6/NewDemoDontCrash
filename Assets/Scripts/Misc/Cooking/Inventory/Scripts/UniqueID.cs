using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
[ExecuteInEditMode]
public class UniqueID : MonoBehaviour
{
    [HideInInspector] public string _id = Guid.NewGuid().ToString();
}
