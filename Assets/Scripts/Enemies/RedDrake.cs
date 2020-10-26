using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedDrake : MonoBehaviour, ISFXResetable, IKnockbackable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPointGround;
    private bool facingRight = false, bodyHitGround = false, delayHitGroundSFX = true;
    private RaycastHit2D wallCheckRaycast;
    private RaycastHit2D groundCheckRaycast, endPlatformCheck;
    private Vector2 raycastDirection;
    public float wallCheckDistance = 1, platformCheckDistance, attackCheckDistance, knockBackTime, turnAroundTimer;
    public float groundCheckDistance = 1;
    private int layerMaskGround = 1 << 8;

    [Header("Attacking")]
    [Space]
    public float fHorDmpBasic;
    public float fHorDmpTurning, speedMulitiplier;
    private float fHorizontalVelocity;
    public Transform attackPoint;
    public float attackCooldown, pauseBeforeAttack, spreadAngle, fireballMoveSpeed;
    public int numOfFireballs;
    public GameObject fireball;


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
        InvokeRepeating("StateHandler", 0, 0.2f);
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
        if (state != State.Dead)
        {
            Collider2D col = Physics2D.OverlapCircle(transform.position, attackCheckDistance, 1 << 11);
            if (col != null && col.transform.gameObject.layer == 11) // 11 is player
            {
                CancelInvoke();
                StartCoroutine(AttackCoroutine(attackCooldown));
            }
            else
                state = State.Normal;
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
    }

    private IEnumerator AttackCoroutine(float cooldown)
    {
        state = State.Attack;
        if (state != State.Dead)    
            StartCoroutine(PauseBeforeAttack(pauseBeforeAttack));

        yield return new WaitForSeconds(cooldown);

        if(state != State.Dead)
           InvokeRepeating("StateHandler", 0, 0.2f);
    }

    private IEnumerator PauseBeforeAttack(float pauseTime)
    {
        turnAroundTimer = 1.2f;
        GetComponent<Enemy>().canBeInterrupted = false;
        if (player.transform.position.x > enemy.transform.position.x && !facingRight)
            Flip();
        else if (player.transform.position.x < enemy.transform.position.x && facingRight)
            Flip();
        animator.SetTrigger("Attacking");
        animator.speed = 0;
        yield return new WaitForSeconds(pauseTime);
        animator.speed = 1;
        yield return new WaitForSeconds(0.3f);
        GetComponent<Enemy>().canBeInterrupted = true;
    }
    public void Attack()
    {
        float angleStep = spreadAngle / numOfFireballs;
        float angle = 0f;
        int sign = facingRight ? 1 : -1;
        AudioManager.instance.PlaySound(AudioManager.SoundList.RedDrakeFireBall);
        for (int i = 0; i < numOfFireballs; i++)
        {
            Vector2 atkPoint = new Vector2(attackPoint.transform.position.x, attackPoint.transform.position.y);
            float dirX = atkPoint.x + Mathf.Cos((angle * Mathf.PI) / 180) * 1;
            float diry = atkPoint.y + Mathf.Sin((angle * Mathf.PI) / 180) * 1;
            Vector2 projectileVector = new Vector2(dirX, diry);
            Vector2 direction = (projectileVector - atkPoint).normalized * fireballMoveSpeed;
            float rotation= Mathf.Atan2(-direction.y, -direction.x * sign) * Mathf.Rad2Deg;
            GameObject projectile = Instantiate(fireball, atkPoint, Quaternion.AngleAxis(rotation, Vector3.forward), transform.parent);
            projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(direction.x * sign, direction.y);

            angle += angleStep;
        }
        if (state != State.Dead)
            state = State.Normal;
    }


    public void SetStateNormal()
    {
        if (state != State.Dead)
            state = State.Normal;
    }

    public void SetStateDead()
    {
        state = State.Dead;
        CancelInvoke();
    }
    void OnEnable()
    {
        InvokeRepeating("StateHandler", 0, 0.2f);
    }

    private void OnDisable()
    {
        CancelInvoke();
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
}
