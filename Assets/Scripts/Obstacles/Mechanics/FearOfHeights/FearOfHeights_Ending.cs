using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearOfHeights_Ending : MonoBehaviour
{
    [HideInInspector]
    public bool isActive;

    public GameObject hiddenGround;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!isActive && (col.gameObject.layer == 11 || col.gameObject.layer == 6))
        {
            isActive = true;
            hiddenGround.SetActive(true);
        }

    }
}
