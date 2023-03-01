using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    public bool dontDealDamage;
    public float _xSize;
    private void Start()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.size = new Vector2(_xSize - 0.2f, col.size.y);
    }

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
