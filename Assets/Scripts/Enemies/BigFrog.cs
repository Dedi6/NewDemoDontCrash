using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigFrog : MonoBehaviour, ISFXResetable, IKnockbackable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPointGround;
    private bool facingRight = false, bodyHitGround = false, delayHitGroundSFX = true, frogSpawnedAlready;
    private RaycastHit2D wallCheckRaycast;
    private RaycastHit2D groundCheckRaycast, endPlatformCheck, rayToAttack;
    private Vector2 raycastDirection;
    public float wallCheckDistance = 1, platformCheckDistance, attackCheckDistance, knockBackTime, turnAroundTimer;
    public float groundCheckDistance = 1;
    private int layerMaskGround = 1 << 8;
    private int layerMaskForAttacking = (1 << 8) | (1 << 11);
    public GameObject frogToSpawn;
    private GameObject frogParent;

    [Header("Attacking")]
    [Space]
    public float fHorDmpBasic;
    public float fHorDmpTurning, speedMulitiplier;
    private float fHorizontalVelocity;
    public Transform attackPoint;
    public float attackCooldown, pauseBeforeAttack;
    private bool onCooldown = false;


    public Animator animator;
    private BoxCollider2D boxCollider;
    GameObject player;

    private State state;

    private enum State
    {
        Normal,
        Attack,
        Stunned,
        Dead,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        raycastDirection = new Vector2(-1, 0);
        boxCollider = GetComponent<BoxCollider2D>();
        player = GameObject.Find("Dirt");
        speedMulitiplier += Random.Range(-1f, 1f);
        bool goRight = GetComponent<Enemy>().goRight;
        if ((goRight && !facingRight) || (!goRight && facingRight))
            Flip();
       // frogParent.transform.SetParent(GameMaster.instance.currentRoom.transform);
    }

    private void Awake()
    {
        state = State.Normal;
    }

    void Update()
    {

        switch (state)
        {
            case State.Normal:
                if (Mathf.Abs(transform.position.x - player.transform.position.x) < 8 && turnAroundTimer <= 0 && (Mathf.Abs(transform.position.y - player.transform.position.y) < 2))
                {
                    if (player.transform.position.x > enemy.transform.position.x && !facingRight)
                        Flip();
                    else if (player.transform.position.x < enemy.transform.position.x && facingRight)
                        Flip();
                }
                break;
            case State.Dead:
                if (delayHitGroundSFX)
                    StartCoroutine(HitGroundSFX(.03f));
                if (!bodyHitGround && !delayHitGroundSFX)
                    BodyHitGround();
                break;
        }

        if (turnAroundTimer > 0)
            turnAroundTimer -= Time.deltaTime;

        StateHandler();
        HandleEnemyClassObjects();
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Normal:
                Move();
                if (groundCheckRaycast && (wallCheckRaycast || !endPlatformCheck))
                    Flip();
                break;
            case State.Attack:
                break;
        }

        HandleRaycasts();

        //    Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //     Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //    Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, boxCollider.bounds.extents.y + groundCheckDistance, 0), Vector2.right * (boxCollider.bounds.extents.x + groundCheckDistance));
    }

    private void HandleRaycasts()
    {
        wallCheckRaycast = Physics2D.Raycast(transform.position, raycastDirection, wallCheckDistance, layerMaskGround);
        groundCheckRaycast = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(boxCollider.bounds.size.x - 0.1f, boxCollider.bounds.size.y), 0, Vector2.down, groundCheckDistance, layerMaskGround);
        endPlatformCheck = Physics2D.Raycast(rayCheckPointGround.position, Vector2.down, platformCheckDistance, layerMaskGround);
        rayToAttack = Physics2D.Raycast(transform.position, raycastDirection, attackCheckDistance, layerMaskForAttacking);
    }
    private void HandleEnemyClassObjects()
    {
        if (groundCheckRaycast)
            GetComponent<Enemy>().isEnemyGrounded = true;
        else
            GetComponent<Enemy>().isEnemyGrounded = false;

        GetComponent<Enemy>().facingRight = facingRight;
    }
    private void StateHandler()
    {
        if (rayToAttack && !onCooldown)
        {
            if (rayToAttack.transform.gameObject.layer == 11)
                StartCoroutine(AttackCoroutine(attackCooldown));
        }
    }

    private void Flip()
    {
        turnAroundTimer = 0.3f;
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
        raycastDirection = new Vector2(-raycastDirection.x, 0);
    }


    private void Move()
    {
        //  fHorizontalVelocity = enemy.velocity.x;
        fHorizontalVelocity += facingRight ? 1 : -1;

        if (Mathf.Sign(enemy.velocity.x) != Mathf.Sign(fHorizontalVelocity))
            fHorizontalVelocity *= Mathf.Pow(1f - fHorDmpTurning, Time.deltaTime * speedMulitiplier);
        else
            fHorizontalVelocity *= Mathf.Pow(1f - fHorDmpBasic, Time.deltaTime * speedMulitiplier);

        enemy.velocity = new Vector2(fHorizontalVelocity, enemy.velocity.y);
    }

    private void BodyHitGround()
    {
        if (groundCheckRaycast)
        {
            bodyHitGround = true;
            AudioManager audioManager = AudioManager.instance;
            audioManager.PlaySound(AudioManager.SoundList.EnemyHitGround);
        }
    }
    private IEnumerator HitGroundSFX(float time)
    {
        yield return new WaitForSeconds(time);
        delayHitGroundSFX = false;
    }

    public void ResetSFXCues()
    {
        delayHitGroundSFX = true;
        bodyHitGround = false;
        state = State.Normal;
        frogSpawnedAlready = false;
       // Destroy(GetComponentInChildren<FrogEnemyBasic>().gameObject);
    }

    private IEnumerator AttackCoroutine(float cooldown)
    {
        onCooldown = true;
        state = State.Attack;
        //animator.SetTrigger("Attacking");
        StartCoroutine(PauseBeforeAttack(pauseBeforeAttack));

        yield return new WaitForSeconds(cooldown);

        onCooldown = false;
    }

    private IEnumerator PauseBeforeAttack(float pauseTime)
    {
        turnAroundTimer = 1.2f;
        GetComponent<Enemy>().canBeInterrupted = false;
        animator.SetTrigger("Attacking");
        animator.speed = 0;
        yield return new WaitForSeconds(pauseTime);
        animator.speed = 1;
        yield return new WaitForSeconds(0.3f);
        GetComponent<Enemy>().canBeInterrupted = true;
    }
    public void Attack()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.BigFrogLick);
        Collider2D[] hitEnemies = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(6.7f, 2.7f), CapsuleDirection2D.Horizontal, 0, 1 << 11); //11 is player's layermask
        foreach (Collider2D player in hitEnemies)
        {
            GetComponent<Enemy>().PlayerKnockBackAndDamage();
        }
    }

    public void SetStateNormal()
    {
        state = State.Normal;
    }

    public void SetStateDead()
    {
        state = State.Dead;
    }

    public void SpawnFrog()
    {
        if(!frogSpawnedAlready)
        {
            frogSpawnedAlready = true;
            GameObject frog = Instantiate(frogToSpawn, transform.position, Quaternion.identity, frogParent.transform);
            if (GetComponent<Enemy>().wasSpawnedBySpawnManager)
            {
                frog.GetComponent<Enemy>().wasSpawnedBySpawnManager = true;
                GameObject spawner = GetComponent<Enemy>().spawnManager;
                frog.GetComponent<Enemy>().spawnManager = spawner;
            }
        }
    }

    public void DisableOtherMovement()
    {
        StartCoroutine(SetStateStunnedFor(knockBackTime));
    }

    private IEnumerator SetStateStunnedFor(float time)
    {
        state = State.Stunned;

        yield return new WaitForSeconds(time);

        state = State.Normal;
    }
    private void OnEnable()
    {
        if (state != State.Dead)
        {
            frogParent = new GameObject("FrogSpawn");
            state = State.Normal;
            onCooldown = false;
        }
    }

    private void OnDisable()
    {
        Destroy(frogParent.gameObject);
        /*
        Transform t = frogParent.transform;
        if(t.childCount > 0)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                Destroy(t.GetChild(i).gameObject);
            }
        }*/
    }
}
