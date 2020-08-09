using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<MovementPlatformer>().RespawnAtLatestCheckpoint();
        }
        else
        {
            GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>().KillBulletObject();
            PrefabManager.instance.PlayVFX(PrefabManager.ListOfVFX.BulletDissapear, other.transform.position);
        }
    }
}
