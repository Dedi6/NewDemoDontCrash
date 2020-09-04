using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomManagerOne : MonoBehaviour
{
    [System.Serializable]
    public class EnemiesRespawner
    {
        public GameObject enemy;
        public bool spawnAfterTime;
    }


    public GameObject virtualCam;
    private GameObject player, bullet;
    public EnemiesRespawner[] enemiesList;
    public float spawnEnemiesAfterTime;
    private bool respawnTimerHasStarted = false;
    public float freezeWhenSwitchingRoomTime;
    [HideInInspector]
    public bool shouldFreeze = true;
    public int roomNumber;
    // private Transform

    void Start()
    {
        player = GameObject.Find("Dirt");
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(true);

            foreach (EnemiesRespawner eR in enemiesList)
            {
                if (eR.enemy.GetComponent<Enemy>().isDead == true)
                    eR.enemy.GetComponent<Enemy>().animator.SetBool("IsDead", true);
            }

            if (other.CompareTag("Player") && !other.isTrigger)
            {
                virtualCam.SetActive(true);
            }

            if (bullet != null)
                bullet.GetComponent<bullet>().SetSpeedNormal();

            player.GetComponent<MovementPlatformer>().currentRoom = this.gameObject;
            GameMaster.instance.currentRoom = gameObject;
            PrefabManager.instance.roomNumber.GetComponent<UnityEngine.UI.Text>().text = roomNumber.ToString();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            virtualCam.SetActive(false);
            StartCoroutine(FreezeGame(freezeWhenSwitchingRoomTime));
        }
        else
        {
            bullet = other.gameObject;
            other.GetComponent<bullet>().SlowBullet(2f);
        }
    }

    private IEnumerator FreezeGame(float timeToWait)
    {
        yield return new WaitForSeconds(0.1f);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        StartCoroutine(player.GetComponent<MovementPlatformer>().SwitchStateIgnore(timeToWait));
        player.GetComponent<MovementPlatformer>().animator.speed = 0;
        player.GetComponent<Footsteps>().enabled = false; 
        if(shouldFreeze)
            rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;

        yield return new WaitForSecondsRealtime(timeToWait);

        foreach (EnemiesRespawner eR in enemiesList)
        {
            if (!eR.spawnAfterTime)
                 eR.enemy.GetComponent<Enemy>().PlayerRespawned();
        }

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);

        player.GetComponent<MovementPlatformer>().animator.speed = 1;
        player.GetComponent<Footsteps>().enabled = true;
        rb.constraints = ~RigidbodyConstraints2D.FreezePosition;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        shouldFreeze = true;

        if(!respawnTimerHasStarted) StartCoroutine(RespawnEnemiesAfterTime(spawnEnemiesAfterTime));

    }

    private IEnumerator RespawnEnemiesAfterTime(float time)
    {
        respawnTimerHasStarted = true;
        yield return new WaitForSeconds(time);

        if (player.GetComponent<MovementPlatformer>().currentRoom != gameObject)
        {
            foreach (EnemiesRespawner eR in enemiesList)
            {
                eR.enemy.GetComponent<Enemy>().PlayerRespawned();
            }
            respawnTimerHasStarted = false;
           // PlayerRespawned.Invoke();
        }
        else StartCoroutine(RespawnEnemiesAfterTime(10));
    }

    public void PlayerRespawnReset2()
    {
        var objects = FindObjectsOfType<MonoBehaviour>().OfType<IRespawnResetable>();
        foreach (IRespawnResetable o in objects)
        {
            o.PlayerHasRespawned();
        }
    }
}
