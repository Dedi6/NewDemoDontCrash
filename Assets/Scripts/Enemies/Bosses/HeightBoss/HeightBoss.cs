using System.Collections;
using UnityEngine;

public class HeightBoss : MonoBehaviour, ISFXResetable, IKnockbackable, IPhaseable<float>, IRespawnResetable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPointGround;
    private bool facingRight = true, playerRespawned = false, isOnFarLayer;
    private Vector2 originalPos;
    public float wallCheckDistance = 1, attackCheckDistance, knockBackTime, turnAroundTimer;
    public float groundCheckDistance = 1, stallAnimation, phaseTwoHp, phaseThreeHp, specialMoveHp;
    private int layerMaskGround = 1 << 8, currentPhase = 1;
    private Collider2D waitingCast;
    private Coroutine cooldownCoroutine;
    [SerializeField]
    private int deathBlinkNumber;

    [Header("Attacking")]
    [Space]
    public float pauseBeforeAttack;
    public float stompShakeTime, stompShakeForce, dashSpeed, swordDelay, swordHeight, cannonDelay, cannonHeight, cannonBallSpeed, fanDuration, fanParticleSpeed, specialSwordDelay;
    private float dashSpeedMultiplier, deathTpWait = 0.5f;
    public int swordAmountMax, specialSwordAmount, cannonAmountMax, playerMassRunToFan;
    private int swordAmount, cannonAmount, cannonDivide, currentFanAngle, currentQuake;
    private Vector2 currentSpawnPoint, dashDir, dashEndPos;
    private bool isDashing, isFanActive, didQuakeOnFar, didQuakeOnRight, isSeondQuake, isFirstQuakeRight, isQuaking, usedSpecialMove, canDodge, shouldDisableSecondCol, shouldDisableCannons;
    private string coroutineName;

    [Header("Positions")]
    [Space]
    public Transform leftPosition;
    public Transform rightPosition, quakePosLeft, quakePosRight, leftPosClose, rightPosClose, quakePosLeftClose, quakePosRightClose, startingQuake;
    [SerializeField]
    private GameObject quakeObjectRight, quakeObjectLeft, quakeObjectRightClose, quakeObjectLeftClose, startQuakeObject, triggerFightObject, firstQuakeCollider, secondQuakeCollider, seperateQuakeCol;

    [Header("Cooldowns")]
    [Space]
    [SerializeField]
    private float cannonSummonCD;
    [SerializeField]
    private float swordSummonCD, fanSummonCD, dashCD;


    private System.Action currentSkill;
    public Animator animator;
    public LayerSwitcher layerSwitcher;
    private BoxCollider2D boxCollider;
    public UnityEngine.Events.UnityEvent bossFightTriggered, bossDied;
    public Transform enemiesParent;
    private AudioManager audioManager;
    GameObject player;
    [Header("Summon Prefabs")]
    [Space]
    [SerializeField]
    private GameObject swordPrefab;
    [SerializeField]
    private GameObject cannonPrefab, fanPrefab, cannonBallPrefab, quakePrefab, blackOverlay;
    [SerializeField]
    private GameObject[] tpPortals;
    [SerializeField]
    private AudioSource bg_Gameobject;

    private State state;

    private enum State
    {
        Waiting,
        Normal,
        Attack,
        Stunned,
        Dead,
        SpecialAttack,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        player = GameMaster.instance.playerInstance;
        GetComponent<Enemy>().usePhases = true;
        bool goRight = GetComponent<Enemy>().goRight;
        if ((goRight && !facingRight) || (!goRight && facingRight))
            ForceFlip();
        originalPos = transform.localPosition;
        isOnFarLayer = true;
        SwitchLayers();
        SwitchLayers();
        SwitchLayers();
        HandleSkillPrefabs();
    }

    void HandleSkillPrefabs()
    {
        swordAmount = swordAmountMax;
        cannonAmount = cannonAmountMax;
        audioManager = AudioManager.instance;
    }

    private void Awake()
    {
        state = State.Waiting;
    }

    void Update()
    {

        if (state == State.Normal)
        {
            if (turnAroundTimer <= 0)
            {
                ForceFlip();
            }
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
                //  StartCoroutine(PauseBeforeAttack(pauseBeforeAttack));
                break;
            case State.Attack:
                if (isDashing)
                    Dash();
                break;
        }

        HandleRaycasts();

        //    Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //     Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //    Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, boxCollider.bounds.extents.y + groundCheckDistance, 0), Vector2.right * (boxCollider.bounds.extents.x + groundCheckDistance));
    }

    private void HandleRaycasts()
    {
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
        if (state == State.Dead) return;

        GetComponent<Enemy>().facingRight = facingRight;

        // handle collider for layer switcher
        if (layerSwitcher.ShouldDisableCollider(isOnFarLayer) && boxCollider.enabled)
            boxCollider.enabled = false;
        else if (!layerSwitcher.ShouldDisableCollider(isOnFarLayer) && boxCollider.enabled == false)
            boxCollider.enabled = true;

        if (isFanActive)
        {
            float playerXInput = player.GetComponent<MovementPlatformer>().moveInput;
            Rigidbody2D playerRB = player.GetComponent<Rigidbody2D>();
            if (playerXInput != 0)
                playerRB.mass = playerMassRunToFan;
            else
                playerRB.mass = 3;
        }
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

    private void SwitchLayers()
    {
        layerSwitcher.HandleBoss(1f, 1.95f, isOnFarLayer, transform);
        isOnFarLayer = !isOnFarLayer;
    }

    private IEnumerator CooldownCoroutine(float cooldown)
    {
        canDodge = true;
        yield return new WaitForSeconds(cooldown - 0.15f); // to prevent dodging

        canDodge = false;

        yield return new WaitForSeconds(0.15f);

        if (state != State.Dead && state != State.SpecialAttack)
        {
            if (!isQuaking)
                SetActionTrigger();
            if (isQuaking)
                StartCoroutine(StartQuake());
        }
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

    private void StartSummon()
    {
        StartCoroutine(DodgeCoroutine());
        StartCoroutine(DissableColliderForTime(0.25f));

        animator.SetTrigger("Summon");
    }

    public void PlaySummonSFX()
    {
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossSummon, transform.position);
    }

    private void SetActionTrigger()
    {
        state = State.Normal;
        if (isDashing) isDashing = false;

        int max = currentPhase == 1 ? 100 : 100;
        int i = Random.Range(1, max); // range is X to Y -1 for ints. [1-3]

        // POSssibiliteS Dash Sword Cannon Fan 
        if (1 <= i && i <= 40)
        {
            StartSummon();
            coroutineName = "SummonSword";
            // AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossPrep);
        }
        else if (41 <= i && i <= 75)
        {
            StartSummon();
            coroutineName = "SummonCannon";
            // animator.SetBool("FirstSlash", SetSlashBool());
        }
        else if (76 <= i && i <= 90)
        {
            StartCoroutine(DashCoroutine());
            // AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossPrepPunch);
        }
        else if (91 <= i && i <= 100)
        {
            StartSummon();
            coroutineName = "SummonFan";
            // AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossPrepPunch);
        }
    }

    public void StartSkill()
    {
        StartCoroutine(coroutineName);
    }


    private bool ShouldTeleportFar()
    {
        return layerSwitcher.IsOnFarSide();
    }

    private bool ShouldSwitchLanes()
    {
        return layerSwitcher.IsOnFarSide() == isOnFarLayer ? true : false;
    }

    private void StartDash()
    {
        animator.SetTrigger("Dash");
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossDodge, transform.position);
        canDodge = false;
        state = State.Attack;
        FreezeAnything();
    }

    private IEnumerator DashCoroutine()
    {
        Vector2 tpPosition;
        animator.SetTrigger("Blink");
        StartCoroutine(DissableColliderForTime(0.25f));
        canDodge = false;

        yield return new WaitForSeconds(0.25f);
        PlayDashStartSFX();
        yield return new WaitForSeconds(0.25f);

        if (ShouldSwitchLanes()) SwitchLayers();

        if (IsRightSide())
        {
            dashDir = Vector2.left;
            if (!isOnFarLayer)
            {
                tpPosition = rightPosition.localPosition;
                dashSpeedMultiplier = 1f;
                dashEndPos = leftPosition.localPosition;
            }
            else
            {
                tpPosition = rightPosClose.localPosition;
                dashSpeedMultiplier = 1.5f;
                dashEndPos = leftPosClose.localPosition;
            }
            // tp to right
            // sfx dash
            // 
            if (facingRight)
                Flip();
        }
        else
        {
            dashDir = Vector2.right;
            if (!isOnFarLayer)
            {
                tpPosition = leftPosition.localPosition;
                dashSpeedMultiplier = 1f;
                dashEndPos = rightPosition.localPosition;
            }
            else
            {
                tpPosition = leftPosClose.localPosition;
                dashSpeedMultiplier = 1.5f;
                dashEndPos = rightPosClose.localPosition;
            }
            if (!facingRight)
                Flip();
        }

        transform.localPosition = tpPosition;

        StartDash();

    }


    private void Dash()
    {
        enemy.velocity = dashDir * dashSpeed * dashSpeedMultiplier;

        if (isDashing && Vector2.Distance(enemy.transform.localPosition, dashEndPos) < 2f)
            StartCoroutine(StopDash());
    }

    public void ActivateDashBool()
    {
        StartCoroutine(DashAnimationDone());
    }

    private IEnumerator DashAnimationDone()
    {
        yield return new WaitForSeconds(0.1f);

        FreezeAnything();
        enemy.constraints = ~RigidbodyConstraints2D.FreezePositionX;
        isDashing = true;
        GetComponent<AudioClipsGameObject>().PlayAudioSource(1);
    }

    private IEnumerator StopDash()
    {
        animator.SetBool("DashStop", true);
        enemy.velocity = Vector2.zero;
        isDashing = false;
        state = State.Normal;
        GetComponent<AudioClipsGameObject>().StopAudioSource(1);

        yield return new WaitForSeconds(0.1f);

        animator.SetBool("DashStop", false);
        FreezeAnything();
        StartCoroutine(DodgeCoroutine());
        if (state != State.Waiting)
            cooldownCoroutine = StartCoroutine(CooldownCoroutine(dashCD));
        //  animator.SetBool("DashStop", false);
    }

    public void PlayDashStartSFX()
    {
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossDashStart, transform.position);
    }

    void FreezeAnything()
    {
        enemy.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public IEnumerator StartSkill2()
    {
        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(stompShakeTime, stompShakeForce));
        GameObject pref = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.RageBossSummon);
        GameObject summonEffect = Instantiate(pref, currentSpawnPoint, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        audioManager.PlaySound(AudioManager.SoundList.RageBossEnrage);
        currentSkill();
    }


    private IEnumerator SummonSword()
    {
        swordAmount--;
        bool isPlayerOnFar = layerSwitcher.IsOnFarSide();

        if (swordAmount % 4 == 0 && swordAmount != swordAmountMax)
        {
            float predictX = player.GetComponent<MovementPlatformer>().moveInput;
            if (predictX != 0)
                predictX = isPlayerOnFar ? predictX * 2f : predictX * 6f;
            Vector2 summonLast = new Vector2(player.transform.position.x + 1.7f + predictX, player.transform.position.y);
            GameObject sword = Instantiate(swordPrefab, summonLast, Quaternion.Euler(0, 0, -90));
            summonLast = new Vector2(player.transform.position.x - 1.7f + predictX, player.transform.position.y);
            GameObject sword2 = Instantiate(swordPrefab, summonLast, Quaternion.Euler(0, 0, 90));
            if (!isPlayerOnFar)
            {
                layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, sword.transform);
                layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, sword2.transform);
            }
            if (currentPhase >= 3)
            {
                summonLast = new Vector2(player.transform.position.x + predictX, player.transform.position.y + swordHeight);
                GameObject sword3 = Instantiate(swordPrefab, summonLast, Quaternion.identity);
                if (!isPlayerOnFar)
                    layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, sword3.transform);
            }
            audioManager.ThreeDSound(AudioManager.SoundList.HeightBossPortal, sword.transform.position);
            //audioManager.ThreeDSound(AudioManager.SoundList.HeightBossSword, sword.transform.position);
            //audioManager.ThreeDSound(AudioManager.SoundList.HeightBossSword, sword2.transform.position);
        }
        else
        {
            Vector2 summonPoint = new Vector2(player.transform.position.x, player.transform.position.y + swordHeight);
            GameObject sword = Instantiate(swordPrefab, summonPoint, Quaternion.identity);
            if (!isPlayerOnFar)
                layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, sword.transform);

            audioManager.ThreeDSound(AudioManager.SoundList.HeightBossPortal, sword.transform.position);
            //audioManager.ThreeDSound(AudioManager.SoundList.HeightBossSword, sword.transform.position);
        }

        yield return new WaitForSeconds(swordDelay);

        if (swordAmount > 0 && CanSummonAgain())
            StartCoroutine(SummonSword());
        else
        {
            swordAmount = swordAmountMax;
            if (state != State.Waiting)
                cooldownCoroutine = StartCoroutine(CooldownCoroutine(swordSummonCD));
        }
    }

    private bool CanSummonAgain()
    {
        return state != State.Waiting && state != State.SpecialAttack;
    }


    private int rndDiv;
    private IEnumerator SummonCannon()
    {
        if (cannonAmount == cannonAmountMax || cannonAmount == cannonAmountMax +1)
        {
            rndDiv = cannonDivide = GetRandomInt(2);
            cannonDivide = GetRandomInt(5);
        }
        // cannonDivide = GetRandomInt(2) == 1 ? 2 : 3;
        Vector2 summonPoint;
        Quaternion yRotation;
        bool isPlayerOnFar = layerSwitcher.IsOnFarSide();
        Vector2 summonHeightDiff = cannonAmount % cannonDivide == rndDiv ? new Vector2(0f, cannonHeight) : new Vector2(0f, -cannonHeight);
        cannonAmount--;

        if (IsRightSide())
        {
            if (isPlayerOnFar) summonPoint = rightPosition.transform.position;
            else summonPoint = rightPosClose.transform.position;
            yRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            if (isPlayerOnFar) summonPoint = leftPosition.transform.position;
            else summonPoint = leftPosClose.transform.position;
            yRotation = Quaternion.Euler(0, 0, 0);
        }

        GameObject cannonP = Instantiate(cannonPrefab, summonPoint + summonHeightDiff, yRotation);
        if(!isPlayerOnFar) layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, cannonP.transform);
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossPortal, cannonP.transform.position);

        yield return new WaitForSeconds(cannonDelay);

        if (!shouldDisableCannons && cannonAmount > 0)
            StartCoroutine(SummonCannon());
        else
        {
            cannonAmount = currentPhase > 1 ? cannonAmountMax + 1 : cannonAmountMax;
            if (state != State.Waiting)
                cooldownCoroutine = StartCoroutine(CooldownCoroutine(cannonSummonCD));
        }
    }

    public void ShootCannon(Transform cannon)
    {
        Vector2 posToSpawn = cannon.GetComponent<TriggerAction>().spawnPos.position;
        Vector2 dir = cannon.rotation.y == 0 ? Vector2.right : Vector2.left;
        GameObject cannonB = Instantiate(cannonBallPrefab, cannon.position, Quaternion.identity);
        layerSwitcher = GameObject.Find("Grid").GetComponent<LayerSwitcher>();
        bool isPlayerOnFar = cannon.localScale.x > 1 ? false : true;
        layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, cannonB.transform);
        cannonB.GetComponent<CircleCollider2D>().enabled = layerSwitcher.ShouldDisableCollider(isPlayerOnFar);
        cannonB.GetComponent<Action_TriggerHitPlayer>().SetPositionAndMovement(dir, cannonBallSpeed, posToSpawn);
        AudioManager.instance.ThreeDSound(AudioManager.SoundList.HeightBossCannon, posToSpawn);
    }

    private IEnumerator SummonFan()
    {
        bool isPlayerOnFar = layerSwitcher.IsOnFarSide();
        Vector2 summonPoint;
        Quaternion yRotation;

        if (IsRightSide())
        {
            if (isPlayerOnFar) summonPoint = rightPosition.transform.position;
            else summonPoint = rightPosClose.transform.position;
            yRotation = Quaternion.Euler(0, 180, 0);
            currentFanAngle = 180;
        }
        else
        {
            if (isPlayerOnFar) summonPoint = leftPosition.transform.position;
            else summonPoint = leftPosClose.transform.position;
            yRotation = Quaternion.Euler(0, 0, 0);
            currentFanAngle = 0;
        }

        GameObject fan = Instantiate(fanPrefab, summonPoint, yRotation);
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossPortal, transform.position);
        layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, fan.transform);
        fan.GetComponent<BoxCollider2D>().enabled = layerSwitcher.ShouldDisableCollider(isPlayerOnFar);
        fan.GetComponent<Animator>().SetBool("IsActive", true);
        fan.GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(1f);

        AreaEffector2D effector = fan.GetComponent<AreaEffector2D>();
        ParticleSystem particles = fan.GetComponentInChildren<ParticleSystem>();
        effector.enabled = true;
        isFanActive = true;
        effector.forceAngle = currentFanAngle;
        particles.Play();
        ParticleSystem.VelocityOverLifetimeModule particlesV = particles.velocityOverLifetime;
        if (currentFanAngle == 180)
        {
            particlesV.x = -fanParticleSpeed;
            particlesV.speedModifier = -1;
        }

        if (state != State.Waiting)
            cooldownCoroutine = StartCoroutine(CooldownCoroutine(fanSummonCD));

         yield return new WaitForSeconds(fanDuration);
                 
        fan.GetComponent<AudioSource>().Stop();
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossPortal, fan.transform.position);
        fan.GetComponent<Animator>().SetBool("IsActive", false);
        effector.enabled = false;
        isFanActive = false;
        particles.Stop();
    }

    
    private IEnumerator StartQuake()
    {
        animator.SetTrigger("Blink");
        StartCoroutine(DissableColliderForTime(0.25f));

        yield return new WaitForSeconds(0.5f);

        transform.localPosition = GetQuakePoint().localPosition;
        if (currentQuake == 0)
            didQuakeOnRight = isBossRightSide();
        if(isSeondQuake) isFirstQuakeRight = didQuakeOnRight;
        bool isPlayerOnFar = !isOnFarLayer;

        yield return new WaitForSeconds(0.5f);

        enemy.constraints = ~RigidbodyConstraints2D.FreezePositionY;
        Vector2 quakePos = QuakePoint();
        animator.SetTrigger("Quake");
        animator.SetBool("QuakeStop", false);
        enemy.velocity = Vector2.up * 3;
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossQuakeStart, transform.position);

        yield return new WaitForSeconds(0.4f);

        enemy.velocity = Vector2.down * 80f;

        yield return new WaitForSeconds(0.2f);

        FreezeAnything();
        GameObject quakeVFX = Instantiate(quakePrefab, quakePos, Quaternion.identity);
        GameMaster.instance.ShakeCamera(0.1f, 5f);
        layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, quakeVFX.transform);
        if (!isPlayerOnFar) 
        {
             SpriteRenderer sprite = quakeVFX.GetComponent<SpriteRenderer>();
             sprite.sortingLayerName = "Tilemap";
             sprite.sortingOrder = 4;
        }
        quakeVFX.transform.localPosition = QuakePoint();
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossQuake, quakeVFX.transform.position);
        if (!layerSwitcher.ShouldDisableCollider(isPlayerOnFar))
            quakeVFX.GetComponent<CheckForHit>().CallThisToIgnoreCheck();
        enemy.velocity = Vector2.zero;
        animator.SetBool("QuakeStop", true);
        DestroyGround();

        if (currentQuake == 1)
        {
            currentQuake = 0;
            isQuaking = false;
            StartCoroutine(DodgeCoroutine());
            if (state != State.Waiting)
                cooldownCoroutine = StartCoroutine(CooldownCoroutine(2f));
        }
        else
        {
            currentQuake++;
            isSeondQuake = true;
            StartCoroutine(StartQuake());
        }
    }

    private void DestroyGround()
    {
        GameObject ground;
        if(!isOnFarLayer)
            ground = didQuakeOnRight ?  quakeObjectRight: quakeObjectLeft;
        else
            ground = didQuakeOnRight ? quakeObjectRightClose : quakeObjectLeftClose;

        ground.GetComponent<Animator>().SetTrigger("Start");
        foreach (Transform child in ground.transform)
        {
            child.gameObject.SetActive(false);
        }

        if(!isOnFarLayer)
        {
            if(!shouldDisableSecondCol)
            {
                shouldDisableSecondCol = true;
                firstQuakeCollider.SetActive(false);
                secondQuakeCollider.SetActive(true);
                float x = didQuakeOnRight ? 812f : 825f;
                float xChild = didQuakeOnRight ? 43.5f : -19.5f;
                secondQuakeCollider.transform.localPosition = new Vector2(x, secondQuakeCollider.transform.localPosition.y);
                secondQuakeCollider.transform.GetChild(0).transform.localPosition = new Vector2(xChild, 0f);
            }
            else
            {
                secondQuakeCollider.SetActive(false);
                seperateQuakeCol.SetActive(true);
            }
        }
    }

    private void RevertEdgeColliders()
    {
        shouldDisableSecondCol = false;
        firstQuakeCollider.SetActive(true);
        firstQuakeCollider.GetComponent<BoxCollider2D>().enabled = true;
        secondQuakeCollider.SetActive(false);
        seperateQuakeCol.SetActive(false);
    }

    private Vector2 QuakePoint()
    {
        if(currentQuake == 0)
        {
            float y = !isOnFarLayer ? 79.6f : 75.17f;
            return new Vector2(transform.localPosition.x, y);
        }
        else
        {
            float y = didQuakeOnFar ? 75.17f : 79.6f;
            return new Vector2(transform.localPosition.x, y);
        }
    }

    public void StartFight()
    {
        StartCoroutine(StartingQuake());
        state = State.Normal;
    }

    private void HandlePortals(bool setActive)
    {
        foreach (GameObject portal in tpPortals)
        {
            portal.SetActive(setActive);
        }
    }

    private IEnumerator StartingQuake()
    {
        animator.SetTrigger("Blink");

        yield return new WaitForSeconds(0.5f);

        transform.localPosition = startingQuake.localPosition;
        if (isOnFarLayer) SwitchLayers();

        yield return new WaitForSeconds(0.5f);

        enemy.constraints = ~RigidbodyConstraints2D.FreezePositionY;
        Vector2 quakePos = QuakePoint();
        animator.SetTrigger("Quake");
        animator.SetBool("QuakeStop", false);
        enemy.velocity = Vector2.up * 3;
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossQuakeStart, transform.position);

        yield return new WaitForSeconds(0.4f);

        enemy.velocity = Vector2.down * 80f;

        yield return new WaitForSeconds(0.2f);

        FreezeAnything();
        GameObject quakeVFX = Instantiate(quakePrefab, quakePos, Quaternion.identity);
        GameMaster.instance.ShakeCamera(0.1f, 5f);
        layerSwitcher.HandleBoss(1f, 1.4f, true, quakeVFX.transform);
       
        quakeVFX.transform.localPosition = QuakePoint();
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossQuake, quakeVFX.transform.position);

        enemy.velocity = Vector2.zero;
        animator.SetBool("QuakeStop", true);

        startQuakeObject.GetComponent<Animator>().SetTrigger("Start");
        foreach (Transform child in startQuakeObject.transform)
        {
            child.gameObject.SetActive(false);
        }

        HandlePortals(true);
        SetStateNormal();
        audioManager.PlayTheme(AudioManager.SoundList.HeightFear_BossBG, 2f);
        audioManager.StartFadeOutSource(bg_Gameobject, 2f);

        animator.SetTrigger("Blink");
        cooldownCoroutine = StartCoroutine(CooldownCoroutine(2f));

        yield return new WaitForSeconds(2f);

        bg_Gameobject.enabled = false;
    }

    private IEnumerator StartSpecialMove()
    {
        StopCoroutine(cooldownCoroutine);
        state = State.SpecialAttack;
        shouldDisableCannons = true;

        StartCoroutine(DodgeCoroutine());

        yield return new WaitForSeconds(0.5f);

        float delayTime = 0.6f;
        StartCoroutine(blackOverlay.GetComponent<FadeIn>().FadeTo(0.6f, delayTime));

        yield return new WaitForSeconds(delayTime);

        swordAmount = specialSwordAmount;
        StartCoroutine(SpecialSword());

        yield return new WaitForSeconds(specialSwordAmount * specialSwordDelay + 1f);

        swordAmount = 4;
        StartCoroutine(SpecialSwordHorizontal());

        yield return new WaitForSeconds(2f);

        shouldDisableCannons = false;
        cannonAmount = 3;
        StartCoroutine(SummonCannon());

        yield return new WaitForSeconds(3f);

        StartCoroutine(blackOverlay.GetComponent<FadeIn>().FadeTo(0f, delayTime));
        state = State.Normal;

        yield return new WaitForSeconds(delayTime);

        if (state != State.Waiting)
            cooldownCoroutine = StartCoroutine(CooldownCoroutine(cannonSummonCD));
    }

    private IEnumerator SpecialSword()
    {
        swordAmount--;
        bool isPlayerOnFar = layerSwitcher.IsOnFarSide();

        Vector2 summonPoint = new Vector2(player.transform.position.x, player.transform.position.y + swordHeight);
        GameObject sword = Instantiate(swordPrefab, summonPoint, Quaternion.identity);
        if (!isPlayerOnFar)
            layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, sword.transform);
        // audioManager.ThreeDSound(AudioManager.SoundList.HeightBossSword, sword.transform.position);
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossPortal, sword.transform.position);

        yield return new WaitForSeconds(specialSwordDelay);

        if (swordAmount > 0)
            StartCoroutine(SpecialSword());
        else
            swordAmount = swordAmountMax;
    }

    private IEnumerator SpecialSwordHorizontal()
    {
        swordAmount--;
        bool isPlayerOnFar = layerSwitcher.IsOnFarSide();

        float predictX = player.GetComponent<MovementPlatformer>().moveInput;
        if (predictX != 0)
            predictX = isPlayerOnFar ? predictX * 2f : predictX * 6f;
        Vector2 summonLast = new Vector2(player.transform.position.x + 1.7f + predictX, player.transform.position.y);
        GameObject sword = Instantiate(swordPrefab, summonLast, Quaternion.Euler(0, 0, -90));
        summonLast = new Vector2(player.transform.position.x - 1.7f + predictX, player.transform.position.y);
        GameObject sword2 = Instantiate(swordPrefab, summonLast, Quaternion.Euler(0, 0, 90));
        if (!isPlayerOnFar)
        {
            layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, sword.transform);
            layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, sword2.transform);
        }
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossPortal, sword.transform.position);
        //audioManager.ThreeDSound(AudioManager.SoundList.HeightBossSword, sword.transform.position);
        //audioManager.ThreeDSound(AudioManager.SoundList.HeightBossSword, sword2.transform.position);

        yield return new WaitForSeconds(0.6f);

        if (swordAmount > 0)
            StartCoroutine(SpecialSwordHorizontal());
        else
            swordAmount = swordAmountMax;
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

    private bool isBossRightSide()
    {
        float aDistance = Vector2.Distance(transform.position, leftPosition.transform.position);
        float bDistance = Vector2.Distance(transform.position, rightPosition.transform.position);
        return aDistance > bDistance;
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

    public IEnumerator DodgeCoroutine()
    {
        animator.SetTrigger("Blink");
        BoxCollider2D collider = GetComponentInChildren<BoxCollider2D>();

        yield return new WaitForSeconds(0.25f);

        collider.enabled = false;

        yield return new WaitForSeconds(0.25f);

        collider.enabled = true;
        if(state != State.Attack)
            transform.localPosition = GetDodgePoint().localPosition;
    }

    private IEnumerator DissableColliderForTime(float colTime)
    {
        BoxCollider2D collider = GetComponentInChildren<BoxCollider2D>();

        yield return new WaitForSeconds(colTime);

        collider.enabled = false;

        yield return new WaitForSeconds(colTime);

        collider.enabled = true;
    }

    private Transform GetDodgePoint()
    {
        bool tpToFar = Random.Range(1, 3) == 1 ? true : false;
        if (!tpToFar)
        {
            if (isOnFarLayer) SwitchLayers();

            if (IsRightSide()) return GetRandomInt(2) == 1 ? leftPosition : quakePosLeft;
            else return GetRandomInt(2) == 1 ? rightPosition : quakePosRight;
        }
        else
        {
            if (!isOnFarLayer) SwitchLayers();

            if (IsRightSide()) return GetRandomInt(2) == 1 ? leftPosClose : quakePosLeftClose;
            else return GetRandomInt(2) == 1 ? rightPosClose : quakePosRightClose;
        }
    }

    private Transform GetQuakePoint()
    {
        if(currentQuake == 0)
        {
            if (layerSwitcher.IsOnFarSide())
            {
                didQuakeOnFar = true;
                if (isOnFarLayer) SwitchLayers();

                if(!isSeondQuake)
                {
                    if (!IsRightSide()) return quakePosLeft;
                    else return quakePosRight;
                }
                else return isFirstQuakeRight ? quakePosLeft : quakePosRight;
            }
            else
            {
                didQuakeOnFar = false;
                if (!isOnFarLayer) SwitchLayers();

                if (!isSeondQuake)
                {
                    if (!IsRightSide()) return quakePosLeftClose;
                    else return quakePosRightClose;
                }
                else return isFirstQuakeRight ? quakePosLeftClose : quakePosRightClose;
            }
        }
        else
        {
            SwitchLayers();
            if (didQuakeOnFar)
                return didQuakeOnRight ? quakePosRightClose : quakePosLeftClose;
            else
                return didQuakeOnRight ? quakePosRight : quakePosLeft;
        }
    }

    private int GetRandomInt(int maxValue)
    {
        return Random.Range(1, maxValue + 1);
    }

    private void DodgeStart()
    {
        if (!canDodge) return;
        int i = Random.Range(1, 11); // 1 - 10.
        if (i < 3)
        {
            StartCoroutine(DodgeCoroutine());
            audioManager.ThreeDSound(AudioManager.SoundList.HeightBossDodge, transform.position);
        }
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
        StopAllCoroutines();
        StartCoroutine(blackOverlay.GetComponent<FadeIn>().FadeTo(0f, 0.6f));
        StartCoroutine(DeathTeleports());
        audioManager.FadeOutCurrent(3f);
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
        if (state == State.Attack) return;

        if (hp < phaseTwoHp && currentPhase == 1)
        {
            currentPhase++;
            isQuaking = true;
            StartCoroutine(DodgeCoroutine());
        }
        if (hp < phaseThreeHp && currentPhase == 2)
        {
            currentPhase++;
            isQuaking = true;
            StartCoroutine(DodgeCoroutine());
        }
        if(!usedSpecialMove && hp < specialMoveHp)
        {
            usedSpecialMove = true;
            StartCoroutine(StartSpecialMove());
        }
        DodgeStart();
    }

    private void OnDisable()
    {
        if (state == State.Dead)
            Destroy(gameObject);
    }


    public void PlayerHasRespawned()
    {
        if (state == State.Dead || state == State.Waiting) return;

        if (!playerRespawned)
        {
            if (isDashing)
                StartCoroutine(StopDash());
            state = State.Waiting;
            playerRespawned = true;
            StartCoroutine(SetBack());
            StopCoroutine(cooldownCoroutine);
            HandleGroundRespawn();
            RevertEdgeColliders();
            enemy.GetComponent<Enemy>().PlayerRespawned();
        }

        transform.localPosition = originalPos;

        currentPhase = 1;
    }

    private void HandleGroundRespawn()
    {
        GroundRespawnHelper(quakeObjectRight);
        GroundRespawnHelper(quakeObjectLeft);
        GroundRespawnHelper(quakeObjectRightClose);
        GroundRespawnHelper(quakeObjectLeftClose);
        GroundRespawnHelper(startQuakeObject);
        triggerFightObject.SetActive(true);
        triggerFightObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    void GroundRespawnHelper(GameObject gObject)
    {
        gObject.SetActive(true);
        foreach (Transform child in gObject.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private IEnumerator DeathTeleports()
    {
        deathBlinkNumber--;
        animator.SetTrigger("Blink");
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossDodge, transform.position);

        yield return new WaitForSeconds(deathTpWait);

        deathTpWait = deathTpWait * 0.95f;
        transform.localPosition = DeathTPPoint().localPosition;

        if (deathBlinkNumber > 0)
        {
            StartCoroutine(DeathTeleports());
        }
        else
        {
            animator.SetBool("IsDead", true);
            boxCollider.enabled = false;
            state = State.Dead;
            yield return new WaitForSeconds(1.5f + 0.5f - deathTpWait);
            enemy.constraints = ~RigidbodyConstraints2D.FreezePositionY;
            enemy.velocity = Vector2.down * 20f;
            //audioManager.AddSoundToGameObject(AudioManager.SoundList.HeightBossDead, transform, 20f);
            GetComponent<AudioClipsGameObject>().PlayAudioSource(0);

            bossDied.Invoke();
            bg_Gameobject.enabled = true;
            audioManager.StartFadeInSource(bg_Gameobject, 2f);

            yield return new WaitForSeconds(20f);

            GetComponent<AudioClipsGameObject>().StopAudioSource(0);
        }
    }

    public void PlayTPsfx()
    {
        audioManager.ThreeDSound(AudioManager.SoundList.HeightBossTeleport, transform.position);
    }

    private Transform DeathTPPoint()
    {
        
        bool tpToFar = Random.Range(1, 3) == 1 ? true : false;
        bool tpRight = Random.Range(1, 3) == 1 ? true : false;
        if (!tpToFar)
        {
            if (isOnFarLayer) SwitchLayers();

            if (deathBlinkNumber <= 0) return GetRandomInt(2) == 1 ? quakePosLeft : quakePosRight;
            if (tpRight) return GetRandomInt(2) == 1 ? leftPosition : quakePosLeft;
            else return GetRandomInt(2) == 1 ? rightPosition : quakePosRight;
        }
        else
        {
            if (!isOnFarLayer) SwitchLayers();

            if (deathBlinkNumber <= 0) return GetRandomInt(2) == 1 ? quakePosLeftClose : quakePosRightClose;
            if (tpRight) return GetRandomInt(2) == 1 ? leftPosClose : quakePosLeftClose;
            else return GetRandomInt(2) == 1 ? rightPosClose : quakePosRightClose;
        }
    }

    private IEnumerator SetBack()
    {
        playerRespawned = true;
       // AudioManager.instance.PlayTheme(AudioManager.SoundList.Fear2, 10f); play the song for the level

        yield return new WaitForSeconds(.35f);

        playerRespawned = false;
        if (isOnFarLayer)
            SwitchLayers();

        audioManager.FadeOutCurrent(6f);
        transform.position = originalPos;
    }
}
