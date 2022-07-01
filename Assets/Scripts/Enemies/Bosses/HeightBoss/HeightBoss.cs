using System.Collections;
using UnityEngine;

public class HeightBoss : MonoBehaviour, ISFXResetable, IKnockbackable, IPhaseable<float>, IRespawnResetable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPointGround;
    private bool facingRight = true, playerRespawned = false, isOnFarLayer;
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
    public float stompShakeTime, stompShakeForce, dashSpeed, swordDelay, swordHeight, cannonDelay, cannonHeight, cannonBallSpeed, fanDuration, fanParticleSpeed;
    private float dashSpeedMultiplier;
    public int swordAmountMax, cannonAmountMax, playerMassRunToFan;
    private int currentSkillsInt, swordAmount, cannonAmount, cannonDivide, currentFanAngle, currentQuake;
    private Vector2 currentSpawnPoint, dashDir, dashEndPos;
    private bool isDashing, isFanActive, didQuakeOnFar, didQuakeOnRight, isSeondQuake, isFirstQuakeRight;

    [Header("Positions")]
    [Space]
    public Transform leftPosition;
    public Transform rightPosition, quakePosLeft, quakePosRight, leftPosClose, rightPosClose, quakePosLeftClose, quakePosRightClose;
    [SerializeField]
    private GameObject quakeObjectRight, quakeObjectLeft, quakeObjectRightClose, quakeObjectLeftClose;

    [Header("Cooldowns")]
    [Space]
    public float cannonSummonCD;


    private System.Action currentSkill;
    public Animator animator;
    public LayerSwitcher layerSwitcher;
    private BoxCollider2D boxCollider;
    public UnityEngine.Events.UnityEvent bossFightTriggered, bossDied;
    public Transform enemiesParent;
    GameObject player;
    [Header("Summon Prefabs")]
    [Space]
    [SerializeField]
    private GameObject swordPrefab;
    [SerializeField]
    private GameObject cannonPrefab, fanPrefab, cannonBallPrefab, quakePrefab;

    private State state;

    private enum State
    {
        Waiting,
        Normal,
        Attack,
        Stunned,
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
            ForceFlip();
        originalPos = transform.position;
        isOnFarLayer = true;
        SwitchLayers();
        HandleSkillPrefabs();
    }

    void HandleSkillPrefabs()
    {
        swordAmount = swordAmountMax;
        cannonAmount = cannonAmountMax;
    }

    private void Awake()
    {
        state = State.Waiting;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
            StartCoroutine(StartQuake());
        if (Input.GetKeyDown(KeyCode.J))
            StartCoroutine(SummonSword());
        if (Input.GetKeyDown(KeyCode.M))
            StartCoroutine(DashCoroutine());
        if (Input.GetKeyDown(KeyCode.N))
            StartCoroutine(SummonFan());
        if (Input.GetKeyDown(KeyCode.B))
            StartCoroutine(SummonCannon());
        if (Input.GetKeyDown(KeyCode.O))
            SwitchLayers();


        if (state == State.Normal)
        {
            if (Mathf.Abs(transform.position.x - player.transform.position.x) < 8 && turnAroundTimer <= 0 && (Mathf.Abs(transform.position.y - player.transform.position.y) < 2))
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
        GetComponent<Enemy>().facingRight = facingRight;

        // handle collider for layer switcher
        if (layerSwitcher.ShouldDisableCollider(isOnFarLayer) && boxCollider.enabled)
            boxCollider.enabled = false;
        else if (!layerSwitcher.ShouldDisableCollider(isOnFarLayer) && boxCollider.enabled == false)
            boxCollider.enabled = true;

        if(isFanActive)
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
        currentSkillsInt = 60;
        currentPhase = 1;
    }

    private void SwitchLayers()
    {
        layerSwitcher.HandleBoss(1f, 1.95f, isOnFarLayer, transform);
        isOnFarLayer = !isOnFarLayer;
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
        state = State.Attack;
        /*dashDir = IsRightSide() ? Vector2.left : Vector2.right;
        if(IsRightSide())
        {
            dashDir = Vector2.left;
            // tp to right
            // sfx dash
            // 
        }*/
    }

    private IEnumerator DashCoroutine()
    {
        Vector2 tpPosition;
        animator.SetTrigger("Blink");

        yield return new WaitForSeconds(0.5f);

        if (ShouldSwitchLanes()) SwitchLayers();

        if (IsRightSide())
        {
            dashDir = Vector2.left;
            if(!isOnFarLayer)
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
            if(facingRight)
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
        isDashing = true;
    }

    private IEnumerator StopDash()
    {
        animator.SetBool("DashStop", true);
        enemy.velocity = Vector2.zero;
        isDashing = false;
        state = State.Normal;

        yield return new WaitForSeconds(0.1f);

        animator.SetBool("DashStop", false);
        //  animator.SetBool("DashStop", false);
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


    private IEnumerator SummonSword()
    {
        swordAmount--;
        bool isPlayerOnFar = layerSwitcher.IsOnFarSide();

        if (swordAmount % 4 == 0 && swordAmount != swordAmountMax)
        {
            /* int rnd = GetRandomInt(2) == 1 ? 1 : -1;
             Vector2 summonLast = new Vector2(player.transform.position.x + swordHeight * rnd, player.transform.position.y);
             PrefabManager.instance.VFXAngle(PrefabManager.ListOfVFX.SwordSummon, summonLast, 90 * rnd);*/
            float predictX = player.GetComponent<MovementPlatformer>().moveInput;
            if (predictX != 0)
                predictX = isPlayerOnFar ? predictX * 2f : predictX * 6f;
            Vector2 summonLast = new Vector2(player.transform.position.x + 1.7f + predictX, player.transform.position.y);
           // PrefabManager.instance.VFXAngle(PrefabManager.ListOfVFX.SwordSummon, summonLast, -90);
            GameObject sword = Instantiate(swordPrefab, summonLast, Quaternion.Euler(0, 0, -90));
            summonLast = new Vector2(player.transform.position.x - 1.7f + predictX, player.transform.position.y);
            // PrefabManager.instance.VFXAngle(PrefabManager.ListOfVFX.SwordSummon, summonLast, 90);
            GameObject sword2 = Instantiate(swordPrefab, summonLast, Quaternion.Euler(0, 0, 90));
            if(!isPlayerOnFar)
            {
                layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, sword.transform);
                layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, sword2.transform);
            }
        }
        else
        {
            Vector2 summonPoint = new Vector2(player.transform.position.x, player.transform.position.y + swordHeight);
            // PrefabManager.instance.PlayVFX(PrefabManager.ListOfVFX.SwordSummon, summonPoint);
            GameObject sword = Instantiate(swordPrefab, summonPoint, Quaternion.identity);
            if (!isPlayerOnFar)
                layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, sword.transform);
        }

        yield return new WaitForSeconds(swordDelay);

        if (swordAmount > 0)
            StartCoroutine(SummonSword());
        else
            swordAmount = swordAmountMax;
    }

    private int rndDiv;
    private IEnumerator SummonCannon()
    {
        if (cannonAmount == cannonAmountMax)
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

        yield return new WaitForSeconds(cannonDelay);

        if (cannonAmount > 0)
            StartCoroutine(SummonCannon());
        else
            cannonAmount = cannonAmountMax;
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
        layerSwitcher.HandleBoss(1f, 1.4f, isPlayerOnFar, fan.transform);
        fan.GetComponent<BoxCollider2D>().enabled = layerSwitcher.ShouldDisableCollider(isPlayerOnFar);
        fan.GetComponent<Animator>().SetBool("IsActive", true);

        yield return new WaitForSeconds(0.1f);

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

        yield return new WaitForSeconds(fanDuration);

        fan.GetComponent<Animator>().SetBool("IsActive", false);
        effector.enabled = false;
        isFanActive = false;
        particles.Stop();
    }
    
    private IEnumerator StartQuake()
    {
        animator.SetTrigger("Blink");

        yield return new WaitForSeconds(0.5f);

        transform.localPosition = GetQuakePoint().localPosition;
        if (currentQuake == 0)
            didQuakeOnRight = isBossRightSide();
        if(isSeondQuake) isFirstQuakeRight = didQuakeOnRight;
        bool isPlayerOnFar = !isOnFarLayer;

        yield return new WaitForSeconds(0.5f);

        Vector2 quakePos = QuakePoint();
        animator.SetTrigger("Quake");
        animator.SetBool("QuakeStop", false);
        enemy.velocity = Vector2.up * 3;

        yield return new WaitForSeconds(0.4f);

        enemy.velocity = Vector2.down * 80f;

        yield return new WaitForSeconds(0.2f);

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
        if (!layerSwitcher.ShouldDisableCollider(isPlayerOnFar))
            quakeVFX.GetComponent<CheckForHit>().CallThisToIgnoreCheck();
        enemy.velocity = Vector2.zero;
        animator.SetBool("QuakeStop", true);
        DestroyGround();

        if (currentQuake == 1)
        {
            currentQuake = 0;
            StartCoroutine(DodgeCoroutine());
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
            Destroy(child.gameObject);
        }
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

    private void QuakeMove()
    {
        enemy.velocity = Vector2.down * 20f;
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

        yield return new WaitForSeconds(0.5f);

        transform.localPosition = GetDodgePoint().localPosition;
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
