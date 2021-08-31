using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    public bool dontDealDamage;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<MovementPlatformer>().RespawnAtLatestCheckpoint();
            if(dontDealDamage)
                other.GetComponent<Health>().health++;
        }
    }
}
