using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonoBehaviour, ISFXResetable, IKnockbackable
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
    public float playerCheckDistance, ceilingCheckDistance;
    private bool wasTriggered = false;

    private RaycastHit2D rayToPlayer, rayToCeiling, rayToFloor;
    private int layerMaskPlayer = (1 << 11) | (1 << 6);

    public PhysicsMaterial2D bouncyMaterial;


    public Animator animator;
    private BoxCollider2D boxCollider;
    GameObject player;

    private State state;

    private enum State
    {
        Normal,
        Attack,
        Summoning,
        Stunned,
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
    }

    void Update()
    {

        switch (state)
        {
            case State.Normal:
                if (enemy.transform.position.x > originalPos.x && facingRight)
                    Flip();
                else if (enemy.transform.position.x < originalPos.x && !facingRight)
                    Flip();
                break;
            case State.Attack:
                if (player.transform.position.x > enemy.transform.position.x && !facingRight && (Mathf.Abs(transform.position.x - player.transform.position.x) > 0.5))
                    Flip();
                else if (player.transform.position.x < enemy.transform.position.x && facingRight && (Mathf.Abs(transform.position.x - player.transform.position.x) > 0.5))
                    Flip();
                break;
            case State.Dead:
                break;
        }

        // StateHandler();
        HandleEnemyClassObjects();
    }

    void FixedUpdate()
    {
        HandleRaycasts();

        switch (state)
        {
            case State.Normal:
                if (!rayToCeiling)
                    Idle();
                else
                {
                    animator.SetBool("IsFlying", false);
                    enemy.velocity = Vector2.zero;
                }
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
        rayToCeiling = Physics2D.Raycast(transform.position, Vector2.up, ceilingCheckDistance, 1 << 8); // 8 is ground
        rayToFloor = Physics2D.Raycast(transform.position, Vector2.down, ceilingCheckDistance, 1 << 8); // 8 is ground
    }

    private void HandleEnemyClassObjects()
    {
        Enemy parentScript = GetComponent<Enemy>();
        parentScript.facingRight = facingRight;

        if (parentScript.isBeingPulledDown)
            state = State.Stunned;
        else if(!parentScript.isBeingPulledDown && state == State.Stunned)
            state = State.Normal;

    }

    

    private void StateHandler()
    {
        if (rayToPlayer)
            state = State.Attack;
        else
        {
            state = State.Normal;
            if (wasTriggered)
                wasTriggered = false;
        }

        if (state == State.Attack && !wasTriggered)
        {
            wasTriggered = true;
            enemy.velocity = Vector2.zero;
            animator.SetBool("IsFlying", true);
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }


    private void Attack()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speedMulitiplier * Time.deltaTime);
    }

    private void Idle()
    {
        transform.position = Vector2.MoveTowards(transform.position, originalPos, speedMulitiplier * Time.deltaTime);

    }


    public void SetStateNormal()
    {
        state = State.Normal;
    }

    public void SetStateDead()
    {
        state = State.Dead;
        CancelInvoke("StateHandler");
        enemy.gravityScale = 3f;
        GetComponent<Enemy>().KnockBackEnemyHit(6f, 1f, 0.2f);
        enemy.sharedMaterial = bouncyMaterial;
    }


    public void ResetSFXCues()
    {
        transform.position = originalPos;
        enemy.gravityScale = 0f;
        InvokeRepeating("StateHandler", 0, 0.2f);
        state = State.Normal;
        enemy.sharedMaterial = null;
        animator.speed = 1;
        GetComponent<SpriteRenderer>().enabled = true;
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

    private void OnDisable()
    {
        if (state == State.Dead)
        {
            animator.speed = 100;
           // GetComponent<SpriteRenderer>().enabled = false;
        }
        else
            ResetSFXCues();
    }
}
