using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDrake : MonoBehaviour, ISFXResetable, IKnockbackable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPointGround;
    private bool facingRight = false, bodyHitGround = false, delayHitGroundSFX = true;
    private RaycastHit2D wallCheckRaycast;
    private RaycastHit2D groundCheckRaycast, endPlatformCheck, rayToAttack, rayAttackCheck;
    private Vector2 raycastDirection;
    public float wallCheckDistance = 1, platformCheckDistance, attackCheckDistance, knockBackTime, turnAroundTimer;
    public float groundCheckDistance = 1;
    private int layerMaskGround = 1 << 8;
    private int layerMaskForAttacking = (1 << 8) | (1 << 11);

    [Header("Attacking")]
    [Space]
    public float fHorDmpBasic;
    public float fHorDmpTurning, speedMulitiplier;
    private float fHorizontalVelocity;
    public float attackCooldown, pauseBeforeAttack, dashSpeed, dashTimer, dashTimeLeft;
    private bool onCooldown = false;
    private int numOfDashes;
    public Transform attackGroundCheck;
    private Coroutine attackCorou;

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
        player = GameMaster.instance.playerInstance;
        speedMulitiplier += Random.Range(-1f, 1f);
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
                if (Mathf.Abs(transform.position.x - player.transform.position.x) < 8 && turnAroundTimer <= 0 && (Mathf.Abs(transform.position.y - player.transform.position.y) < 2))
                {
                    if (IsPlayerRight() && !facingRight)
                        Flip();
                    else if (!IsPlayerRight() && facingRight)
                        Flip();
                }
                break;
            case State.Dead:
                if (delayHitGroundSFX)
                    StartCoroutine(HitGroundSFX(.03f));
                if (!bodyHitGround && !delayHitGroundSFX)
                    BodyHitGround();
                break;
            case State.Attack:
                if (dashTimeLeft > 0)
                    dashTimeLeft -= Time.deltaTime;
                break;
        }

        if (turnAroundTimer > 0)
            turnAroundTimer -= Time.deltaTime;

        HandleEnemyClassObjects();
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Normal:
                StateHandler();
                Move();
                if (groundCheckRaycast && (wallCheckRaycast || !endPlatformCheck))
                    Flip();
                break;
            case State.Attack:
                Attack();
                break;
        }

        HandleRaycasts();

        //    Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //     Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //    Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, boxCollider.bounds.extents.y + groundCheckDistance, 0), Vector2.right * (boxCollider.bounds.extents.x + groundCheckDistance));
    }

    private bool IsPlayerRight()
    {
        return player.transform.position.x > enemy.transform.position.x ? true : false;
    }


    private void HandleRaycasts()
    {
        wallCheckRaycast = Physics2D.Raycast(transform.position, raycastDirection, wallCheckDistance, layerMaskGround);
        groundCheckRaycast = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(boxCollider.bounds.size.x - 0.1f, boxCollider.bounds.size.y), 0, Vector2.down, groundCheckDistance, layerMaskGround);
        endPlatformCheck = Physics2D.Raycast(rayCheckPointGround.position, Vector2.down, platformCheckDistance, layerMaskGround);
        rayToAttack = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(boxCollider.bounds.size.x - 0.1f, boxCollider.bounds.size.y), 0, raycastDirection, attackCheckDistance, layerMaskForAttacking);
        rayAttackCheck = Physics2D.Raycast(attackGroundCheck.position, Vector2.down, platformCheckDistance, layerMaskGround);
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
            if (rayToAttack.transform.gameObject.layer == 11) // 11 is player
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
        animator.speed = 1;
        GetComponent<SpriteRenderer>().enabled = true;
    }

    private IEnumerator AttackCoroutine(float cooldown)
    {
        onCooldown = true;
        attackCorou = StartCoroutine(PauseBeforeAttack());
        numOfDashes = Random.Range(1, 3);

        yield return new WaitForSeconds(cooldown);

        onCooldown = false;

    }

    private IEnumerator PauseBeforeAttack()
    {
        turnAroundTimer = 1.2f;
        Enemy enemyScript = GetComponent<Enemy>();
        enemyScript.canBeInterrupted = false;
        StartCoroutine(enemyScript.StunEnemy(pauseBeforeAttack + 0.3f));
        state = State.Stunned;
        animator.SetTrigger("Attack");
        dashTimeLeft = dashTimer;
        animator.speed = 0f;

        yield return new WaitForSeconds(pauseBeforeAttack);

        animator.speed = 1;
        AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossPrep);

        yield return new WaitForSeconds(0.3f);

        //GetComponent<Enemy>().canBeInterrupted = true;
    }
    private void Attack()
    {
        if (endPlatformCheck && dashTimeLeft > 0 && !wallCheckRaycast)
        {
            enemy.velocity = new Vector2(raycastDirection.x * dashSpeed, 0);
        }
        else
        {
            enemy.velocity = Vector2.zero;
            if(numOfDashes > 1 && state != State.Dead)
            {
                numOfDashes--;
                Flip();
                attackCorou = StartCoroutine(PauseBeforeAttack());
            }
            /*else if(numOfDashes == 0 && state != State.Dead)
                state = State.Normal;*/
        }
    }

    public void SetStateNormal()
    {
        if(state != State.Dead)
            state = State.Normal;
        GetComponent<Enemy>().canBeInterrupted = true;
    }

    public void SetStateAttacking()
    {
        if (state != State.Dead)
        {
            state = State.Attack;
            AudioManager.instance.PlaySound(AudioManager.SoundList.SwordDrakeDash);
        }
    }

    public void SetStateDead()
    {
        state = State.Dead;
        StopCoroutine(attackCorou);
        if (animator.speed != 1) animator.speed = 1;
    }

    public void DisableOtherMovement()
    {
        StartCoroutine(SetStateStunnedFor(knockBackTime));
    }

    private IEnumerator SetStateStunnedFor(float time)
    {
        state = State.Stunned;

        yield return new WaitForSeconds(time);

        if (state != State.Dead)
            state = State.Normal;
    }

    private void OnEnable()
    {
        if (state != State.Dead)
        {
            state = State.Normal;
            onCooldown = false;
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
        if (state == State.Dead)
        {
            animator.speed = 0;
            GetComponent<SpriteRenderer>().enabled = false;
        }
        else
            ResetSFXCues();
    }
}
