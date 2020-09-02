using System.Collections;
using UnityEngine;

public class ShellDragon : MonoBehaviour, ISFXResetable, IKnockbackable
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
    private float fHorizontalVelocity, rightWallXPos, leftWallXPos;
    public float pauseBeforeAttack, rockFallSpawnWidth, stompSpawnHeight, rockFallCooldown, stompCooldown, stompShakeTime, stompShakeForce, stompSpawnSpacing, stompSpeed;
    private bool  firstStomp = true;
    public GameObject rockFallingObject, stompObject;
    private Vector2 stompSpawnPos;

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
        player = GameObject.FindGameObjectWithTag("Player");
        InvokeRepeating("StateHandler", 0, 0.2f);
        GetWallPositions();
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
                    ForceFlip();
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
                if(groundCheckRaycast && (wallCheckRaycast || !endPlatformCheck))
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
                StartCoroutine(PauseBeforeAttack(pauseBeforeAttack));
            }
            else
            {
                state = State.Normal;
            }
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

    private IEnumerator CooldownCoroutine(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);

        firstStomp = true;
        InvokeRepeating("StateHandler", 0, 0.2f); // thats cooldown basically. It wont check a long as it's in cooldown. 
    }

    private IEnumerator PauseBeforeAttack(float pauseTime)
    {
        state = State.Attack;
        ForceFlip();
        GetComponent<Enemy>().canBeInterrupted = false;
        int skillRNG = Random.Range(1, 3); // range is X to Y -1 for ints.
        if (skillRNG == 1)
        {
            animator.SetTrigger("SkillStomp");
        }
        else if (skillRNG == 2)
        {
            animator.SetTrigger("SkillRock");
        }
        animator.speed = 0;
        yield return new WaitForSeconds(pauseTime);
        animator.speed = 1;
        yield return new WaitForSeconds(0.3f);
        GetComponent<Enemy>().canBeInterrupted = true;
    }
    public void StompSkill()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.MoonBossAttack);
        int sign = facingRight ? 1 : -1;
        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(stompShakeTime, stompShakeForce));
        stompSpawnPos = enemy.transform.position;
        RaycastHit2D rayToWall = Physics2D.Raycast(transform.position, raycastDirection, 100, layerMaskGround);
        GameObject stompSkill = Instantiate(stompObject, stompSpawnPos, Quaternion.identity, transform.parent);
        stompSkill.GetComponent<ShockWave>().MoveWave(sign, rayToWall.point.x, facingRight, stompSpawnSpacing);
        if (firstStomp)
            firstStomp = false;
        else
            StartCoroutine(CooldownCoroutine(stompCooldown));
    }

    private void ForceFlip()
    {
        if (player.transform.position.x > enemy.transform.position.x && !facingRight)
            Flip();
        else if (player.transform.position.x < enemy.transform.position.x && facingRight)
            Flip();
    }
    private IEnumerator StompWave(int sign, float wallXPos)
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.StompWave);
        stompSpawnPos.x += sign * stompSpawnSpacing;
        if ((stompSpawnPos.x < wallXPos && facingRight) || (stompSpawnPos.x > wallXPos && !facingRight))
        {
            yield return new WaitForSeconds(1);
            GameObject stompSkill = Instantiate(stompObject, stompSpawnPos, Quaternion.identity, transform.parent);
            StartCoroutine(StompWave(sign, wallXPos));
        }
    }

    public void RockFallSkill()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.MoonBossAttack);
        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(stompShakeTime, stompShakeForce));
        int numberOfRocks = Random.Range(15, 25);
        for (int i = numberOfRocks; i > 0; i--)
        {
            Vector2 playerPos = player.transform.position;
            if(i == numberOfRocks)
            {
                RaycastHit2D rayToCeiling = Physics2D.Raycast(playerPos, Vector2.up, 100, layerMaskGround);
                GameObject rock = Instantiate(rockFallingObject, new Vector2(rayToCeiling.point.x, rayToCeiling.point.y - 2), Quaternion.identity, transform.parent);
            }
            else
            {
                float rayXPos = Random.Range(leftWallXPos, rightWallXPos);
                RaycastHit2D rayToCeiling = Physics2D.Raycast(new Vector2(rayXPos, playerPos.y), Vector2.up, 100, layerMaskGround);
                GameObject rock = Instantiate(rockFallingObject, new Vector2(rayToCeiling.point.x, rayToCeiling.point.y - 2), Quaternion.identity, transform.parent);
            }
        }
        StartCoroutine(CooldownCoroutine(rockFallCooldown));
    }

    private void GetWallPositions()
    {
        RaycastHit2D rayToRightWall = Physics2D.Raycast(enemy.position, Vector2.right, 100, layerMaskGround);
        RaycastHit2D rayToLeftWall = Physics2D.Raycast(enemy.position, Vector2.left, 100, layerMaskGround);
        rightWallXPos = rayToRightWall.point.x;
        leftWallXPos = rayToLeftWall.point.x;
    }

    public void SetStateNormal()
    {
        if(state != State.Dead)
            state = State.Normal;
    }

    private void OnDisable()
    {
        CancelInvoke();
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
