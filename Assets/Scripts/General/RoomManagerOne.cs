using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyBox;

public class RoomManagerOne : MonoBehaviour
{
    [System.Serializable]
    public class EnemiesRespawner
    {
        public GameObject enemy;
        public bool spawnAfterTime;
    }

    public bool respawnEnemiesOnRespawn = true;
    public GameObject virtualCam;
    private GameObject player, bullet;
    public EnemiesRespawner[] enemiesList;
    public float spawnEnemiesAfterTime;
    private bool respawnTimerHasStarted = false, delayingNow;
    public float freezeWhenSwitchingRoomTime;
    [HideInInspector]
    public bool shouldFreeze = true;
    public int roomNumber;
    public bool changeRenderer;
    [ConditionalField("changeRenderer")] public int rendererIndex;
    // private Transform
    private Coroutine corou;

    void Start()
    {
        player = GameMaster.instance.playerInstance;
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
                if (delayingNow)
                    StopCoroutine(corou);
            }

            if (bullet != null)
                bullet.GetComponent<bullet>().SetSpeedNormal();

            ChangeRendererIndex();      // set the correct renderer data index
                        
            MovementPlatformer playerScript = player.GetComponent<MovementPlatformer>();
            playerScript.currentRoom = this.gameObject;
            playerScript.SetBulletSpeedNormal();
            GameMaster.instance.currentRoom = gameObject;
            PrefabManager.instance.roomNumber.GetComponent<UnityEngine.UI.Text>().text = roomNumber.ToString();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            virtualCam.SetActive(false);
            corou = StartCoroutine(FreezeGame(freezeWhenSwitchingRoomTime));
            CellOrganizer.instance.ReleaseAll();
        }
        else
        {
            bullet = other.gameObject;

            if (other.TryGetComponent(out bullet b))
                b.SlowBullet(2f);
            else if(other.TryGetComponent(out BulletGhost bG))
                bG.DestroyBulletAndReset();
        }
    }

    private IEnumerator FreezeGame(float timeToWait)
    {
        delayingNow = true;

        yield return new WaitForSeconds(0.1f);
        delayingNow = false;

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
        /*if(respawnEnemiesOnRespawn)       // this only happens for if respawn enemies
        {
            var objects = FindObjectsOfType<MonoBehaviour>().OfType<IRespawnResetable>();
            CellOrganizer.instance.ReleaseAll();
            foreach (IRespawnResetable o in objects)
            {
                o.PlayerHasRespawned();
            }
        }*/
        var objects = FindObjectsOfType<MonoBehaviour>().OfType<IRespawnResetable>();
        CellOrganizer.instance.ReleaseAll();
        foreach (IRespawnResetable o in objects)
        {
            o.PlayerHasRespawned();
        }
    }

    void ChangeRendererIndex()
    {
        UnityEngine.Rendering.Universal.UniversalAdditionalCameraData additionalCameraData = Camera.main.transform.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (changeRenderer)
            additionalCameraData.SetRenderer(rendererIndex);
        else
            additionalCameraData.SetRenderer(1);
    }
}
