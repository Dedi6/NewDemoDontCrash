using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class FallZone : MonoBehaviour
{
    public bool isOnFearOfHeights;
    [ConditionalField("isOnFearOfHeights")] public LayerSwitcher layerSwitcher;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isOnFearOfHeights)
                StartCoroutine(DelaySwitching());
            other.GetComponent<MovementPlatformer>().RespawnAtLatestCheckpoint();
        }
        else
        {
            GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>().KillBulletObject();
            PrefabManager.instance.PlayVFX(PrefabManager.ListOfVFX.BulletDissapear, other.transform.position);
        }
    }

    private IEnumerator DelaySwitching()
    {
        yield return new WaitForSeconds(0.5f);

        layerSwitcher.PlayerRespawned();
    }
}
