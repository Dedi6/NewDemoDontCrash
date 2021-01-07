using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PufferFish : MonoBehaviour, ISFXResetable
{
    public float respawnTime;
    public GameObject spawnAnim;
    private Vector2 originalPos;
    Coroutine corou;

    private void Start()
    {
        originalPos = transform.position;
        if (GetComponent<Enemy>().goRight)
            Flip();
    }
    public void RespawnCoroutine()
    {
        corou = StartCoroutine(StartRespawn());
    }

    private IEnumerator StartRespawn()
    {
        GetComponent<Enemy>().Invoke("PlayerRespawned", respawnTime + 1f);
        yield return new WaitForSeconds(respawnTime);

        GameObject spawnAnimation = Instantiate(spawnAnim, originalPos, Quaternion.identity);
    }

    private void SpawnAnimation()
    {
        GameObject spawnAnimation = Instantiate(spawnAnim, originalPos, Quaternion.identity);
    }

    public void ResetSFXCues()
    {
        transform.position = originalPos;
        StopCoroutine(corou);
    }

    private void OnDisable()
    {
        transform.position = originalPos;
    }

    public void SetStateDead()
    {
        GetComponent<Enemy>().isBeingPulledDown = false;
        GetComponent<Enemy>().StopPullDownVFX();
    }

    private void Flip()
    {
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

}
