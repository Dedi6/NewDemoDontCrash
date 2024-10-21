using System.Collections;
using UnityEngine;

public class DemoMan_Script : MonoBehaviour, ISFXResetable, IPhaseable<float>, IRespawnResetable
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
    private bool fightHasStarted, shouldMovePhase;

    [Header("Attacking")]
    [Space]
    [SerializeField] private float screenShake_Force;
    [SerializeField] private float screenShake_Time, pauseBeforeAttack, pauseBefore_Flying, runningSpeed, smokeBombOffset;
    [SerializeField] private float melee_Collider_Radius, flySpeed;
    [SerializeField] private float melee_CD, throw_CD, launch_CD, playerCloseDistance;
    private float skillCoolDownTimer;


    private int currentSkillsInt;
    private Vector2 flyDirection;
    [SerializeField] private Vector2 bomb_Speed_Vector;

    [Header("Cooldowns")]
    [Space]
    public float cannonSummonCD;


    [Header("Caching")]
    [Space]
    [SerializeField]
    private Transform getUp_Position;
    [SerializeField]
    private Transform melee_HitPos, _meleePos_2, bomb_Throw_Position, bombInHand_HitPos, respawnPosition, startFightTrigger, flyBombPos;
    [SerializeField]
    private GameObject flying_Collider, bomb_Prefab, smokeBomb_Prefab, bushObject;
    [SerializeField]
    private UnityEngine.Video.VideoPlayer videoPlayer;



    private AudioManager audio_M;
    private System.Action currentSkill;
    [SerializeField]
    private Animator animator;
    private BoxCollider2D boxCollider;
    public UnityEngine.Events.UnityEvent bossFightTriggered, bossDied;
    GameObject player;

    private State state;

    private enum State
    {
        Waiting,
        Normal,
        Attack,
        Stunned,
        Flying,
        Running,
        Dead,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        player = GameMaster.instance.playerInstance;
        GetComponent<Enemy>().usePhases = true;
        bool goRight = GetComponent<Enemy>().goRight;
        if ((goRight && !facingRight) || (!goRight && facingRight))
            Flip();
        originalPos = transform.position;

        audio_M = AudioManager.instance;
    }

    private void Awake()
    {
        state = State.Waiting;
    }

    void Update()
    {

        if (state == State.Normal)
        {
            if (Mathf.Abs(transform.position.x - player.transform.position.x) < 8 && turnAroundTimer <= 0 && (Mathf.Abs(transform.position.y - player.transform.position.y) < 2))
            {
                ForceFlip();
            }
            if (skillCoolDownTimer > 0)
                skillCoolDownTimer -= Time.deltaTime;
            else
                StartAttacking();
        }

        // inputs for testing
        /* if (Input.GetKeyDown(KeyCode.L))
             Start_Melee();
         if (Input.GetKeyDown(KeyCode.O))
             Start_Launch();
         if (Input.GetKeyDown(KeyCode.P))
             Start_Throw();
         if (Input.GetKeyDown(KeyCode.I))
             Start_SmokeBomb();
         if (Input.GetKeyDown(KeyCode.U))
         {
             state = State.Normal;
             enemy.velocity = Vector2.zero;
             transform.localPosition = Vector2.zero;
         }*/


        if (turnAroundTimer > 0)
            turnAroundTimer -= Time.deltaTime;

        HandleEnemyClassObjects();
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Waiting:
                break;
            case State.Normal:
                break;
            case State.Flying:
                Fly();
                break;
            case State.Running:
                Sprint();
                break;
        }

        HandleRaycasts();

        //    Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //     Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //    Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, boxCollider.bounds.extents.y + groundCheckDistance, 0), Vector2.right * (boxCollider.bounds.extents.x + groundCheckDistance));
    }

    private void HandleRaycasts()
    {
        Vector2 _wallcheck_Dir = facingRight ? Vector2.right : Vector2.left;
        wallCheckRaycast = Physics2D.Raycast(transform.position, _wallcheck_Dir, wallCheckDistance, layerMaskGround);
        Debug.DrawRay(transform.position, _wallcheck_Dir * wallCheckDistance, Color.red);
    }

    private void StartAttacking()
    {
        state = State.Attack;

        if(shouldMovePhase)
        {
            Start_SmokeBomb();
            return;
        }


        int max = IsPlayerClose() ? 4 : 3;
        int skillInt = Random.Range(1, max); // max is n-1

        if (skillInt == 1)
            Start_Launch();
        if (skillInt == 2)
            Start_Throw();
        if (skillInt == 3)
            Start_Melee();
    }

    private bool IsPlayerClose()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);
        return distance < playerCloseDistance;
    }

    private void HandleFirstTime()
    {

    }

    private bool IsFirstTime()
    {
        return true;
    }

    public void HitBush()
    {
        Animator bushAnimator = bushObject.GetComponent<Animator>();
        string isRightString = Bush_Is_PlayerToTheRight() ? "Right" : "Left";

        bushAnimator.GetComponent<Animator>().Play("Demoman_Bush_Attacked_" + isRightString);
        startFightTrigger.GetComponent<TriggerAction>().triggered.Invoke();
        AudioManager.instance.PlaySound(AudioManager.SoundList.EnemyHit);

        StartFight();

        PlayerPrefs.SetInt("Demoman_FirstTime", 2);
    }

    public void TriggeredCollider()
    {
        Animator bushAnimator = bushObject.GetComponent<Animator>();

        bushObject.layer = 13;

        if (!PlayerPrefs.HasKey("Demoman_FirstTime"))
        {
            PlayerPrefs.SetInt("Demoman_FirstTime", 1);
            bushAnimator.GetComponent<Animator>().Play("Demoman_Bush_Appear");

            StartFight();
        }
        else
        {
            Repeat_StartFight();
        }
    }

    private void StartFight()
    {
        if (!fightHasStarted)
        {
            player.GetComponent<MovementPlatformer>().StartIgnoreInput();
            StartCoroutine(PlayVideo_Delay());
            fightHasStarted = true;
            //   audio_M.StartTransitionCoroutine(AudioManager.SoundList.DrillBoss_bg_Start, AudioManager.SoundList.DrillBoss_bg_loop, 10f);
        }
    }

    private void Repeat_StartFight()
    {
        if (!fightHasStarted)
        {
            fightHasStarted = true;
            SetStateNormal();
        }
    }

    private IEnumerator PlayVideo_Delay()
    {
        yield return new WaitForSeconds(0.8f);

        videoPlayer.Play();

        float waitReduction = 1f;
        yield return new WaitForSeconds((float)videoPlayer.length - waitReduction);

        transform.position = bushObject.transform.position;
        bushObject.GetComponent<Animator>().Play("Demoman_Bush_Empty");
        ForceFlip();

        yield return new WaitForSeconds(waitReduction);

        player.GetComponent<MovementPlatformer>().EndIgnoreInput();
        state = State.Normal;
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
        // ForceFlip();
        animator.speed = 0;

        yield return new WaitForSeconds(pauseTime);

        animator.speed = 1;
    }


    private void SetActionTrigger()
    {
        int max = currentPhase == 1 ? 65 : 100;
        int i = Random.Range(1, max); // range is X to Y -1 for ints. [1-3]

        // bool isPlayerClose();


        if (1 <= i && i <= 40)
        {
            // Start Melee


            // AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossPrep);
        }
        else if (41 <= i && i <= 65)
        {
            // Start Launch Towards Player


            // animator.SetBool("FirstSlash", SetSlashBool());
        }
        else if (66 <= i && i <= 100)
        {
            // Start Throw Bomb


            // AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossPrepPunch);
        }
    }

    private void Start_Melee()
    {
        animator.Play("Demoman_Melee");
    }

    public void Melee_HitCheck()
    {
        GameMaster.instance.ShakeCamera(0.2f, 2.5f);
        Collider2D _player_Hit = Physics2D.OverlapCircle(melee_HitPos.position, melee_Collider_Radius, 1 << 11);
        if (_player_Hit != null)
            GetComponent<Enemy>().PlayerKnockBackAndDamage();

        GameObject anotherBomb = Instantiate(bomb_Prefab, _meleePos_2.position, Quaternion.identity);
        anotherBomb.GetComponent<Action_TriggerHitPlayer>().TriggerNow();

        skillCoolDownTimer = melee_CD;
        SetStateNormal();
    }

    private void Start_Launch()
    {
        animator.Play("Demoman_Launch");
    }

    public void HoldBeforeFlying()
    {
        StartCoroutine(PauseBeforeAttack(pauseBefore_Flying));
    }

    public void Start_Flying()
    {
        GameObject anotherBomb = Instantiate(bomb_Prefab, flyBombPos.position, Quaternion.identity);
        anotherBomb.GetComponent<Action_TriggerHitPlayer>().TriggerNow();

        GameMaster.instance.ShakeCamera(0.15f, 1.5f);
        state = State.Flying;
        flyDirection = Is_PlayerToTheRight() ? Vector2.right : Vector2.left;
        SwitchColliders_Flying(true);
        ForceFlip();
    }

    private void SwitchColliders_Flying(bool isFlyingRightNow)
    {
        flying_Collider.SetActive(isFlyingRightNow);
        boxCollider.enabled = !isFlyingRightNow;
    }

    private void Fly()
    {
        if (wallCheckRaycast)    // hit wall 
        {
            SetStateNormal();
            enemy.velocity = Vector2.zero;
            animator.Play("Demoman_GetUp");
            SwitchColliders_Flying(false);
            ForceFlip();

            float angle = facingRight ? 270f : 90f;
            PrefabManager.instance.Play_VFX_Complex(PrefabManager.ListOfVFX.SmokeBomb, transform.position, angle, "Tilemap", 0.5f);
            skillCoolDownTimer = launch_CD;
        }

        enemy.velocity = flyDirection * flySpeed;

    }

    private void Start_Throw()
    {
        state = State.Attack;
        animator.Play("Demoman_Throw");
        ForceFlip();
        Switch_LayerMask(true);
        //  Invoke("Set_GetUP_Position", animator.GetCurrentAnimatorStateInfo(0).length);
    }

    public void Spawn_Bomb()
    {
        GameObject bomb_Spawn = Instantiate(bomb_Prefab, bomb_Throw_Position.position, Quaternion.identity);
        int faceRight_Correction = facingRight ? 1 : -1;
        Vector2 bombVector = new Vector2(faceRight_Correction * Mathf.Abs(transform.position.x - player.transform.position.x) * bomb_Speed_Vector.x, bomb_Speed_Vector.y);
        bomb_Spawn.GetComponent<Rigidbody2D>().velocity = bombVector;
    }

    public void Explode_BombInHand()
    {
        GameMaster.instance.ShakeCamera(0.2f, 2.5f);
        Collider2D _player_Hit = Physics2D.OverlapCircle(bombInHand_HitPos.position, melee_Collider_Radius, 1 << 11);
        if (_player_Hit != null)
            GetComponent<Enemy>().PlayerKnockBackAndDamage();
    }

    private void Start_SmokeBomb()
    {
        animator.Play("Demoman_Smoke");
        bossDied.Invoke();
    }

    public void Play_smokeBomb_VFX()
    {
        Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + smokeBombOffset);

        GameObject smokeBombVFX = Instantiate(smokeBomb_Prefab, spawnPos, Quaternion.identity);

        if (!facingRight)
            Flip();

        StartCoroutine(DelaySprint());
    }

    private IEnumerator DelaySprint()
    {
        gameObject.layer = 13;

        yield return new WaitForSeconds(0.1f);

        state = State.Running;

        // delay X seconds and destroy
    }

    private void Sprint()
    {
        enemy.velocity = new Vector2(runningSpeed, enemy.velocity.y);
    }

    public void Set_GetUP_Position()
    {
        StartCoroutine(DelayChangeAnimation());
    }

    private IEnumerator DelayChangeAnimation()
    {
        animator.Play("Demoman_Rearming");

        yield return null;

        transform.position = getUp_Position.position;
        Switch_LayerMask(false);
        SetStateNormal();
        skillCoolDownTimer = throw_CD;
    }


    private void Switch_LayerMask(bool shouldIgnorePlayer)
    {
        string _layermask_String = shouldIgnorePlayer ? "DeadEnemy" : "enemy";
        gameObject.layer = LayerMask.NameToLayer(_layermask_String);
    }


    public IEnumerator StartSkill()
    {
        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(screenShake_Time, screenShake_Force));

        yield return new WaitForSeconds(0.5f);

        AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossEnrage);
        currentSkill();
    }



    private void DodgeStart()
    {
        if (Vector2.Distance(transform.position, player.transform.position) > 10)
            return;
        int i = Random.Range(1, 11); // 1 - 10.
        if (i < 5)
        {
            //Dodge()
            //  animator.SetTrigger("Dodge");

        }
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
        if (state != State.Dead || state != State.Waiting)
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
            shouldMovePhase = true;
            
        }
        if (hp < phaseThreeHp && currentPhase == 2)
        {
            currentPhase++;
        }
    }

    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("Demoman_FirstTime"))
            transform.position = respawnPosition.position;

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

       /* bossDied.Invoke();
        animator.Play("Demoman_Idle");*/

        fightHasStarted = false;
        startFightTrigger.position = new Vector2(bushObject.transform.position.x, startFightTrigger.transform.position.y);

        state = State.Waiting;
        currentSkillsInt = 60;

        currentPhase = 1;
    }

    private IEnumerator SetBack()
    {
        playerRespawned = true;
        // AudioManager.instance.PlayTheme(AudioManager.SoundList.Fear2, 10f); play the song for the level

        yield return new WaitForSeconds(.35f);

        shouldMovePhase = false;
        bushObject.layer = 13;
        bossDied.Invoke();
        animator.Play("Demoman_Idle");
        playerRespawned = false;
        transform.position = respawnPosition.position;
    }

    private bool Is_PlayerToTheRight()
    {
        return transform.position.x < player.transform.position.x;
    }

    private bool Bush_Is_PlayerToTheRight()
    {
        return bushObject.transform.position.x < player.transform.position.x;
    }
}
