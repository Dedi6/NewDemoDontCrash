using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBoar : MonoBehaviour, ISFXResetable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    private GameObject player;
    public float moveSpeed = 10;
    private bool facingRight = false, shouldStun = true, setStun = true;
    private bool bodyHitGround = false, delayHitGroundSFX = true;
    private RaycastHit2D wallCheckRaycast;
    private RaycastHit2D groundCheckRaycast , rayToPlayer, rayFromBack;
    private Vector2 raycastDirection;
    public float wallCheckDistance = 1, playerCheckDistance, stunTimer, stunTimerMax;
    public float groundCheckDistance = 1;
    private int layerMaskGround = 1 << 8;
    private int layerMaskPlayerAndGround = (1 << 11) | (1 << 8);

    [Header("Attacking")]
    [Space]
    public float fHorDmpBasic;
    public float fHorDmpTurning, speedMulitiplier;
    private float fHorizontalVelocity;


    public Animator animator;
    private BoxCollider2D boxCollider;

    private State state;

    private enum State
    {
        Normal,
        Attacking,
        Dead,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Dirt");
        raycastDirection = new Vector2(-1, 0);
        boxCollider = GetComponent<BoxCollider2D>();
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
                stunTimer = stunTimerMax;
                if (((rayFromBack && rayFromBack.transform.gameObject.layer == 11) || wallCheckRaycast) && groundCheckRaycast)
                    Flip();
                break;
            case State.Attacking:
                StunIfHitWall();
                break;
            case State.Dead:
                if (delayHitGroundSFX)
                    StartCoroutine(HitGroundSFX(.03f));
                if (!bodyHitGround && !delayHitGroundSFX)
                    BodyHitGround();
                break;
        }

        StateHandler();
        HandleEnemyClassObjects();
    }

    void FixedUpdate()
    {

        switch (state)
        {
            case State.Normal:
                break;
            case State.Attacking:
                Attack();
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
        rayToPlayer = Physics2D.Raycast(transform.position, raycastDirection, playerCheckDistance, layerMaskPlayerAndGround); // 11 is player's layermask
        rayFromBack = Physics2D.Raycast(transform.position, new Vector2(-raycastDirection.x, 0), playerCheckDistance, layerMaskPlayerAndGround);
    }

    private void HandleEnemyClassObjects()
    {
        if (groundCheckRaycast)
            enemy.GetComponent<Enemy>().isEnemyGrounded = true;
        else
            enemy.GetComponent<Enemy>().isEnemyGrounded = false;

        enemy.GetComponent<Enemy>().facingRight = facingRight;
    }

    private void StateHandler()
    {
        if(state != State.Dead && rayToPlayer)
        {
            if (rayToPlayer.transform.gameObject.layer == 11) // 1 << 11 = player
            {
                state = State.Attacking;
                animator.SetBool("IsAttacking", true);
                if (setStun)
                {
                    stunTimer = stunTimerMax;
                    setStun = false;
                }
            }
            else if (rayToPlayer.transform.gameObject.layer != 11 && Mathf.Abs(transform.position.x - player.transform.position.x) > 10)
            {
                state = State.Normal;
                if(enemy.velocity.x <= 0)
                   animator.SetBool("IsAttacking", false);
            }
        }
    }

    void StunIfHitWall()
    {
        if (stunTimer > 0)
            stunTimer -= Time.deltaTime;
        if (stunTimer <= 0 && wallCheckRaycast && shouldStun)
        {
            StartCoroutine(StunOnlyOnce());
            GetComponent<Enemy>().TakeDamage(35);
            state = State.Normal;
            animator.SetBool("IsAttacking", false);
        }
        else if (stunTimer > 0 && wallCheckRaycast)
            stunTimer = stunTimerMax;
    }
    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
        raycastDirection = new Vector2(-raycastDirection.x, 0);
        stunTimer = stunTimerMax;
    }


    private void Attack()
    {
        //  fHorizontalVelocity = enemy.velocity.x;
        fHorizontalVelocity += player.transform.position.x > enemy.transform.position.x ? 1 : -1;


        if (Mathf.Sign(enemy.velocity.x) != Mathf.Sign(fHorizontalVelocity))
            fHorizontalVelocity *= Mathf.Pow(1f - fHorDmpTurning, Time.deltaTime * speedMulitiplier);
        else
            fHorizontalVelocity *= Mathf.Pow(1f - fHorDmpBasic, Time.deltaTime * speedMulitiplier);

        enemy.velocity = new Vector2(fHorizontalVelocity, enemy.velocity.y);

        if (player.transform.position.x > enemy.transform.position.x && !facingRight)
            Flip();
        else if (player.transform.position.x < enemy.transform.position.x && facingRight)
            Flip();
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

    private IEnumerator StunOnlyOnce()
    {
        shouldStun = false;
        yield return new WaitForSeconds(1f);
        shouldStun = true;
        setStun = true;
    }
    private IEnumerator HitGroundSFX(float time)
    {
        yield return new WaitForSeconds(time);
        delayHitGroundSFX = false;
    }

    public void HitGroundSFXReset()
    {
        delayHitGroundSFX = true;
        bodyHitGround = false;
    }

    public void ResetSFXCues()
    {
        delayHitGroundSFX = true;
        bodyHitGround = false;
    }

    public void SetStateDead()
    {
        state = State.Dead;
    }
}
