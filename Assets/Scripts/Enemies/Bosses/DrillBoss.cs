using System.Collections;
using UnityEngine;

public class DrillBoss : MonoBehaviour, ISFXResetable, IPhaseable<float>, IRespawnResetable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPointGround;
    private bool facingRight = false, playerRespawned = false, fightHasStarted;
    private RaycastHit2D groundCheckRaycast;
    private Vector2 originalPos;
    public float wallCheckDistance = 1, playerMassRunWind, spriteSize;
    public float groundCheckDistance = 1, phaseTwoHp, phaseThreeHp;
    private int layerMaskGround = 1 << 8, currentPhase = 1;

    [Header("Attacking")]
    [Space]
    public float projectileSpeed;
    public float moveSpeed, sideDrillY, sideDrillXLeft, sideDrillXRight;
    private int current_sideDrillAmount;
    public int sideDrillAmount;
    public Transform leftPosition, rightPosition;
    private Vector2 moveTowardsPoint, drillSpecialPos;
    private bool isWindActive, drillToLeft, isChangingPhase, isPreMoving;
    private IEnumerator stopDrillCoroutine;
    private float cooldownTimer, orgSize;
    private Color orgColor;

    [Header("Positions")]
    [Space]

    [SerializeField]
    private Transform topMovePos;
    [SerializeField]
    private Transform shootPosLeft, shootPosRight, moveTowardsSide, specialMovePos, secondIdlePos;
    [SerializeField]
    private Transform[] jumpStonePositions;

    [Header("Objects Cashing")]
    [Space]

    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private GameObject floaterProjectile, drillParticles, windParticles, specialPrefab, specialEndObject, createJumpstonePrefab, blackOverlay, specialDrillEndPrefab;


    [Header("Cooldowns")]
    [Space]
    public float cannonSummonCD;


    public Animator animator;
    private RuntimeAnimatorController orgAnimator;
    public UnityEngine.Events.UnityEvent bossFightTriggered, bossDied, playerDied;
    private AudioManager audio_M;
    public Transform enemiesParent;
    [SerializeField]
    private Transform windCollidersParent, jumpStonesParent;
    GameObject player;
    [SerializeField]
    private RuntimeAnimatorController animatorSecond;

    private State state;

    private enum State
    {
        Waiting,
        Normal,
        Attack,
        Stunned,
        Moving,
        SideMoving,
        Dead,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        player = GameMaster.instance.playerInstance;
        GetComponent<Enemy>().usePhases = true;
        bool goRight = GetComponent<Enemy>().goRight;
        if ((goRight && !facingRight) || (!goRight && facingRight))
            Flip();
        originalPos = transform.position;
        stopDrillCoroutine = StopDrillDown();
        orgAnimator = animator.runtimeAnimatorController;
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
            if (Mathf.Abs(transform.position.x - player.transform.position.x) < 8)
            {
                ForceFlip();
            }

            HandleCooldown();
        }

        HandleEnemyClassObjects();
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Waiting:
                break;
            case State.Normal:
               //StartCoroutine(PauseBeforeAttack(pauseBeforeAttack));
                break;
            case State.Attack:
                if (groundCheckRaycast)
                    StartCoroutine(stopDrillCoroutine);
                break;
            case State.Moving:
                MoveTop();
                break;
            case State.SideMoving:
                MoveSide();
                break;
        }

        HandleRaycasts();

        //    Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //     Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //    Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, boxCollider.bounds.extents.y + groundCheckDistance, 0), Vector2.right * (boxCollider.bounds.extents.x + groundCheckDistance));
    }

    private void HandleRaycasts()
    {
        groundCheckRaycast = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, layerMaskGround);
    }

    private void HandleCooldown()
    {
        if(cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else
        {
            if(!isChangingPhase)
                SetActionTrigger();
            else
            {
                isChangingPhase = false;
                if (currentPhase == 2) Shoot2nd();
                else StartThirdPhase();
            }
        }
    }

    private IEnumerator StartFightCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        enemy.velocity = Vector2.up * 20f;

        yield return new WaitForSeconds(3f);

        enemy.velocity = Vector2.zero;

        SpriteRenderer s_Renderer = GetComponent<SpriteRenderer>();
        orgSize = transform.localScale.x;
        orgColor = s_Renderer.color;
        transform.localScale = new Vector2(spriteSize, spriteSize);
        s_Renderer.color = Color.white;
        s_Renderer.sortingLayerName = "Default";
        GetComponent<BoxCollider2D>().enabled = true;
        /*enemy.transform.position = new Vector2(specialMovePos.position.x, specialMovePos.position.y + 10f);
        MoveSetUp(specialMovePos.localPosition, true);*/
        StartDrillDown();
    }

    void ResetSprite()
    {
        SpriteRenderer s_Renderer = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector2(orgSize, orgSize);
        s_Renderer.color = orgColor;
        s_Renderer.sortingLayerName = "Foreground";
        GetComponent<BoxCollider2D>().enabled = false;
    }

    public void StartFight()
    {
        if(!fightHasStarted)
        {
            fightHasStarted = true;
            StartCoroutine(StartFightCoroutine());
            bossFightTriggered.Invoke();
            audio_M.StartTransitionCoroutine(AudioManager.SoundList.DrillBoss_bg_Start, AudioManager.SoundList.DrillBoss_bg_loop, 10f);
        }
    }

    private void HandleEnemyClassObjects()
    {
        GetComponent<Enemy>().facingRight = facingRight;

        if (isWindActive)
        {
            float playerXInput = player.GetComponent<MovementPlatformer>().moveInput;
            Rigidbody2D playerRB = player.GetComponent<Rigidbody2D>();
            if (playerXInput != 0)
                playerRB.mass = playerMassRunWind;
            else
                playerRB.mass = 3;
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
        windCollidersParent.Rotate(0.0f, 180.0f, 0.0f);
    }

    public void ResetSFXCues()
    {
        state = State.Waiting;
        currentPhase = 1;

        //clean orbs
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
        int i = Random.Range(1, 65); 

        if (1 <= i && i <= 40)
        {
            StartDrillDown();
        }
        else if (41 <= i && i <= 65)
        {
            if (currentPhase == 1)
                StartShooting();
            else
                StartSideDrill();
        }
    }

    
    void StartShooting()
    {
        state = State.Stunned;
        ForceFlip();
        animator.SetTrigger("Shoot");
        audio_M.PlaySound(AudioManager.SoundList.DrillBoss_Shoot);
    }

    void Shoot2nd()
    {
        state = State.Stunned;
        ForceFlip();
        animator.SetTrigger("Shoot2nd");
        audio_M.PlaySound(AudioManager.SoundList.DrillBoss_Shoot);
    }

    private void MoveTop()
    {
        if (Vector2.Distance(transform.localPosition, moveTowardsPoint) > 0.5f)
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, moveTowardsPoint, moveSpeed * Time.deltaTime);
        else
        {
            if (isPreMoving)
            {
                state = State.Normal;
                isPreMoving = false;
                GetComponent<AfterImage>().enabled = false;
                return;
            }

            state = State.Attack;
            animator.SetTrigger("StartDrill");
            audio_M.PlaySound(AudioManager.SoundList.DrillBoss_Prepare);
            GetComponent<AudioClipsGameObject>().PlayAudioSource(1);
        }
    }

    private void MoveSide()
    {
        if (Vector2.Distance(transform.localPosition, moveTowardsPoint) > 1f)
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, moveTowardsPoint, moveSpeed * Time.deltaTime);
        else
        {
            if (current_sideDrillAmount < 0)
            {
                animator.SetBool("SideDrill", false);
                RotateCollider(true);
                state = State.Normal;
                cooldownTimer = 1f;
                if(currentPhase != 3)
                    ReturnToHighpoint();
                GetComponent<AudioClipsGameObject>().StopAudioSource(0);
                return;
            }
            else if(current_sideDrillAmount == sideDrillAmount)
            {
                RotateCollider(false);
                GetComponent<AudioClipsGameObject>().PlayAudioSource(0);
            }

            current_sideDrillAmount--;
            drillToLeft = !drillToLeft;
            if (drillToLeft)
                moveTowardsSide.localPosition = new Vector2(sideDrillXLeft, sideDrillY);
            else
                moveTowardsSide.localPosition = new Vector2(sideDrillXRight, sideDrillY);
            MoveSetUp(moveTowardsSide.localPosition, false);
            animator.SetBool("SideDrill", true);
            ForceFlip();
        }
    }

    private void MoveSetUp(Vector2 moveTo, bool isMovingTop)
    {
        if (isMovingTop)
            ForceFlip();
        else
            FlipTowardsSide();

        StopCoroutine(stopDrillCoroutine);
        HandleDrillStopValues();
        moveTowardsPoint = moveTo;
        state = isMovingTop ? State.Moving : State.SideMoving;
        GetComponent<AfterImage>().enabled = true;

        audio_M.PlaySound(AudioManager.SoundList.DrillBoss_Move);
    }

    void FlipTowardsSide()
    {
        if (enemy.transform.position.x > moveTowardsPoint.x && !facingRight)
            Flip();
        else if (enemy.transform.position.x < moveTowardsPoint.x && facingRight)
            Flip();
    }

    void HandleDrillStopValues()
    {
        GameMaster.instance.StopCameraShake();
        isWindActive = false;
        stopDrillCoroutine = StopDrillDown();
        windCollidersParent.gameObject.SetActive(false);
        animator.SetBool("DrillDown", false);
        drillParticles.SetActive(false);
        windParticles.SetActive(false);
        audio_M.PlaySound(AudioManager.SoundList.DrillBoss_StopDrill);
        GetComponent<AudioClipsGameObject>().StopAudioSource(1);
    }

    private void DrillDown()
    {
        state = State.Attack;
        animator.SetBool("DrillDown", true);
        enemy.velocity = Vector2.down * 50f;
    }

    private void StartDrillDown()
    {
        topMovePos.position = new Vector2(player.transform.position.x, topMovePos.position.y);
        MoveSetUp(topMovePos.localPosition, true);
    }

    
    private IEnumerator StopDrillDown()
    {
        enemy.velocity = Vector2.zero;
        state = State.Stunned;
        GameMaster.instance.ShakeCamera(15f, 1.5f); // change for special attack
        drillParticles.SetActive(true);
        GetComponent<AfterImage>().enabled = false;

        if(currentPhase == 2 && !isChangingPhase)
            StartCoroutine(SpawnSpecialDrill());
        if (currentPhase == 3)
        {
            // HandleSpecialObjects();
            StartCoroutine(StartBottomDrills());
            yield break;
        }
        if(currentPhase == 1)
            windCollidersParent.gameObject.SetActive(true);

        windParticles.SetActive(true);
        isWindActive = true;

        yield return new WaitForSeconds(1.5f);

        ResetAfterDrill();
    }

    void ResetAfterDrill()
    {
        state = State.Normal;
        cooldownTimer = 2f; // drillDownTimer
        HandleDrillStopValues();

        ReturnToHighpoint();
    }

    void ReturnToHighpoint()
    {
        cooldownTimer = 2f;
        float phaseOneOffset = currentPhase == 1 ? 4f : 0;
        secondIdlePos.position = new Vector2(topMovePos.transform.position.x, secondIdlePos.position.y - phaseOneOffset);
        MoveSetUp(secondIdlePos.localPosition, true);
        isPreMoving = true;
        secondIdlePos.position = new Vector2(secondIdlePos.transform.position.x, secondIdlePos.position.y + phaseOneOffset);
    }

    private IEnumerator SpawnSpecialDrill()
    {
        yield return new WaitForSeconds(0.25f);

        CreateSpecialDrill();

        yield return new WaitForSeconds(0.35f);

        CreateSpecialDrill();
    }

    private void CreateSpecialDrill()
    {
        float predictX = player.GetComponent<MovementPlatformer>().moveInput * 2f;
        RaycastHit2D rayToGround = Physics2D.Raycast(player.transform.position, Vector2.down, 10f, 1 << 8);
        Vector2 spawnPoint = new Vector2(rayToGround.point.x + predictX, rayToGround.point.y + 0.5f); // offset
        GameObject drillSpawn = Instantiate(specialPrefab, spawnPoint, Quaternion.identity);
        Animator spawnAnimator = drillSpawn.GetComponent<Animator>();
        spawnAnimator.SetBool("Active", false);
        audio_M.PlaySound(AudioManager.SoundList.DrillBoss_BottomDrill);
    }


    private void HandleSpecialObjects()
    {
        specialEndObject.SetActive(true);
        specialEndObject.GetComponent<Animator>().SetBool("Active", true);
    }

    private IEnumerator StartBottomDrills()
    {
        specialEndObject.SetActive(true);
        specialEndObject.GetComponent<Animator>().SetBool("Active", true);
        Animator sAnimator = specialEndObject.GetComponent<Animator>();
        sAnimator.SetBool("Active", true);
        sAnimator.speed = 0.2f;

        yield return new WaitForSeconds(0.5f);

        sAnimator.speed = 1f;
    }

    private void CreateJumpStones()
    {
        foreach (Transform pos in jumpStonePositions)
        {
            GameObject spawner = Instantiate(createJumpstonePrefab, pos.position, Quaternion.identity, jumpStonesParent);
        }
    }

    private IEnumerator SpecialMoveCoroutine()
    {
        StartCoroutine(blackOverlay.GetComponent<FadeIn>().FadeTo(0.8f, 0.5f));

        yield return new WaitForSeconds(0.5f);

        StartSideDrill();
        CreateJumpStones();


        while(state != State.Normal)
        {
            yield return null;
        }

        CreateDrillSpecial();
        enemy.transform.position = new Vector2(specialMovePos.position.x, specialMovePos.position.y + 10f);
        MoveSetUp(specialMovePos.localPosition, true);

        
        yield return new WaitForSeconds(10f);

        GetComponent<Enemy>().enemyDead();
        animator.SetBool("IsDead", true);
        drillParticles.SetActive(false);
        GameMaster.instance.StopCameraShake();
        audio_M.PlayTheme(AudioManager.SoundList.DrillBoss_bg_ending, 1f);

        yield return new WaitForSeconds(1f);

        StartCoroutine(blackOverlay.GetComponent<FadeIn>().FadeTo(0f, 0.5f));
        specialEndObject.GetComponent<Animator>().SetBool("Active", false);

        yield return new WaitForSeconds(1f);

        bossDied.Invoke();
    }

    void CreateDrillSpecial()
    {
        RaycastHit2D rayToFloor = Physics2D.Raycast(specialMovePos.transform.position, Vector2.down, 20f, layerMaskGround);
        GameObject specialDrill = Instantiate(specialDrillEndPrefab, rayToFloor.point, Quaternion.identity, transform.parent);
        specialEndObject = specialDrill;
    }

    private void StartSideDrill()
    {
        current_sideDrillAmount = sideDrillAmount;
        int rndInt = GetRandomInt(2);
        float xPos = rndInt == 1 ? sideDrillXLeft : sideDrillXRight;
        moveTowardsSide.localPosition = new Vector2(xPos, sideDrillY);
        drillToLeft = rndInt == 1 ? false : true;
        MoveSetUp(moveTowardsSide.localPosition, false);
    }

    void Shoot()
    {
        CreateProjectile(shootPosLeft.position, projectilePrefab);
        CreateProjectile(shootPosRight.position, projectilePrefab);

        state = State.Normal;
        cooldownTimer = 2f; // shoot cooldowntimer;
    }

    void StartSecondsPhase()
    {
        CreateProjectile(shootPosLeft.position, floaterProjectile);
        CreateProjectile(shootPosRight.position, floaterProjectile);
        animator.runtimeAnimatorController = animatorSecond;
        currentPhase = 2;
       // moveSpeed = 50f;
        GetComponent<BoxCollider2D>().size = new Vector2(1.1f, 2.56f);

        state = State.Normal;
        cooldownTimer = 2f; // shoot cooldowntimer;
    }

    private void StartThirdPhase()
    {
        //CreateJumpStones();
        StopCoroutine(stopDrillCoroutine);
        state = State.Stunned;
        currentPhase = 3;
        StartCoroutine(SpecialMoveCoroutine());
    }

    void CreateProjectile(Vector3 shootPos, GameObject prefab)
    {
        var pos = shootPos;
        var dir = player.transform.position - pos;
        GameObject projectile = Instantiate(prefab, pos, Quaternion.identity, jumpStonesParent);
        projectile.GetComponent<Action_TriggerHitPlayer>().SetMovement(dir.normalized, projectileSpeed);
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

    public void HandlePhases(float hp) // boss hit 
    {
        //DodgeStart();
        if (state == State.Waiting)
            state = State.Normal;
        if (hp < phaseTwoHp && currentPhase == 1)
        {
            currentPhase++;
            isChangingPhase = true;
            Invoke("ReturnToHighpoint", 0.5f);
        }
        if (hp < phaseThreeHp && currentPhase == 2)
        {
            currentPhase++;
            isChangingPhase = true;
            cooldownTimer = 2f;
            GetComponent<Enemy>().currentHealth = 1000;
        }

        if (state == State.Stunned)
            DodgeStart();
    }

    void DodgeStart()
    {
        GetComponent<Enemy>().currentHealth += 15;
        int i = Random.Range(1, 11); // 1 - 10.
        if (i < 5)
        {
            StopCoroutine(stopDrillCoroutine);
            ResetAfterDrill();
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
        DestroyJumpstones();
        HandleDrillStopValues();
        StopAllCoroutines();
        StartCoroutine(blackOverlay.GetComponent<FadeIn>().FadeTo(0f, 0.5f));
        GetComponent<Enemy>().PlayerRespawned();
        animator.runtimeAnimatorController = orgAnimator;
        fightHasStarted = false;
        ResetSprite();
        playerDied.Invoke();
        if(currentPhase == 3)
            specialEndObject.GetComponent<Animator>().SetBool("Active", false);

        currentPhase = 1;
        AudioManager.instance.FadeOutCurrent(3f);
    }

    void DestroyJumpstones()
    {
        foreach (Transform child in jumpStonesParent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private IEnumerator SetBack()
    {
        playerRespawned = true;
        // AudioManager.instance.PlayTheme(AudioManager.SoundList.Fear2, 10f); play the song for the level

        yield return new WaitForSeconds(.35f);

        playerRespawned = false;
    }

    private int GetRandomInt(int maxValue)
    {
        return Random.Range(1, maxValue + 1);
    }

    private void RotateCollider(bool isVertical)
    {
        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        float boxX = boxCol.size.x;
        float boxY = boxCol.size.y;
        if (isVertical && boxY > boxX)
            return;
        boxCol.size = new Vector2(boxY, boxX);
    }

    public void PlayExplodeSFX()
    {
        audio_M.PlaySound(AudioManager.SoundList.DrillBoss_Explode);
    }
}
