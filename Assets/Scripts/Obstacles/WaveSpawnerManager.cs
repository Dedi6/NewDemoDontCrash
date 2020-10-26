using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawnerManager : MonoBehaviour, IRespawnResetable
{
    [System.Serializable]
    public class WaveArray
    {
        public HelperArray[] arrayOfWaves;
        public float spawnAfterTimer;
        public bool dontWaitForWaveToEnd;
    }
    
    [System.Serializable]
    public class HelperArray
    {
        public GameObject enemiesToSpawnEachRound;
        public Transform spawnLocations;
    }

    public int currentWave = 0;
    public WaveArray[] Waves;
    private bool isTriggeredAlready, playerRespawning;
    public GameObject spawner;
    public GameObject currentRoom;
    public GameObject spawnAnimation;

    public UnityEngine.Events.UnityEvent ClearedWaves;
    public UnityEngine.Events.UnityEvent WavesTriggered;
    public bool loopFear = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("player") && !isTriggeredAlready)
        {
            InvokeEnemies();
            isTriggeredAlready = true;
            WavesTriggered.Invoke();
        }
    }

    public void SpawnEnemies()
    {
        for (int i = 0; i < Waves[currentWave].arrayOfWaves.Length; i++)
        {
            GameObject currentGameObject = Waves[currentWave].arrayOfWaves[i].enemiesToSpawnEachRound;
            Transform spawnLocation = Waves[currentWave].arrayOfWaves[i].spawnLocations;
            GameObject enemy = Instantiate(currentGameObject, spawnLocation.transform.position, Quaternion.identity) as GameObject;
            enemy.transform.SetParent(currentRoom.transform);
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            enemyScript.wasSpawnedBySpawnManager = true;
            enemyScript.spawnManager = gameObject;
            enemyScript.goRight = Random.Range(1, 3) == 1 ? true : false;
          //  Debug.Log(currentWave + " " + Waves[currentWave].arrayOfWaves[i].enemiesToSpawnEachRound + " " + Waves[currentWave].arrayOfWaves[i].spawnLocations);
        }
        currentWave++;
        if (currentWave < Waves.Length && Waves[currentWave].dontWaitForWaveToEnd == true)
        {
            InvokeEnemies();
        }
    }

    private IEnumerator StartSpawning(float time)
    {
        yield return new WaitForSeconds(time);
        for (int i = 0; i < Waves[currentWave].arrayOfWaves.Length; i++)
        {
            Transform spawnLocation = Waves[currentWave].arrayOfWaves[i].spawnLocations;
            AudioManager.instance.PlaySound(AudioManager.SoundList.SummonMinions);
            GameObject spawnAnim = Instantiate(spawnAnimation, spawnLocation.transform.position, Quaternion.identity) as GameObject;
            //  Debug.Log(currentWave + " " + Waves[currentWave].arrayOfWaves[i].enemiesToSpawnEachRound + " " + Waves[currentWave].arrayOfWaves[i].spawnLocations);
        }
        yield return new WaitForSeconds(0.9f);
        SpawnEnemies();
    }

    public void InvokeEnemies()
    {
        if(!loopFear)
        {
            if (currentWave < Waves.Length)
                StartCoroutine(StartSpawning(Waves[currentWave].spawnAfterTimer));
            else
                ClearedWaves.Invoke();
        }
        else
        {
            if (currentWave < Waves.Length)
                StartCoroutine(StartSpawning(Waves[currentWave].spawnAfterTimer));
            else
            {
                currentWave = 0;
                StartCoroutine(StartSpawning(Waves[0].spawnAfterTimer));
            }
        }
    }

    public void PlayerHasRespawned()
    {
        if (!playerRespawning)
        {
            playerRespawning = true;
            Invoke("PlayerRespawningCoroutine", 0.5f);
            if (!loopFear)
            {
                isTriggeredAlready = false;
                ClearedWaves.Invoke();
                currentWave = 0;
            }
            else
            {
                ClearedWaves.Invoke();
            }
        }
    }

    private void PlayerRespawningCoroutine()
    {
        playerRespawning = false;
    }
}
