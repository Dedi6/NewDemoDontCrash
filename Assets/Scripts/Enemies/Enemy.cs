using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IRespawnResetable
{
    public int maxHealth = 100;
    public int currentHealth;
    public Animator animator;
    public ParticleSystem particle;
    private GameObject player;
    private GameObject enemyGameObject;
    public Rigidbody2D enemy;
    [HideInInspector]
    public Vector2 originalPos;
    public float knockBackWhenHit;
    public float knockBackWhenDie;
    public float pullDownForce, stunTimeFromAir = 0.3f, StunTimeGrounded = 3f;
    [HideInInspector]
    public bool isEnemyGrounded, isBeingPulledDown = false, facingRight, isDead = false, wasSpawnedBySpawnManager = false, outlineOn = false, usePhases = false;
    [HideInInspector]
    public GameObject spawnManager;
    private bool wasInAirWhenPulledDown;
    public BoxCollider2D triggerCollider;
    public bool knockBackInterface, canBeInterrupted = true, goRight;
    AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.instance;
        currentHealth = maxHealth;
        player = GameObject.Find("Dirt");
        enemy = GetComponent<Rigidbody2D>();
        originalPos = transform.position;
        SetParticleSystem();
    }

    // Update is called once per frame

    void FixedUpdate()
    {
        if (isBeingPulledDown)
            LowerEnemyToTheGround();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if(canBeInterrupted)
            animator.SetTrigger("Hurt");

       
        if(currentHealth > 0)
        {
            audioManager.PlaySound(AudioManager.SoundList.EnemyHit);
            KnockBackEnemyHit(knockBackWhenHit, 1.5f, 1);
        }
        else
        {
            audioManager.PlaySound(AudioManager.SoundList.EnemyDie);
            enemyDead();
        }

        if (usePhases)
            GetComponent<IPhaseable<float>>().HandlePhases(currentHealth);
    }

    public void KnockBackEnemyHit(float forceInt, float x, float y)
    {
        if(knockBackInterface && !isDead)
             GetComponent<IKnockbackable>().DisableOtherMovement();
        if (player.transform.position.x > enemy.transform.position.x)
            enemy.AddForce(new Vector2(-x, y) * forceInt, ForceMode2D.Impulse);
        else
            enemy.AddForce(new Vector2(x, y) * forceInt, ForceMode2D.Impulse);
    }


    public void Highlight()
    {
        if (!outlineOn)
        {
            this.GetComponent<SpriteOutline>().enabled = true;
            outlineOn = true;
        }
        else
        {
            this.GetComponent<SpriteOutline>().enabled = false;
            outlineOn = false;
        }
    }

    public void LowerEnemyToTheGround()
    {
        StartPullDownVFX();
        if (!isEnemyGrounded)
        {
            enemy.velocity = new Vector2(0, -pullDownForce);
            wasInAirWhenPulledDown = true;
        }
        else
        {
            if (wasInAirWhenPulledDown)
            {
                StartCoroutine(StunEnemy(stunTimeFromAir));
                wasInAirWhenPulledDown = false;
            }
            else
                StartCoroutine(StunEnemy(StunTimeGrounded));

            isBeingPulledDown = false;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("bullet"))
        {
            player.GetComponent<MovementPlatformer>().CurrentBulletGameObject = gameObject;
            player.GetComponent<MovementPlatformer>().didHitAnEnemy = true;
            Highlight();
            GetComponent<SpriteRenderer>().sortingOrder = 1;
            player.GetComponent<MovementPlatformer>().UnHightLightEnemies();
            audioManager.PlaySound(AudioManager.SoundList.BulletHitEnemy);
            Destroy(col.gameObject);
        }
        if (col.gameObject.layer == LayerMask.NameToLayer("player"))
            PlayerKnockBackAndDamage();
    }

    public void PlayerKnockBackAndDamage()
    {
        if (player.transform.position.x > enemy.transform.position.x)
            player.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, true);
        else
            player.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, false);
        player.GetComponent<MovementPlatformer>().GotHitByAnEnemy(1);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("player"))
        {
            if (player.transform.position.x > enemy.transform.position.x)
                player.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, true);
            else
                player.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, false);
            player.GetComponent<MovementPlatformer>().GotHitByAnEnemy(1);

        }
    }

    public void PlayerRespawned()
    {
        enemy.velocity = Vector2.zero;
        transform.position = originalPos;
        currentHealth = maxHealth;
        if(isDead)
        {
            isDead = false;
            animator.SetBool("IsDead", false);
            triggerCollider.enabled = true; 
            gameObject.layer = LayerMask.NameToLayer("enemy");
            gameObject.tag = "EnemyAlive";
            GetComponent<ISFXResetable>().ResetSFXCues();
        }
    }
    void enemyDead()
    {
        isDead = true;
        KnockBackEnemyHit(knockBackWhenDie, 1.5f, 1);
        animator.SetBool("IsDead", true);
        triggerCollider.enabled = false;
        GetComponent<ISFXResetable>().SetStateDead();
        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
        gameObject.tag = "EnemyDead";

        if (wasSpawnedBySpawnManager && (GameObject.FindGameObjectsWithTag("EnemyAlive").Length == 0))
        {
            spawnManager.GetComponent<WaveSpawnerManager>().InvokeEnemies();
        }

    }

    private void OnDisable()
    {
        if (wasSpawnedBySpawnManager)
            Destroy(enemy.gameObject);
        if (!isDead && (triggerCollider.enabled == false))
            triggerCollider.enabled = true;
    }
    public IEnumerator StunEnemy(float stunDuration) //Don't forget StartCoroutine.
    {
        enemy.velocity = Vector2.zero;
        enemy.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        yield return new WaitForSeconds(stunDuration);
        enemy.constraints = ~RigidbodyConstraints2D.FreezePosition;
        enemy.constraints = RigidbodyConstraints2D.FreezeRotation;
        StopPullDownVFX();
    }

    void SetParticleSystem()
    {
        UnityEngine.ParticleSystem.ShapeModule shape = particle.shape;
        float sizeX = GetComponent<BoxCollider2D>().bounds.size.x;
        shape.scale = new Vector3(sizeX, shape.scale.y, shape.scale.z);
    }
    void StartPullDownVFX()
    {
        particle.Play();
    }
    public void StopPullDownVFX()
    {
        particle.Stop();
    }


    public void ToggleTriggerCollider()
    {
        bool b = triggerCollider.enabled;
        triggerCollider.enabled = !b;
    }

    public void PlayerHasRespawned()
    {
        PlayerRespawned();
    }

    public void PlayerTeleportedToEnemy()
    {
        Highlight();
        TakeDamage(20);
        GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

}
