using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedFrog : MonoBehaviour, ISFXResetable
{
    public Rigidbody2D enemy;
    private GameObject player;
    private float jumpTimer = 2;
    private float groundCheckTimer = 1;
    public float jumpForce = 10;
    public float moveSpeed = 10;
    public int amountOfJumpsBeforeTurningMax = 3;
    public int amountOfJumpsCurrent;
    private bool facingRight = false;
    private bool checkIfHittingGround = false, bodyHitGround = false, delayHitGroundSFX = true;
    private RaycastHit2D wallCheckRaycast;
    private RaycastHit2D groundCheckRaycast;
    private Vector2 raycastDirection;
    public float wallCheckDistance = 1;
    public float groundCheckDistance = 1;
    private int layerMaskGround = 1 << 8;
    public Animator animator;
    public GameObject zoneEmitter;
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
        amountOfJumpsCurrent = amountOfJumpsBeforeTurningMax;
        raycastDirection = new Vector2(-1, 0);
        boxCollider = GetComponent<BoxCollider2D>();
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

                break;
            case State.Attacking:

                break;
            case State.Dead:
                if (delayHitGroundSFX)
                    StartCoroutine(HitGroundSFX(.03f));
                if (!bodyHitGround && !delayHitGroundSFX)
                    BodyHitGround();
                break;
        }

        HandleAnimations();
        Timers();
        HandleEnemyClassObjects();



        if (amountOfJumpsCurrent <= 0 && jumpTimer <= 0)
            Flip();
    }

    void FixedUpdate()
    {

        switch (state)
        {
            case State.Normal:
                MoveRegular();
                break;
            case State.Attacking:
                Attack();
                break;
        }


        wallCheckRaycast = Physics2D.Raycast(transform.position, raycastDirection, wallCheckDistance, layerMaskGround);
        groundCheckRaycast = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(boxCollider.bounds.size.x - 0.1f, boxCollider.bounds.size.y), 0, Vector2.down, groundCheckDistance, layerMaskGround);
        //    Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //     Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //    Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, boxCollider.bounds.extents.y + groundCheckDistance, 0), Vector2.right * (boxCollider.bounds.extents.x + groundCheckDistance));
        if (wallCheckRaycast)
        {
            enemy.velocity = new Vector2(-enemy.velocity.x, enemy.velocity.y);
            Flip();
        }
    }

    private void HandleEnemyClassObjects()
    {
        if (groundCheckRaycast)
            enemy.GetComponent<Enemy>().isEnemyGrounded = true;
        else
            enemy.GetComponent<Enemy>().isEnemyGrounded = false;

        enemy.GetComponent<Enemy>().facingRight = facingRight;

    }

    private void HandleAnimations()
    {
        if (enemy.velocity.y > 0 && !groundCheckRaycast)
            animator.SetBool("IsJumping", true);
        else if (enemy.velocity.y < 0 && !groundCheckRaycast)
            animator.SetBool("IsFalling", true);
        else if (enemy.velocity.y == 0)
            animator.SetBool("IsJumping", false);

        if (groundCheckRaycast)
            animator.SetBool("IsFalling", false);
    }

    private void StateHandler()
    {
        if (state != State.Dead)
        {
            Collider2D col = Physics2D.OverlapCircle(transform.position, 10, 1 << 11);
            if (col != null)
                state = State.Attacking;
            else
                state = State.Normal;
        }
    }
    private void Timers()
    {
        if (jumpTimer > 0)
            jumpTimer -= Time.deltaTime;

        if (checkIfHittingGround)
            StartGroundCheckTimer();
    }
    private void MoveRegular()
    {
        if (jumpTimer <= 0 && groundCheckRaycast)
        {
            JumpRegular();
            float offset = Random.Range(0.3f, 0.61f);
            jumpTimer = 2.2f + offset;
            checkIfHittingGround = true;
        }

    }
    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
        amountOfJumpsCurrent = amountOfJumpsBeforeTurningMax;
        raycastDirection = new Vector2(-raycastDirection.x, 0);
    }
    private void JumpRegular()
    {
        amountOfJumpsCurrent--;
        if (facingRight)
            enemy.velocity = new Vector2(moveSpeed, jumpForce);
        else
            enemy.velocity = new Vector2(-moveSpeed, jumpForce);
        AudioManager.instance.PlaySound(AudioManager.SoundList.FrogJump);
    }

    private void StartGroundCheckTimer()
    {
        if (groundCheckTimer > 0)
            groundCheckTimer -= Time.deltaTime;
        else
        {
            if (groundCheckRaycast)
                CheckIfHittingGround();
        }
    }

    private void CheckIfHittingGround()
    {
        enemy.velocity = new Vector2(0, 0);
        groundCheckTimer = 1;
        checkIfHittingGround = false;
        AudioManager.instance.PlaySound(AudioManager.SoundList.FrogLand);
    }

    private void Attack()
    {
        if (player.transform.position.x > enemy.transform.position.x && !facingRight)
            Flip();
        else if (player.transform.position.x < enemy.transform.position.x && facingRight)
            Flip();
        if (jumpTimer <= 0 && groundCheckRaycast)
        {
            checkIfHittingGround = true;
            AudioManager.instance.PlaySound(AudioManager.SoundList.FrogJump);
            jumpTimer = 2.5f;
            if (facingRight)
                enemy.velocity = new Vector2(Mathf.Abs(transform.position.x - player.transform.position.x), jumpForce);
            else
                enemy.velocity = new Vector2(-(Mathf.Abs(transform.position.x - player.transform.position.x)), jumpForce);
        }
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
        zoneEmitter.GetComponent<LightZone>().SetZone();
        state = State.Normal;
    }

    public void SetStateDead()
    {
        state = State.Dead;
        zoneEmitter.GetComponent<LightZone>().isDead = true;
        CancelInvoke();
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void OnEnable()
    {
        InvokeRepeating("StateHandler", 0, 0.2f);
    }
}
