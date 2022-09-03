using System.Collections;
using UnityEngine;

public class DrillBoss : MonoBehaviour, ISFXResetable, IKnockbackable, IPhaseable<float>, IRespawnResetable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPointGround;
    private bool facingRight = false, playerRespawned = false;
    private RaycastHit2D wallCheckRaycast;
    private RaycastHit2D groundCheckRaycast;
    private Vector2 originalPos;
    public float wallCheckDistance = 1, attackCheckDistance, knockBackTime, turnAroundTimer;
    public float groundCheckDistance = 1, stallAnimation, phaseTwoHp, phaseThreeHp;
    private int layerMaskGround = 1 << 8, currentPhase = 1;
    private Collider2D waitingCast;

    [Header("Attacking")]
    [Space]
    public float pauseBeforeAttack;
    public float stompShakeTime, stompShakeForce, projectileSpeed;
    private int currentSkillsInt;
    public Transform leftPosition, rightPosition;
    private Vector2 currentSpawnPoint;
    [SerializeField]
    private Transform topMovePos, shootPosLeft, shootPosRight;
    private IEnumerator stopDrillCoroutine;
    [SerializeField]
    private GameObject projectilePrefab;


    [Header("Cooldowns")]
    [Space]
    public float cannonSummonCD;


    private System.Action currentSkill;
    public Animator animator;
    private BoxCollider2D boxCollider;
    public UnityEngine.Events.UnityEvent bossFightTriggered, bossDied;
    public Transform enemiesParent;
    GameObject player;

    private State state;

    private enum State
    {
        Waiting,
        Normal,
        Attack,
        Stunned,
        Moving,
        Dead,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        GetComponent<Enemy>().usePhases = true;
        currentSkillsInt = 45;
        bool goRight = GetComponent<Enemy>().goRight;
        if ((goRight && !facingRight) || (!goRight && facingRight))
            Flip();
        originalPos = transform.position;
        stopDrillCoroutine = StopDrillDown();
    }

    private void Awake()
    {
        state = State.Waiting;
    }

    void Update()
    {

        if (state == State.Normal)
        {
            if (Mathf.Abs(transform.position.x - player.transform.position.x) < 8 && turnAroundTimer <= 0 )
            {
                ForceFlip();
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
            DrillDown();
        if (Input.GetKeyDown(KeyCode.J))
            MoveSetUp();
        if (Input.GetKeyDown(KeyCode.M))
            StartShooting();

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
               //StartCoroutine(PauseBeforeAttack(pauseBeforeAttack));
                break;
            case State.Attack:
                if (groundCheckRaycast)
                    StartCoroutine(stopDrillCoroutine);
                break;
            case State.Moving:
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
        groundCheckRaycast = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, 1 << 8);
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
        GetComponent<Enemy>().facingRight = facingRight;
    }

    private void Flip()
    {
        turnAroundTimer = 0.3f;
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    public void ResetSFXCues()
    {
        state = State.Waiting;
        currentSkillsInt = 60;
        if (currentPhase == 3)
            GetComponentInChildren<LightZone>().Shrink();
        currentPhase = 1;
    }

    private IEnumerator CooldownCoroutine(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);

        if (state != State.Dead && state != State.Waiting)
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


    private void SetActionTrigger()
    {
        int max = currentPhase == 1 ? 65 : 100;
        int i = Random.Range(1, max); // range is X to Y -1 for ints. [1-3]

        if (1 <= i && i <= 40)
        {
            animator.SetTrigger("Enrage");
            // AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossPrep);
            SetEnrageSkill();
        }
        else if (41 <= i && i <= 65)
        {
            animator.SetTrigger("Slash");
            // animator.SetBool("FirstSlash", SetSlashBool());
        }
        else if (66 <= i && i <= 100)
        {
            animator.SetTrigger("PunchStart");
            // AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossPrepPunch);
        }
    }



    private void SetEnrageSkill()
    {

        if (currentPhase != 3)             // not final phase
        {
            int i = Random.Range(1, currentSkillsInt + 1); // range is X to Y -1 for ints. 
            if (1 <= i && i <= 15)
            {
                //   currentSkill = SummonRedFrogs;
            }
            else if (16 <= i && i <= 30)
            {
                // currentSkill = ShootInACircle;
                currentSpawnPoint = new Vector2(player.transform.position.x, player.transform.position.y + 5f);
            }
            else if (31 <= i && i <= 45)
            {
                // currentSkill = ShootMeteor;
            }
        }
        else                               // in final phase
        {
            int i = Random.Range(1, 3); // range is X to Y -1 for ints.  so 1-2 here.
            if (i == 1)
            {
                //Meteor side to side
                // currentSkill = StartMeteorFall;
            }
            else
            {
                //Meteor interlace
                // currentSkill = StartMeteorInterlace;
            }
        }
    }

    void StartShooting()
    {
        ForceFlip();
        animator.SetTrigger("Shoot");
    }

    private void Move()
    {
        if (Vector2.Distance(transform.position, topMovePos.transform.position) > 1f)
            transform.position = Vector2.MoveTowards(transform.position, topMovePos.transform.position, 30f * Time.deltaTime);
        else
        {
            state = State.Attack;
            animator.SetTrigger("StartDrill");
        }
    }

    private void MoveSetUp()
    {
        ForceFlip();
        StopCoroutine(stopDrillCoroutine);
        stopDrillCoroutine = StopDrillDown();
        animator.SetBool("DrillDown", false);
        topMovePos.transform.position = new Vector2(player.transform.position.x, topMovePos.transform.position.y);
        state = State.Moving;
    }

    public IEnumerator StartSkill()
    {
        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(stompShakeTime, stompShakeForce));
        GameObject pref = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.RageBossSummon);
        GameObject summonEffect = Instantiate(pref, currentSpawnPoint, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossEnrage);
        currentSkill();
    }

    private void DrillDown()
    {
        state = State.Attack;
        animator.SetBool("DrillDown", true);
        enemy.velocity = Vector2.down * 50f;
    }

    private IEnumerator StopDrillDown()
    {
        enemy.velocity = Vector2.zero;
        state = State.Stunned;
        GameMaster.instance.ShakeCamera(1.5f, 1.5f);

        yield return new WaitForSeconds(1.5f);

        animator.SetBool("DrillDown", false);
        state = State.Normal;
    }


    void Shoot()
    {
        CreateProjectile(shootPosLeft.position);
        CreateProjectile(shootPosRight.position);
    }

    void CreateProjectile(Vector3 shootPos)
    {
        var pos = shootPos;
        var dir = player.transform.position - pos;
        GameObject projectile = Instantiate(projectilePrefab, pos, Quaternion.identity);
        projectile.GetComponent<Action_TriggerHitPlayer>().SetMovement(dir.normalized, projectileSpeed);
    }


    private bool IsRightSide()
    {
        float aDistance = Vector2.Distance(player.transform.position, leftPosition.transform.position);
        float bDistance = Vector2.Distance(player.transform.position, rightPosition.transform.position);
        if (aDistance > bDistance)
            return true;
        else
            return false;
    }



    public void Dissapear()
    {
        ToggleColliders();
        //AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossTeleport);
        StartCoroutine(StallAnimationForTime(1f));
    }

    public void ToggleColliders()
    {
        GetComponent<Enemy>().ToggleTriggerCollider();
        gameObject.layer = gameObject.layer == 12 ? 13 : 12;
    }


    private void DodgeStart()
    {
        if (Vector2.Distance(transform.position, player.transform.position) > 10)
            return;
        int i = Random.Range(1, 11); // 1 - 10.
        if (i < 5)
            animator.SetTrigger("Dodge");
    }

    public void Dodge()
    {
        //AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossTeleport);
        //transform.position = new Vector2(GetDodgePointX(), transform.position.y);
        ForceFlip();
    }


    private void ForceFlip()
    {
        if (player.transform.position.x > enemy.transform.position.x && !facingRight)
            Flip();
        else if (player.transform.position.x < enemy.transform.position.x && facingRight)
            Flip();
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


    public IEnumerator StallAnimationForTime(float time)
    {
        animator.speed = 0;

        yield return new WaitForSeconds(time);

        animator.speed = 1;
    }

    public void HandlePhases(float hp)
    {
        DodgeStart();
        if (hp < phaseTwoHp && currentPhase == 1)
        {
            currentPhase++;
        }
        if (hp < phaseThreeHp && currentPhase == 2)
        {
            currentPhase++;
        }
    }

    private void OnDisable()
    {
        if (state == State.Dead)
            Destroy(gameObject);
    }


    public void PlayerHasRespawned()
    {
        if (!playerRespawned)
        {
            playerRespawned = true;
            StartCoroutine(SetBack());
        }

        transform.position = originalPos;
        state = State.Waiting;
        currentSkillsInt = 60;

        currentPhase = 1;
    }

    private IEnumerator SetBack()
    {
        playerRespawned = true;
        // AudioManager.instance.PlayTheme(AudioManager.SoundList.Fear2, 10f); play the song for the level

        yield return new WaitForSeconds(.35f);

        playerRespawned = false;
    }
}
