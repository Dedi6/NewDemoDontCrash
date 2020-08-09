using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{

    private GameMaster gm;
    public GameObject respawnPosition;

    void Start()
    {
        gm = GameMaster.instance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gm.lastCheckPointPosition = respawnPosition.transform.position;
        }
    }    

}
