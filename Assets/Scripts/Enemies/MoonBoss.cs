using System.Collections;
using UnityEngine;

public class MoonBoss : MonoBehaviour, ISFXResetable, IKnockbackable, IPhaseable<float>, IRespawnResetable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPointGround;
    private bool facingRight = false, playerRespawned;
    private RaycastHit2D wallCheckRaycast;
    private RaycastHit2D groundCheckRaycast;
    private Vector2 raycastDirection;
    public float wallCheckDistance = 1, attackCheckDistance, knockBackTime, turnAroundTimer;
    public float groundCheckDistance = 1, stallAnimation, phaseTwoHp, phaseThreeHp;
    private int layerMaskGround = 1 << 8, currentPhase = 1;
    private Collider2D waitingCast;

    [Header("Attacking")]
    [Space]
    public float fHorDmpBasic;
    public float fHorDmpTurning, speedMulitiplier;
    private float fHorizontalVelocity, rightWallXPos, leftWallXPos;
    public float pauseBeforeAttack, thunderFallSpawnWidth, warningThunderTime, stompSpawnHeight, waveCooldown, scatterCooldown, stompShakeTime, stompShakeForce, waveSpawnSpacing, waveSpeed, runningCooldown;
    public GameObject warningThunder, thunderObject;
    private Vector2 thunderSpawnPos;
    public int numberOfThunders;
    private bool isWave;
    private float[] arrayOfPosition;

    public Animator animator;
    private BoxCollider2D boxCollider;
    public UnityEngine.Events.UnityEvent bossFightTriggered, bossDied, playerDied;
    public GameObject spellUnlockObject;
    GameObject player;

    private State state;

    private enum State
    {
        Waiting,
        Normal,
        Attack,
        Running,
        Stunned,
        Dead,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        raycastDirection = new Vector2(-1, 0);
        boxCollider = GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        GetWallPositions();
        arrayOfPosition = new float[numberOfThunders];
        GetComponent<Enemy>().usePhases = true;
        bool goRight = GetComponent<Enemy>().goRight;
        if ((goRight && !facingRight) || (!goRight && facingRight))
            Flip();
    }

    private void Awake()
    {
        if(state != State.Dead)
            state = State.Waiting;
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
            case State.Running:
                if(wallCheckRaycast)
                {
                    HitWall();
                }
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
            case State.Waiting:
                CheckForPlayer();
                break;
            case State.Normal:
                StartCoroutine(PauseBeforeAttack(pauseBeforeAttack));
                break;
            case State.Running:
                Move();
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
    }

    private void CheckForPlayer()
    {
        waitingCast = Physics2D.OverlapCircle(transform.position, attackCheckDistance, 1 << 11);
        if (!playerRespawned && waitingCast != null && waitingCast.transform.gameObject.layer == 11) // 11 is player
        {
            state = State.Normal;
            bossFightTriggered.Invoke();
        }
    }
    private void HandleEnemyClassObjects()
    {
        if (groundCheckRaycast)
            GetComponent<Enemy>().isEnemyGrounded = true;
        else
            GetComponent<Enemy>().isEnemyGrounded = false;

        GetComponent<Enemy>().facingRight = facingRight;
    }
    
    private void HitWall()
    {
        Flip();
        animator.SetBool("IsRunning", false);
        state = State.Stunned;
        StartCoroutine(CooldownCoroutine(runningCooldown));
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

        fHorizontalVelocity *= Mathf.Pow(1f - fHorDmpBasic, Time.deltaTime * speedMulitiplier);

        enemy.velocity = new Vector2(fHorizontalVelocity, enemy.velocity.y);
    }

    public void ResetSFXCues()
    {
        if (state != State.Dead)
            state = State.Waiting;
        playerDied.Invoke();
        StartCoroutine(SetBack());
    }

    public void PlayHitGroundSFX()
    {
        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(stompShakeTime, stompShakeForce));
        AudioManager.instance.PlaySound(AudioManager.SoundList.EnemyHitGround);
    }

    private IEnumerator CooldownCoroutine(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);

        if(state != State.Dead)
            state = State.Normal;
    }

    private IEnumerator PauseBeforeAttack(float pauseTime)
    {
        state = State.Attack;
        ForceFlip();
        SetActionTrigger();
        animator.speed = 0;
        yield return new WaitForSeconds(pauseTime);
        animator.speed = 1;
        yield return new WaitForSeconds(0.3f);
    }

    public void StartThunderSkills()
    {
        if (isWave)
            ThunderSkill();
        else
            ThunderFallSkill();
    }

    private void SetActionTrigger()
    {
        int i = SetSkillInt(); // range is X to Y -1 for ints. [1-3]
        switch (i)
        {
            case 1:
                animator.SetBool("IsRunning", true);
                //animator.SetTrigger("Run");
                ForceFlip();
                state = State.Running;
                break;
            case 2:
                animator.SetTrigger("LightningSkill");
                isWave = true;
                break;
            case 3:
                animator.SetTrigger("LightningSkill");
                isWave = false;
                break;
        }
    }

    private int SetSkillInt()
    {
        RaycastHit2D raySkillWall = Physics2D.Raycast(transform.position, raycastDirection, 15, layerMaskGround);
        if (raySkillWall)
            return 1;
        else
            return Random.Range(1, 4);
    }

    public void ThunderSkill()
    {
        int sign = facingRight ? 1 : -1;
        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(stompShakeTime, stompShakeForce));
        thunderSpawnPos = new Vector2(enemy.transform.position.x, enemy.transform.position.y + stompSpawnHeight);
        RaycastHit2D rayToWall = Physics2D.Raycast(transform.position, raycastDirection, 100, layerMaskGround);
        StartCoroutine(ThunderWave(sign, rayToWall.point.x));
        StartCoroutine(CooldownCoroutine(waveCooldown));
    }

    
    private IEnumerator ThunderWave(int sign, float wallXPos)
    {
        thunderSpawnPos.x += sign * waveSpawnSpacing;
        if ((thunderSpawnPos.x < wallXPos && facingRight) || (thunderSpawnPos.x > wallXPos && !facingRight))
        {
            AudioManager.instance.PlaySound(AudioManager.SoundList.MoonBossZap);
            yield return new WaitForSeconds(waveSpeed);
            GameObject stompSkill = Instantiate(thunderObject, thunderSpawnPos, Quaternion.identity, transform.parent);
            StartCoroutine(ThunderWave(sign, wallXPos));
        }
    }

    private void ForceFlip()
    {
        if (player.transform.position.x > enemy.transform.position.x && !facingRight)
            Flip();
        else if (player.transform.position.x < enemy.transform.position.x && facingRight)
            Flip();
    }

    public void ThunderFallSkill()
    {
        SetPosition();
        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(stompShakeTime, stompShakeForce));
        foreach (float position in arrayOfPosition)
        {
            float y = player.transform.position.y;
            RaycastHit2D rayToFloor = Physics2D.Raycast(new Vector2(position, y), Vector2.down, 100, layerMaskGround);
            Vector2 spawnPos = new Vector2(rayToFloor.point.x, rayToFloor.point.y + 1);
            GameObject warning = Instantiate(warningThunder, spawnPos, Quaternion.identity, transform.parent);
            StartCoroutine(ThunderWarning(spawnPos));
        }
        StartCoroutine(CooldownCoroutine(scatterCooldown));
    }

    private IEnumerator ThunderWarning(Vector2 pos)
    {
        yield return new WaitForSeconds(warningThunderTime);
        //AudioManager.instance.PlaySound(AudioManager.SoundList.MoonBossZap);
        StartCoroutine(StartZapSounds());
        GameObject thunder = Instantiate(thunderObject, new Vector2(pos.x, pos.y + stompSpawnHeight*2), Quaternion.identity, transform.parent);
    }

    private IEnumerator StartZapSounds()
    {
        AudioManager manager = AudioManager.instance;
        manager.PlaySound(AudioManager.SoundList.MoonBossZap);
        yield return new WaitForSeconds(0.05f);
        manager.PlaySound(AudioManager.SoundList.MoonBossZap);
        yield return new WaitForSeconds(0.05f);
        manager.PlaySound(AudioManager.SoundList.MoonBossZap);
    }

    private void SetPosition()
    {
        arrayOfPosition[0] = transform.position.x;
        for (int i = 1; i < arrayOfPosition.Length; i++)
        {
            CheckPosition(i);
        }
    }

    private void CheckPosition(int currentI)
    {
        float rayXPos = facingRight ? Random.Range(transform.position.x + 3f, rightWallXPos) : Random.Range(transform.position.x - 3f, leftWallXPos);
        int check = 0;
        for (int i = 0; i < currentI; i++)
        {
            if (Mathf.Abs(rayXPos - arrayOfPosition[i]) > thunderFallSpawnWidth)
                check++;
        }
        if (check == currentI)
            arrayOfPosition[currentI] = rayXPos;
        else
            CheckPosition(currentI);
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
        if (state != State.Dead)
            state = State.Normal;
    }


    public void SetStateDead()
    {
        state = State.Dead;
        enemy.velocity = Vector2.zero;
        bossDied.Invoke();
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

    private IEnumerator StallAnimationCoroutine()
    {
        animator.speed = 0;

        yield return new WaitForSeconds(stallAnimation);

        animator.speed = 1;
    }

    public void HandlePhases(float hp)
    {
        if (hp < phaseTwoHp && currentPhase == 1)
        {
            currentPhase++;
            animator.SetFloat("IdleSpeed", 1.5f);
            animator.SetFloat("RunSpeed", 1.25f);
            speedMulitiplier = 1.25f;
        }
        if (hp < phaseThreeHp && currentPhase == 2)
        {
            currentPhase++;
            animator.SetFloat("IdleSpeed", 2f);
            animator.SetFloat("RunSpeed", 1.5f);
            speedMulitiplier = 1f;
        }
    }

    private void OnDisable()
    {
        if (state == State.Dead)
        {
            GetComponentInParent<RoomManagerOne>().enemiesList = new RoomManagerOne.EnemiesRespawner[0];
            Destroy(gameObject);
        }
    }

    public void PlayerHasRespawned()
    {
        if(state != State.Dead)
        {
            state = State.Waiting;
            if (currentPhase != 1)
            {
                currentPhase = 1;
                animator.SetFloat("IdleSpeed", 1f);
                animator.SetFloat("RunSpeed", 1f);
                speedMulitiplier = 1.5f;
            }
        }
        playerDied.Invoke();
    }

    public void SpawnSpellUnlock()
    {
        spellUnlockObject.transform.position = transform.position;
        spellUnlockObject.SetActive(true);
    }

    public void PlayBreathSFX()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.MoonBossBreathing);
    }

    public void PlayStompSFX()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.MoonBossAttack);
    }

    public void PlayRunSFX()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.MoonBossRun);
    }

    private IEnumerator SetBack()
    {
        playerRespawned = true;

        yield return new WaitForSeconds(.35f);

        playerRespawned = false;
    }

}
