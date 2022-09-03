using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour, ISFXResetable, IKnockbackable
{
    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    private bool facingRight = false;
    private Vector2 raycastDirection, originalPos;
    public float wallCheckDistance = 1, attackCheckDistance, knockBackTime;

    [Header("Attacking")]
    [Space]
    public float speedMulitiplier;
    public float playerCheckDistance, projectileSpeed;
    private float attackTimer, idleTimer;
    [SerializeField]
    Transform attackPos;
    private bool doneMoving;
    [SerializeField]
    private Transform shootPos;
    [SerializeField]
    private GameObject projectilePrefab, jumpStonePrefab;
    private GameObject cachedStone;
    private Vector2 idleTarget;

    private RaycastHit2D rayToPlayer;
    private int layerMaskPlayer = (1 << 11) | (1 << 6);


    public Animator animator;
    private BoxCollider2D boxCollider;
    GameObject player;

    private State state;

    private enum State
    {
        Normal,
        Attack,
        Idle,
        Stunned,
        Firing,
        Dead,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        originalPos = transform.position;
        player = GameMaster.instance.playerInstance;
        speedMulitiplier += Random.Range(-0.3f, 0.3f);
        if (GetComponent<Enemy>().wasSpawnedBySpawnManager)
            playerCheckDistance += 10f;

        InvokeRepeating("StateHandler", 0, 0.2f);

        bool goRight = GetComponent<Enemy>().goRight;
        if ((goRight && !facingRight) || (!goRight && facingRight))
            Flip();
    }


    private void Awake()
    {
        state = State.Normal;
        IdleSetReset();
    }

    void Update()
    {

        switch (state)
        {
            case State.Normal:
                CheckFlip();
                break;
            case State.Attack:
                if (player.transform.position.x > enemy.transform.position.x && !facingRight && (Mathf.Abs(transform.position.x - player.transform.position.x) > 0.5))
                    Flip();
                else if (player.transform.position.x < enemy.transform.position.x && facingRight && (Mathf.Abs(transform.position.x - player.transform.position.x) > 0.5))
                    Flip();
                break;
        }

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
        if(idleTimer > 0)
            idleTimer -= Time.deltaTime;

        // StateHandler();
        HandleEnemyClassObjects();
    }

    void CheckFlip()
    {
        if (enemy.transform.position.x > idleTarget.x && facingRight)
            Flip();
        else if (enemy.transform.position.x < idleTarget.x && !facingRight)
            Flip();
    }

    void FixedUpdate()
    {
        HandleRaycasts();

        switch (state)
        {
            case State.Idle:
                Idle();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    private void HandleRaycasts()
    {
        raycastDirection = player.transform.position - transform.position;
        rayToPlayer = Physics2D.Raycast(transform.position, raycastDirection, playerCheckDistance, layerMaskPlayer); // 11 is player's layermask
    }

    private void HandleEnemyClassObjects()
    {
        Enemy parentScript = GetComponent<Enemy>();
        parentScript.facingRight = facingRight;

        if (parentScript.isBeingPulledDown)
            state = State.Stunned;
        else if (!parentScript.isBeingPulledDown && state == State.Stunned)
            state = State.Normal;
    }



    private void StateHandler()
    {
        if (state == State.Normal && rayToPlayer)
        {
            state = State.Attack;
            GetComponent<MoveUpNDown>().enabled = false;
            AttackPosSet();
        }
        else if(doneMoving && state != State.Normal && !rayToPlayer)
        {
            SetStateNormal();
        }
        if (state == State.Normal && idleTimer <= 0)
        {
            state = State.Idle;
            CheckFlip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }


    private void Attack()
    {
        if(Vector2.Distance(transform.position, attackPos.transform.position) > 1f)
            transform.position = Vector2.MoveTowards(transform.position, attackPos.transform.position, speedMulitiplier * Time.deltaTime);
        else
        {
            state = State.Firing;
            animator.SetTrigger("Attack");
        }
    }
    

    private void Idle()
    {
        if (Vector2.Distance(transform.position, idleTarget) > 1f)
            transform.position = Vector2.MoveTowards(transform.position, idleTarget, speedMulitiplier * Time.deltaTime / 2f);
        else
        {
            doneMoving = true;
            state = State.Normal;
            IdleSetReset();
        }
    }

    private void IdleSetReset()
    {
        idleTimer = 4.5f;
        Vector2 newPos = (Vector2)attackPos.position + Random.insideUnitCircle * 5;
        idleTarget = newPos;
        doneMoving = false;
    }

    public void Shoot()
    {
        var pos = shootPos.position;
        var dir = player.transform.position - pos;
        GameObject projectile = Instantiate(projectilePrefab, pos, Quaternion.identity);
        projectile.GetComponent<Action_TriggerHitPlayer>().SetMovement(dir.normalized, projectileSpeed);
    }

    void AttackPosSet()
    {
        attackPos.SetParent(player.transform);
        attackPos.localPosition = Vector2.zero;
        attackPos.SetParent(transform.parent);
        attackTimer = 1.5f;
        doneMoving = false;
    }

    public void SetStateNormal()
    {
        state = State.Normal;
        GetComponent<MoveUpNDown>().enabled = true;
    }

    public void SetStateDead()
    {
        state = State.Dead;
        CancelInvoke("StateHandler");
        enemy.gravityScale = 3f;
        GetComponent<Enemy>().KnockBackEnemyHit(6f, 1f, 0.2f);
    }

    public void SpawnStone()
    {
        cachedStone = Instantiate(jumpStonePrefab, shootPos.position, Quaternion.identity);
    }

    public void ResetSFXCues()
    {
        InvokeRepeating("StateHandler", 0, 0.2f);
        state = State.Normal;
        transform.position = originalPos;
        if(cachedStone != null)
            Destroy(cachedStone);
    }

    public void DisableOtherMovement()
    {
        StartCoroutine(KnockBackState());
    }

    private IEnumerator KnockBackState()
    {
        State currentState = state;
        state = State.Dead;

        yield return new WaitForSeconds(knockBackTime);

        state = currentState;
    }
}
