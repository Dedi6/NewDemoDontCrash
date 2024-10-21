using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowWalk : MonoBehaviour, ISFXResetable, IKnockbackable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPoint;
    private bool facingRight2 = false, bodyHitGround = false, delayHitGroundSFX = true;
    private RaycastHit2D wallCheckRaycast;
    private RaycastHit2D groundCheckRaycast, endPlatformCheck;
    private Vector2 raycastDirection;
    public float wallCheckDistance = 1, platformCheckDistance;
    public float groundCheckDistance = 1;
    private int layerMaskGround = 1 << 8;
    [SerializeField]
    private float knockBackTime = 0.1f;

    [Header("Attacking")]
    [Space]
    public float fHorDmpBasic;
    public float fHorDmpTurning, speedMulitiplier;
    private float fHorizontalVelocity, stepTimer;
    [SerializeField] private float stepTimerMax, stepForce;
    [SerializeField]
    private Vector2 boxCheckSize;
    [SerializeField] private Transform hitCheck_Pos;


    public Animator animator2;
    private BoxCollider2D boxCollider;
    GameObject player;

    private State state;

    private enum State
    {
        Normal,
        Stunned,
        Dead,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        raycastDirection = new Vector2(-1, 0);
        boxCollider = GetComponent<BoxCollider2D>();
        player = GameObject.Find("Dirt");
        bool goRight = GetComponent<Enemy>().goRight;
       /* if ((goRight && !facingRight2) || (!goRight && facingRight2))
            Flip();*/
        stepTimer = stepTimerMax;
    }

    private void Awake()
    {
        state = State.Stunned;
    }

    void Update()
    {

        switch (state)
        {
            case State.Normal:
                StateHandler();
                break;
            case State.Dead:
                if (delayHitGroundSFX)
                    StartCoroutine(HitGroundSFX(.03f));
                if (!bodyHitGround && !delayHitGroundSFX)
                    BodyHitGround();
                break;
        }

        if (stepTimer > 0)
            stepTimer -= Time.deltaTime;

        HandleEnemyClassObjects();
    }

    void FixedUpdate()
    {
        if (state == State.Normal)
        {
           // Move();
            if (groundCheckRaycast && (wallCheckRaycast || !endPlatformCheck))
                Flip();
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
        endPlatformCheck = Physics2D.Raycast(rayCheckPoint.position, Vector2.down, platformCheckDistance, layerMaskGround);
    }
    private void HandleEnemyClassObjects()
    {
        if (groundCheckRaycast)
            GetComponent<Enemy>().isEnemyGrounded = true;
        else
            GetComponent<Enemy>().isEnemyGrounded = false;

        GetComponent<Enemy>().facingRight = facingRight2;
    }
    private void StateHandler()
    {
        if (stepTimer < 0)
        {
            stepTimer = stepTimerMax;
           // state = State.Normal;
            Move_Steps();
        }


    }

    void Move_Steps()
    {
        float offset = facingRight2 ? 1 : -1;
        transform.position = new Vector2(transform.position.x + offset * stepForce, transform.position.y);
    }

    private void Flip()
    {
        facingRight2 = !facingRight2;
        transform.Rotate(0.0f, 180.0f, 0.0f);
        raycastDirection = new Vector2(-raycastDirection.x, 0);
    }


    private void Move()
    {
        //  fHorizontalVelocity = enemy.velocity.x;
        fHorizontalVelocity += facingRight2 ? 1 : -1;

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
        animator2.speed = 1;
    }

    public void SetStateDead()
    {
        state = State.Dead;
        Destroy(GetComponent<SpriteOutline>());
    }

    public void DisableOtherMovement()
    {
        StartCoroutine(KnockBackState());
    }

    private IEnumerator KnockBackState()
    {
        State currentState = state;
        state = State.Stunned;

        yield return new WaitForSeconds(knockBackTime);

        if (state != State.Dead)
            state = currentState;
    }
   public void ForceFlip(bool bossFacingRight)
    {
        if((bossFacingRight && !facingRight2) || (!bossFacingRight && facingRight2 ))
            Flip();

        StartCoroutine(WaitBeforeStart());
    }

    private IEnumerator WaitBeforeStart()
    {
        yield return new WaitForSeconds(stepTimerMax);

        state = State.Normal;
    }


    private void OnDisable()
    {
        if (state == State.Dead)
        {
            animator2.speed = 100;
            //GetComponent<SpriteRenderer>().enabled = false;
        }
        else
            ResetSFXCues();
    }

    public void CheckForHit_Explode()
    {
        Collider2D _player_Hit = Physics2D.OverlapBox(hitCheck_Pos.position, boxCheckSize, 0f ,  1 << 11);
        Debug.DrawLine(hitCheck_Pos.position, new Vector3(hitCheck_Pos.position.x + boxCheckSize.x / 2, hitCheck_Pos.position.y, hitCheck_Pos.position.z));
        Debug.Log(_player_Hit);
        if (_player_Hit != null)
            GetComponent<Enemy>().PlayerKnockBackAndDamage();
    }
}
