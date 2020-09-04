using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class MovementPlatformer : MonoBehaviour
{

    [Header("Movement")]
    [Space]
    // moving and jumping vars
    public float jumpV;
    [HideInInspector]
    public Vector2 directionPressed;
    [HideInInspector]
    public float jumpMemory = 0.2f, groundedMemory = 0.2f, moveInput, moveInputVertical;
    public float fHorDmpBasic;
    public float fHorDmpStopping;
    public float fHorDmpTurning;
    private float fHorizontalVelocity;
    private float wallSlideMemory;
    public float speedMulitiplier, fallSpeed;


    private bool facingRight = true;
    //[HideInInspector]
    public bool isGrounded, isAirborn;         // bools for jumping 
    private bool isWallSliding = false;
    [HideInInspector]
    public Transform groundCheck, wallCheck;           // objects for checking if the player touches the ground/wall.
    public float wallCheckDistance;
    public float groundCheckDistance;
    private RaycastHit2D wallCheckHit;
    [HideInInspector]
    public LayerMask whatIsGround;          // !

    [Header("Shooting & Teleporting")]
    [Space]

    public bool bulletUnlocked = true;
    public Transform shootingPoint;      // variables for shooting and teleporting
    public GameObject bulletPrefab;
    [HideInInspector]
    public bool canTeleport = false, canShoot = true, shouldICheckIsGrounded = true, didHitAnEnemy = false, bulletHitDoor = false, thrustHappening = false, showPointer;
    public float waitTimeShooting = 0.3f;
    [HideInInspector]
    public float canShootTimer, groundTimer;
    private float shootMemoryTimer;
    [HideInInspector]
    public bool shouldCheckForShootMemory = true;
    public GameObject CurrentBulletGameObject;
    [Header("Shooting Skills")]
    [Space]
    public float thrustPower = 50;
    public float thrustTimer = 0.5f;
    public float topSidesSkillSpeed;
    public float fHorDmpSideSkill;
    public float constantTopSidesSkill;
    public float timerForSkill;
    public float timerForSkillMax;
    public float topSidesSkillTime;
    private bool enemyNearWallRight, enemyNearWallLeft;
    public float enemyHitCheckDistance = 0.1f;
    public float skillTimer, teleportShakeTime, teleportShakeForce;
    private bool isKnockingBack = false;

    // !
    public float distanceTest;


    [Header("Attacking")]
    [Space]

    public Transform regularAttackPoint;    // Variables for attacking. 
    public Transform upAttackPoint;
    public GameObject bulletPointer;
    public GameObject hitVFX;
    public GameObject hitVFXWall;
    private int canBeAttackedLayerMask = (1 << 12) | (1 << 17) | (1 << 30) | (1 << 19);
    public int attackDamage = 40, manaFillPerAttack = 7;
    private float atkAnimationStallTimer;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;
    private float regularAttackTimer, atkAnimationCombo = 0;
    private bool atkAndFlip;        //!

    [Header("General")]
    [Space]

    public Animator animator;
    public ParticleSystem dust;

    private bool playerIsInvulnerable = false, dontRespawn = false, canHeal = true;

    private State state;
    public DirectionPressed directionPressedNow;

    public GameObject currentRoom;
    public Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private GameMaster gm;
    private AudioManager audioManager;
    private InputManager input;
    private bool usingKeyboard = true;
    private ManaBar manaBar;


    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    private enum State              // all of the states available for the character
    {
        Normal,
        WallJumping,
        DashingToEnemy,
        IgnorePlayerInput,
    }

    public enum DirectionPressed
    {
        Nothing,
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        gm = GameMaster.instance;
        audioManager = AudioManager.instance;
        input = InputManager.instance;
        manaBar = GetComponent<ManaBar>();
        audioManager.PlayTheme(AudioManager.SoundList.FirstTestSong, 0.1f); /////// test song
        //  delete when building!!!!
        gm.savePointPosition = transform.position;
        //Time.timeScale = 0.9f;
    }

    private void Awake()
    {
        SetStateNormal();
        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

    }

    void Update()
    {

        if (Time.timeScale == 0)
            return;

        //HandleAnimations();     // General functions
        Timers();
        JumpMemory();

        switch (state)
        {
            case State.Normal:              // actions available in normal state
                FlipStart();
                HandleAnimations();
                MoveStart();
                WallSlideCheck();
                //WallJumpStart();
                SwitchStates();
                ShootMemory();
                CheckDirectionPressed();
                AttackMemory();
                if (input.KeyDown(Keybindings.KeyList.ResetBullet))
                    KillBulletObject();
                if (regularAttackTimer > 0 && Time.time >= nextAttackTime)
                {
                    AttackRegular();
                    nextAttackTime = Time.time + 1f / attackRate;
                }
                if (shootMemoryTimer > 0 && !canTeleport)
                    ShootStart();
                else if (input.KeyDown(Keybindings.KeyList.Shoot) && canTeleport)
                    Teleport();
                if (canHeal && input.KeyDown(Keybindings.KeyList.Heal) && manaBar.HaveEnoughMana(25) && !GetComponent<Health>().IsFullHealth())
                    StartCoroutine(StartHeal());
                break;
            case State.DashingToEnemy:
                HandleAnimations();
                break;
            case State.IgnorePlayerInput:
                break;
        }

        /* if (isUsingTopNSideTeleport && InputManager.instance.KeyDown(Keybindings.KeyList.Attack))
         {
             state = State.DashingToEnemy;
             Flip();
             Debug.Log("hi");
         }*/
         

        if (shouldICheckIsGrounded)
            GroundCheckForShoot();

        if (input.KeyDown(Keybindings.KeyList.Attack) && canShootTimer <= 0 && !isGrounded)
            canShoot = false;
        else if (canShootTimer <= 0 && isGrounded)
            shouldICheckIsGrounded = true;

        if (showPointer)
            RotatePointer();
    }

    void FixedUpdate()
    {
        HandleWallSlide();
        RayCastsAndChecks();

        switch (state)
        {
            case State.Normal:
                Move();
                JumpNow();
                break;
            case State.DashingToEnemy:
                DashToEnemy();
                JumpMemory();
                break;
            case State.IgnorePlayerInput:
                break;
        }
        if(isAirborn && (rb.velocity.y < -fallSpeed))
        {
            rb.velocity = new Vector2(rb.velocity.x, -fallSpeed);
        }
    }

    private void SetStateNormal()
    {
        state = State.Normal;
    }

    private void RayCastsAndChecks()
    {
        isGrounded = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(boxCollider.bounds.size.x - 0.01f, boxCollider.bounds.size.y), 0, Vector2.down, groundCheckDistance, whatIsGround);

        wallCheckHit = Physics2D.Raycast(wallCheck.position, wallCheck.right, wallCheckDistance, whatIsGround);

        if (CurrentBulletGameObject != null && (CurrentBulletGameObject.layer == 12 || CurrentBulletGameObject.layer == 19)) // 12 is enemy 19 is hollow
        {
            BoxCollider2D EnemyBoxColl = CurrentBulletGameObject.GetComponent<BoxCollider2D>();

            enemyNearWallRight = Physics2D.BoxCast(EnemyBoxColl.bounds.center, new Vector2(EnemyBoxColl.bounds.size.x - 0.1f, EnemyBoxColl.bounds.size.y - 0.39f), 0, Vector2.right, distanceTest, whatIsGround);
            enemyNearWallLeft = Physics2D.BoxCast(EnemyBoxColl.bounds.center, new Vector2(EnemyBoxColl.bounds.size.x - 0.1f, EnemyBoxColl.bounds.size.y - 0.39f), 0, Vector2.left, distanceTest, whatIsGround);
        }
    }

    private void HandleMoveInput()
    {
        if (usingKeyboard)
        {
            if (input.GetKey(Keybindings.KeyList.Up))
                moveInputVertical = 1;
            if (input.GetKey(Keybindings.KeyList.Down))
                moveInputVertical = -1;
            if (input.GetKey(Keybindings.KeyList.Right))
                moveInput = 1;
            if (input.GetKey(Keybindings.KeyList.Left))
                moveInput = -1;
            if (input.KeyUp(Keybindings.KeyList.Up))
                moveInputVertical = 0;
            if (input.KeyUp(Keybindings.KeyList.Down))
                moveInputVertical = 0;
            if (input.KeyUp(Keybindings.KeyList.Right))
                moveInput = 0;
            if (input.KeyUp(Keybindings.KeyList.Left))
                moveInput = 0;
        }
        else
        {
            moveInput = Input.GetAxisRaw("Horizontal");
            moveInputVertical = Input.GetAxisRaw("Vertical");

            if (moveInput > 0) moveInput = 1;
            else if (moveInput < 0) moveInput = -1;

            if (moveInputVertical > 0) moveInputVertical = 1;
            else if (moveInputVertical < 0) moveInputVertical = -1;
        }
    }

    public void SwitchToOrFromJoystick()
    {
        usingKeyboard = !usingKeyboard;
    }

    
    private void Timers()
    {
        if (canShootTimer > 0)
        {
            canShootTimer -= Time.deltaTime;
            if (!isGrounded)
                canShoot = false;
        }

        if (regularAttackTimer > 0)
            regularAttackTimer -= Time.deltaTime;

        if (atkAnimationStallTimer > 0)
            atkAnimationStallTimer -= 1;

        if (atkAnimationCombo > 0)
            atkAnimationCombo -= Time.deltaTime;
    }


    public void GotHitByAnEnemy(int damage)
    {
        //Make Player Invincible
        //Lower player's HP
        //Hurt Animation
        //Knock back the player
        if (!playerIsInvulnerable)
        {
            StartCoroutine(MakePlayerInvincible(1.5f));
            StartCoroutine(PlayerBlinkingAnimation());
            audioManager.PlaySound(AudioManager.SoundList.PlayerHit);
            StartCoroutine(FreezeGameForTime(0.2f));
            GetComponent<Health>().DealDamage(damage);
        }
    }

    public void KnockBackPlayer(float knockBackForce, float xKnockBack, float yKnockBack, bool isKnockedBackRight)
    {
        if (!playerIsInvulnerable)
        {
            StartCoroutine(StartSwitchStateToIgnoreInputs(0.2f, true));
            if (isKnockedBackRight)
                rb.AddForce(new Vector2(xKnockBack, yKnockBack) * knockBackForce, ForceMode2D.Impulse);
            else
                rb.AddForce(new Vector2(-xKnockBack, yKnockBack) * knockBackForce, ForceMode2D.Impulse);
            moveInput = 0;
            moveInputVertical = 0;
        }
    }


    private void HandleAnimations()
    {
        if (rb.velocity.y > 0 && !isGrounded && atkAnimationStallTimer <= 0)
            animator.SetBool("IsJumping", true);
        else if (rb.velocity.y < 0 && !isGrounded && atkAnimationStallTimer <= 0)
            animator.SetBool("IsFalling", true);
        else if (rb.velocity.y == 0)
            animator.SetBool("IsJumping", false);

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
    }

    private void FlipStart()
    {
        if (facingRight == false && moveInput > 0 && atkAnimationStallTimer <= 0)     //flip the character while switching directions
            Flip();

        else if (facingRight == true && moveInput < 0 && atkAnimationStallTimer <= 0)
            Flip();
    }
    void Flip()
    {
        if (!thrustHappening)
        {
            facingRight = !facingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void MoveStart()
    {

        //    moveInput = Input.GetAxisRaw("Horizontal");        //   Felt old might delete later :)
        //  moveInputVertical = Input.GetAxisRaw("Vertical");
        //directionPressed = new Vector2(moveInput, moveInputVertical);

        HandleMoveInput();
        directionPressed = DirectionPressedV();
    }
    private Vector2 DirectionPressedV()
    {
        int hori;
        int vert;
        if (moveInput == 0)
            hori = 0;
        else
            hori = moveInput > 0 ? 1 : -1;
        if (moveInputVertical == 0)
            vert = 0;
        else
            vert = moveInputVertical > 0 ? 1 : -1;
        return new Vector2(hori, vert);
    }
    private void Move()
    {
        fHorizontalVelocity = rb.velocity.x;
        fHorizontalVelocity += moveInput;

        if (Mathf.Abs(moveInput) < 0.01f)
            fHorizontalVelocity *= Mathf.Pow(1f - fHorDmpStopping, Time.deltaTime * speedMulitiplier);
        else if (Mathf.Sign(moveInput) != Mathf.Sign(fHorizontalVelocity))
            fHorizontalVelocity *= Mathf.Pow(1f - fHorDmpTurning, Time.deltaTime * speedMulitiplier);
        else
            fHorizontalVelocity *= Mathf.Pow(1f - fHorDmpBasic, Time.deltaTime * speedMulitiplier);

        rb.velocity = new Vector2(fHorizontalVelocity, rb.velocity.y);

        /*    this is bad and not good and also bad but maybe ill use that again.
        if (isUsingTopNSideTeleport == true)
        {
            if (timerForSkill > 0)
                timerForSkill -= Time.deltaTime;
            else
                topSidesSkillSpeed *= Mathf.Pow(1f - fHorDmpSideSkill, 2 * Time.deltaTime);

            if (rb.velocity.y > 0)
                rb.velocity = new Vector2(topSidesSkillSpeed, rb.velocity.y);
            else
                rb.velocity = new Vector2(topSidesSkillSpeed, 0);
        }*/


    }

    void JumpMemory()
    {

        if (jumpMemory > 0)
            jumpMemory -= Time.deltaTime;

        if (InputManager.instance.KeyDown(Keybindings.KeyList.Jump))
        {
            jumpMemory = 0.2f;
        }
        if (isGrounded && isAirborn)
        {
            isAirborn = false;
            groundedMemory = 0.05f;
            animator.SetBool("IsFalling", false);
            GetComponent<Footsteps>().PlayerLanded();
            CreateDust();
            OnLandEvent.Invoke();
        }
        else if (!isGrounded)
        {
            if (groundedMemory > 0)
                groundedMemory -= Time.deltaTime;
            else isAirborn = true;
        }

    }       // preparing the conditions to the jump

    public void JumpNow()  // the action of jumping
    {
        if ((jumpMemory > 0) && (groundedMemory > 0))
        {
            groundedMemory = 0;
            jumpMemory = 0;
            //rb.velocity = Vector2.up * jumpV;
            rb.velocity = new Vector2(rb.velocity.x, 1 * jumpV);
            CreateDust();
            audioManager.PlaySound(AudioManager.SoundList.PlayerJump);
        }
    } 

    public Vector2 GetPosition()
    {
        return transform.position;
    }  // gets the position of the player

    public void WallSlideCheck()
    {
        // wallSlideDirectionInput = Input.GetAxis("Horizontal");
        if (wallCheckHit && rb.velocity.y <= 0 && !isGrounded && moveInput != 0)
        {
            //    wallSlideDuration = 2f;
            wallSlideMemory = 0.2f;
        }
        else if (moveInput != 1 && moveInput != -1)
            wallSlideMemory = 0;

        // היי דדי של מחר, תכניס את הבוליאן של הסלידינג לתוך האם למעלה ושתהנ את הפיקס אפדייט

        if (wallSlideMemory > 0)
            isWallSliding = true;
        else if (wallSlideMemory <= 0)
            isWallSliding = false;

    }


    private void HandleWallSlide()
    {
        if (!wallCheckHit && wallSlideMemory > 0)
            wallSlideMemory -= Time.deltaTime;

        if (isWallSliding && wallCheckHit)
            rb.drag = 10f;
        else if (!isWallSliding || !wallCheckHit)
            rb.drag = 0f;
    }

    public void SwitchStates()
    {
    }
    

    private void ShootMemory()
    {
        if (InputManager.instance.KeyDown(Keybindings.KeyList.Shoot) && shouldCheckForShootMemory && bulletUnlocked)
            shootMemoryTimer = 0.2f;

        if (shootMemoryTimer > 0)
            shootMemoryTimer -= Time.deltaTime;

        if (shouldICheckIsGrounded)
            GroundCheckForShoot();

        if (canShootTimer > 0)
        {
            canShootTimer -= Time.deltaTime;
            if (!isGrounded)
                canShoot = false;
        }

        if (canShootTimer <= 0 && isGrounded)
            shouldICheckIsGrounded = true;

    }           //shoot functions
    private void GroundCheckForShoot()
    {
        if (isGrounded)
            canShoot = true;
    }
    public void ShootStart()
    {
        if (shootMemoryTimer > 0 && !canTeleport && canShootTimer <= 0 && canShoot)
        {
            shootMemoryTimer = 0;
            shouldCheckForShootMemory = false;
            shouldICheckIsGrounded = false;
            canTeleport = true;
            isKnockingBack = false;
            CurrentBulletGameObject = Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation) as GameObject;
            StartCoroutine(PauseMovement(waitTimeShooting));
            audioManager.PlaySound(AudioManager.SoundList.BulletFired);
        }
    }

    private void Teleport()
    {
        canTeleport = false;
        canShootTimer = 0.2f;
        shouldICheckIsGrounded = false;
        shouldCheckForShootMemory = true;
        if (!isGrounded)
            canShoot = false;
        CreateDust();

        if (!didHitAnEnemy)
        {
            StartCoroutine(FreezeGame());
            rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y + 0.5f);
            //rb.transform.rotation = new Quaternion(0, 0, 0, 0);
            rb.velocity = new Vector2(0, 0);
        }

        KillBulletObject();

        if (didHitAnEnemy)
        {
            StartCoroutine(FreezeGame());
            StartCoroutine(MakePlayerInvincible(0.15f));
            if (!bulletHitDoor)
            {
                TeleportToEnemy();
                CurrentBulletGameObject.GetComponent<Enemy>().PlayerTeleportedToEnemy();
            }
            else
            {
                StartCoroutine(DontRespawnAtCheckpointForTime(0.1f));
                TeleportToDoor();
                CurrentBulletGameObject.gameObject.GetComponent<DoorDashThrough>().Highlight();
            }
            canShoot = true;
            didHitAnEnemy = false;
            canShootTimer = 0;
        }
        ResetAxis();
        audioManager.PlaySound(AudioManager.SoundList.Teleport);
        StartCoroutine(currentRoom.GetComponent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(teleportShakeTime, teleportShakeForce));
    }

    public void UnHightLightEnemies()
    {
        GameObject[] arrayOfEnemies = GameObject.FindGameObjectsWithTag("EnemyAlive");
        foreach (GameObject enemy in arrayOfEnemies)
        {
            if (enemy.GetComponent<Enemy>().outlineOn && !GameObject.ReferenceEquals(enemy, CurrentBulletGameObject))
            {
                enemy.GetComponent<Enemy>().Highlight();
                enemy.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }
        }
    }

    private void TeleportToEnemy()
    {
   
        switch (directionPressedNow)
        {
            case DirectionPressed.Nothing:
                TeleportToEnemyNoInput();
                break;
            case DirectionPressed.Right:
                if (rb.transform.position.x > CurrentBulletGameObject.transform.position.x) // checks if knocking back or not
                {
                    isKnockingBack = true;
                    Flip();
                    CurrentBulletGameObject.GetComponent<Enemy>().KnockBackEnemyHit(CurrentBulletGameObject.GetComponent<Enemy>().knockBackWhenHit * 25, 1, 0.2f);
                }
                else
                    StartCoroutine(StartSwitchStateToIgnoreInputs(0.1f, false)); // if not knocking back, ignore player's input

                if (!enemyNearWallRight)  // checks the currentEnemy isn't close to a wall so the player won't clip through the wall
                {
                    rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x + 2, CurrentBulletGameObject.transform.position.y + 0.5f);
                    StartCoroutine(ChangeBoolAfterSeconds(thrustTimer));
                    rb.AddForce(Vector2.right * thrustPower, ForceMode2D.Impulse);
                }
                else if (enemyNearWallRight && !isKnockingBack)
                {
                    rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y + 0.5f);
                    StartCoroutine(ChangeBoolAfterSeconds(thrustTimer / 3f));
                    rb.AddForce(Vector2.right * thrustPower, ForceMode2D.Impulse);
                }

                break;
            case DirectionPressed.Left:
                if (rb.transform.position.x < CurrentBulletGameObject.transform.position.x)
                {
                    isKnockingBack = true;
                    Flip();
                    CurrentBulletGameObject.GetComponent<Enemy>().KnockBackEnemyHit(CurrentBulletGameObject.GetComponent<Enemy>().knockBackWhenHit * 25, 1, 0.2f);
                }
                else
                    StartCoroutine(StartSwitchStateToIgnoreInputs(0.1f, false));

                if (!enemyNearWallLeft)
                {
                    rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x - 2, CurrentBulletGameObject.transform.position.y + 0.5f);
                    StartCoroutine(ChangeBoolAfterSeconds(thrustTimer));
                    rb.AddForce(new Vector2(-1, 0) * thrustPower, ForceMode2D.Impulse);
                }
                else if (enemyNearWallLeft && !isKnockingBack)
                {
                    rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y + 0.5f);
                    StartCoroutine(ChangeBoolAfterSeconds(thrustTimer / 3f));
                    rb.AddForce(Vector2.left * thrustPower, ForceMode2D.Impulse);
                }
                break;
            case DirectionPressed.Up:
                StartCoroutine(ChangeBoolAfterSeconds(0.15f));
                rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y + 1);
                rb.AddForce(new Vector2(0, 1) * thrustPower * 0.9f, ForceMode2D.Impulse);
                break;
            case DirectionPressed.Down:
                //rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y - 1);
                CurrentBulletGameObject.GetComponent<Enemy>().isBeingPulledDown = true;
                break;
            case DirectionPressed.UpRight:
                TeleportToEnemyNoInput();
                /*
                rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x + 0.8f, CurrentBulletGameObject.transform.position.y + 1.5f);
                //StartCoroutine(ChangeBoolAfterSeconds(topSidesSkillTime));
                isUsingTopNSideTeleport = true;
                topSidesSkillSpeed = constantTopSidesSkill;
                timerForSkill = timerForSkillMax;
                rb.velocity = new Vector2(15, 30);*/
                break;
            case DirectionPressed.UpLeft:
                TeleportToEnemyNoInput();
                //rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x - 0.5f, CurrentBulletGameObject.transform.position.y + 1);
                break;
            case DirectionPressed.DownRight:
                TeleportToEnemyNoInput();
                break;
            case DirectionPressed.DownLeft:
                TeleportToEnemyNoInput();
                break;
        }
    }

    void TeleportToEnemyNoInput()
    {
        StartCoroutine(StartSwitchStateToIgnoreInputs(0.1f, false));
        if (facingRight)
        {
            if (!enemyNearWallRight)
            {
                rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x + 2, CurrentBulletGameObject.transform.position.y + 0.5f);
                StartCoroutine(ChangeBoolAfterSeconds(thrustTimer));
                rb.AddForce(Vector2.right * thrustPower, ForceMode2D.Impulse);
            }
            else
            {
                rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y + 0.5f);
                StartCoroutine(ChangeBoolAfterSeconds(thrustTimer / 3f));
                rb.AddForce(Vector2.right * thrustPower, ForceMode2D.Impulse);
            }
        }
        else
        {
            if (!enemyNearWallLeft)
            {
                rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x - 2, CurrentBulletGameObject.transform.position.y + 0.5f);
                StartCoroutine(ChangeBoolAfterSeconds(thrustTimer));
                rb.AddForce(new Vector2(-1, 0) * thrustPower, ForceMode2D.Impulse);
            }
            else
            {
                rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y + 0.5f);
                StartCoroutine(ChangeBoolAfterSeconds(thrustTimer / 3f));
                rb.AddForce(Vector2.left * thrustPower, ForceMode2D.Impulse);
            }
        }
    }

    private void TeleportToDoor()
    {
        bulletHitDoor = false;
        if (CurrentBulletGameObject.GetComponent<DoorDashThrough>().IsDoorLayedVertical == true)
        {
            if (moveInput > 0)
            {
                rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x + 2, CurrentBulletGameObject.transform.position.y + 0.5f);
                StartCoroutine(ChangeBoolAfterSeconds(thrustTimer));
                rb.AddForce(Vector2.right * thrustPower, ForceMode2D.Impulse);
            }
            else if (moveInput < 0)
            {
                rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x - 2, CurrentBulletGameObject.transform.position.y + 0.5f);
                StartCoroutine(ChangeBoolAfterSeconds(thrustTimer));
                rb.AddForce(new Vector2(-1, 0) * thrustPower, ForceMode2D.Impulse);
            }
            else if (moveInput == 0)
            {
                if (facingRight)
                {
                    rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x + 2, CurrentBulletGameObject.transform.position.y + 0.5f);
                    StartCoroutine(ChangeBoolAfterSeconds(thrustTimer));
                    rb.AddForce(Vector2.right * thrustPower, ForceMode2D.Impulse);
                }
                else
                {
                    rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x - 2, CurrentBulletGameObject.transform.position.y + 0.5f);
                    StartCoroutine(ChangeBoolAfterSeconds(thrustTimer));
                    rb.AddForce(new Vector2(-1, 0) * thrustPower, ForceMode2D.Impulse);
                }
            }
        }
        else
        {
            StartCoroutine(ChangeBoolAfterSeconds(0.15f));
            if (moveInputVertical > 0)
            {
                rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y + 1);
                rb.AddForce(new Vector2(0, 1) * thrustPower * 0.7f, ForceMode2D.Impulse);
            }
            else if (moveInputVertical < 0)
            {
                rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y - 1);
            }
            else if (moveInputVertical == 0)
            {
                if (CurrentBulletGameObject.transform.position.y > rb.transform.position.y)
                {
                    rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y + 1);
                    rb.AddForce(new Vector2(0, 1) * thrustPower * 0.7f, ForceMode2D.Impulse);
                }
                else
                {
                    rb.transform.position = new Vector2(CurrentBulletGameObject.transform.position.x, CurrentBulletGameObject.transform.position.y - 1);
                }
            }
        }

    }
    private void DashToEnemy()
    {



    }

    void BulletReset()
    {
        if(!didHitAnEnemy)
            KillBulletObject();
        else
        {
            canTeleport = false;
            canShoot = true;
            didHitAnEnemy = false;
            canShootTimer = 0;
            shouldCheckForShootMemory = true;

            if(!bulletHitDoor)
                CurrentBulletGameObject.GetComponent<Enemy>().Highlight();
            else
            {
                bulletHitDoor = false;
                CurrentBulletGameObject.GetComponent<DoorDashThrough>().Highlight();
            }

            CurrentBulletGameObject = null;
        }
    }

    public void KillBulletObject()
    {
        if (!didHitAnEnemy)
        {
            if (CurrentBulletGameObject != null)
                PrefabManager.instance.PlayVFX(PrefabManager.ListOfVFX.BulletDissapear, CurrentBulletGameObject.transform.position);
            canTeleport = false;
            canShoot = true;
            if(FindObjectsOfType<bullet>().Length > 0)
                Destroy(CurrentBulletGameObject);

            shouldICheckIsGrounded = false;
            shouldCheckForShootMemory = true;
            if (isGrounded == false)
                canShoot = false;

        }
    }




    private void AttackMemory()
    {
        if (InputManager.instance.KeyDown(Keybindings.KeyList.Attack))
            regularAttackTimer = 0.1f;
    }       // attacking functions
    public void AttackRegular()
    {
        atkAnimationStallTimer = 19;
        if (moveInputVertical != 1)
        {
            if (atkAnimationCombo <= 0)
            {
                atkAnimationCombo = 1f;
                animator.SetTrigger("AttackingRegular");
            }
            else
            {
                animator.SetTrigger("AttackingRegularCombo");
                atkAnimationCombo = 0;
            }

            //Detect all the enemies hit and puts the data in a collider array, allowing to affect each enemy hit.
            Collider2D[] hitEnemies = Physics2D.OverlapCapsuleAll(regularAttackPoint.position, new Vector2(5, 3.2f), CapsuleDirection2D.Horizontal, 0, canBeAttackedLayerMask);

            if (hitEnemies.Length == 0)
            {
                RaycastHit2D checkIfHitWall;
                if (facingRight)
                    checkIfHitWall = Physics2D.Raycast(shootingPoint.position, Vector2.right, 5, whatIsGround);
                else
                    checkIfHitWall = Physics2D.Raycast(shootingPoint.position, Vector2.left, 5, whatIsGround);
                if (checkIfHitWall)
                {
                    audioManager.PlaySound(AudioManager.SoundList.HitWall);
                    GameObject hitVFXspawn = Instantiate(hitVFXWall, checkIfHitWall.point, transform.rotation);
                }
            }
            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.gameObject.layer == 12) // enemy
                {
                    enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
                    GetComponent<ManaBar>().FillUpMana(manaFillPerAttack);
                    float y = facingRight ? 0f : 180f;
                    GameObject hitVFXspawn = Instantiate(hitVFX, enemy.transform.position, Quaternion.Euler(0, y, Random.Range(-20, 20)));
                }
                if (enemy.gameObject.layer == 17) //crank
                    enemy.GetComponent<DoorCrank>().Triggered();
                if (enemy.gameObject.layer == 30) //hidden door
                {
                    audioManager.PlaySound(AudioManager.SoundList.HiddenDoorOpen);
                    GameObject hitVFXspawn = Instantiate(hitVFXWall, enemy.transform.position, transform.rotation);
                    Destroy(enemy.gameObject);
                }
                if (enemy.gameObject.layer == 19)
                    enemy.GetComponentInParent<Hollow>().BlinkVFX();
            }
        }
        else if (moveInputVertical > 0)
        {
            animator.SetTrigger("AttackingRegularUp");
            //Detect all the enemies hit and puts the data in a collider array, allowing to affect each enemy hit.
            Collider2D[] hitEnemies = Physics2D.OverlapCapsuleAll(upAttackPoint.position, new Vector2(3.2f, 5f), CapsuleDirection2D.Vertical, 0, canBeAttackedLayerMask);

            if (hitEnemies.Length == 0)
            {
                RaycastHit2D checkIfHitWall = Physics2D.Raycast(shootingPoint.position, Vector2.up, 5, whatIsGround);
                if (checkIfHitWall)
                {
                    audioManager.PlaySound(AudioManager.SoundList.HitWall);
                    GameObject hitVFXspawn = Instantiate(hitVFXWall, checkIfHitWall.point, Quaternion.Euler(0, 0, 90));
                }
            }

            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.gameObject.layer == 12) // enemy
                {
                    enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
                    GameObject hitVFXspawn = Instantiate(hitVFX, enemy.transform.position, Quaternion.Euler(0, 0, 90));
                }
                if (enemy.gameObject.layer == 17) //crank
                    enemy.GetComponent<DoorCrank>().Triggered();
                if (enemy.gameObject.layer == 30) //hidden door
                {
                    audioManager.PlaySound(AudioManager.SoundList.HiddenDoorOpen);
                    GameObject hitVFXspawn = Instantiate(hitVFXWall, enemy.transform.position, Quaternion.Euler(0, 0, 90));
                    Destroy(enemy.gameObject);
                }
                if (enemy.gameObject.layer == 19)
                    enemy.GetComponentInParent<Hollow>().BlinkVFX();
            }
        }
        //else if (moveInputVertical < 0)  // dont need attack for down 
        //  {
        //    if (rb.velocity.y > 0)
        //       rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * -10);
        //    else if (rb.velocity.y < 0)
        //       rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 10);
        //    else if (rb.velocity.y == 0)
        //        rb.velocity = new Vector2(rb.velocity.x, -10);
        //    animator.SetTrigger("AttackingRegularDown");
        // }
        audioManager.PlaySound(AudioManager.SoundList.PlayerAttack);
    }       // .




    public void HealNow()
    {
        manaBar.UseMana(25);
        GetComponent<Health>().health++;
        AudioManager.instance.PlaySound(AudioManager.SoundList.Heal);
        PrefabManager.instance.PlayVFX(PrefabManager.ListOfVFX.HealParticle, transform.position);
    }   //healing

    public void FullRestore()
    {
        GetComponent<Health>().FullHeal();
        GetComponent<ManaBar>().SetManaFull();
    }





    // coroutines are confusing :)

    private IEnumerator MakePlayerInvincible(float time)
    {
        playerIsInvulnerable = true;
        yield return new WaitForSeconds(time);
        playerIsInvulnerable = false;
    }       // make the player invincible for float time 

    private IEnumerator PlayerBlinkingAnimation()
    {

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        yield return new WaitForSeconds(0.3f);

        sr.enabled = false;

        yield return new WaitForSeconds(0.15f);
        sr.enabled = true;
        yield return new WaitForSeconds(0.3f);
        sr.enabled = false;
        yield return new WaitForSeconds(0.15f);
        sr.enabled = true;
        yield return new WaitForSeconds(0.3f);
        sr.enabled = false;
        yield return new WaitForSeconds(0.15f);
        sr.enabled = true;

    }

    private IEnumerator DontRespawnAtCheckpointForTime(float time)
    {
        dontRespawn = true;
        yield return new WaitForSeconds(time);
        dontRespawn = false;
    }

    public IEnumerator PauseMovement(float timeToWait)
    {
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        yield return new WaitForSeconds(timeToWait);
        rb.constraints = ~RigidbodyConstraints2D.FreezePosition;
    }       // pause only the player's movement for float timeToWait


    private IEnumerator ChangeBoolAfterSeconds(float timeToWait)
    {
        rb.velocity = new Vector2(0, 0);
        thrustHappening = true;
        yield return new WaitForSeconds(timeToWait);
        thrustHappening = false;
    }

    private IEnumerator FreezeGame()
    {
        var original = Time.timeScale;
        Time.timeScale = 0f;

        yield return null;

        Time.timeScale = original;
    }           // Freeze the game, for a frame currently.

    private IEnumerator FreezeGameForTime(float time)
    {
        var original = Time.timeScale;
        Time.timeScale = 0.1f;

        yield return new WaitForSecondsRealtime(time);

        Time.timeScale = original;
    }       // Freeze the game for float time

    private IEnumerator StartSwitchStateToIgnoreInputs(float time, bool isPlayerHurt)
    {
        state = State.IgnorePlayerInput;
        if (isPlayerHurt)
            animator.SetBool("IsHurt", true);
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(time);
        rb.velocity = Vector2.zero;
        if (isPlayerHurt)
            animator.SetBool("IsHurt", false);
        state = State.Normal;
    }       //Ignore player's input for float time, if isPlayerHurt, play hurt animation.

    private IEnumerator SideSkillsState(float time)
    {
        yield return new WaitForSeconds(time);
    }

    private IEnumerator StartHeal()
    {
        canHeal = false;
        animator.SetTrigger("Heal");
        state = State.IgnorePlayerInput;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.05f);

        // manaBar.UseMana(25);
        ResetAxis();
        state = State.Normal;

        yield return new WaitForSeconds(0.25f);

        canHeal = true;
    }

    public IEnumerator SwitchStateIgnore(float time)
    {
        state = State.IgnorePlayerInput;

        yield return new WaitForSeconds(time);

        ResetAxis();
        state = State.Normal;
    }

    void ResetAxis()
    {
        moveInputVertical = 0;
        moveInput = 0;
    }



    public IEnumerator RespawnAtCheckpoint(float timeToWait)
    {
        audioManager.PlaySound(AudioManager.SoundList.RespawnSound);
        StartCoroutine(TransitionPrep(0.6f));
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        
        //Add here death animation.
        yield return new WaitForSeconds(timeToWait);

        ResetAxis();
        rb.constraints = ~RigidbodyConstraints2D.FreezePosition;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.position = gm.lastCheckPointPosition;
        GetComponent<Health>().DealDamage(1);
        BulletReset();
        //currentRoom.GetComponent<RoomManagerOne>().PlayerRespawned.Invoke();
        currentRoom.GetComponent<RoomManagerOne>().PlayerRespawnReset2();
    }   ///respawn and transitions

    public void RespawnAtLatestCheckpoint()
    {
        if (!dontRespawn)
        {
            StartCoroutine(StartSwitchStateToIgnoreInputs(0.1f, false));
            StartCoroutine(RespawnAtCheckpoint(0.4f));
        }
    }

    public void RespawnAtSavePoint()
    {
        transform.position = gm.savePointPosition;
        GetComponent<ManaBar>().SetManaFull();
        GetComponent<Health>().FullHeal();
        BulletReset();
        currentRoom.GetComponent<RoomManagerOne>().PlayerRespawnReset2();
    }

    public IEnumerator TransitionStart()
    {
        StartCoroutine(TransitionPrep(1f));
        currentRoom.GetComponent<RoomManagerOne>().PlayerRespawnReset2();

        yield return new WaitForSeconds(0.05f);

        RespawnAtSavePoint();

        yield return new WaitForSeconds(0.1f);

        currentRoom.GetComponent<RoomManagerOne>().PlayerRespawnReset2();
        audioManager.PlaySound(AudioManager.SoundList.Load);
    }

    private IEnumerator TransitionPrep(float blackScreenTime)
    {
        TransitionManager tm = PrefabManager.instance.transitionManager.GetComponent<TransitionManager>();
        tm.EnableObject();
        tm.animator.SetTrigger("Start");
        StartCoroutine(SwitchStateIgnore(0.6f));    

        yield return new WaitForSeconds(blackScreenTime);

        tm.animator.SetTrigger("End");
    }




    private void CheckDirectionPressed()            //Check which direction is being pressed right now.
    {
        if (directionPressed.Equals(new Vector2(0, 0)))
            directionPressedNow = DirectionPressed.Nothing;
        if (directionPressed.Equals(new Vector2(1, 0)))
            directionPressedNow = DirectionPressed.Right;
        if (directionPressed.Equals(new Vector2(0, 1)))
            directionPressedNow = DirectionPressed.Up;
        if (directionPressed.Equals(new Vector2(-1, 0)))
            directionPressedNow = DirectionPressed.Left;
        if (directionPressed.Equals(new Vector2(0, -1)))
            directionPressedNow = DirectionPressed.Down;
        if (directionPressed.Equals(new Vector2(1, 1)))
            directionPressedNow = DirectionPressed.UpRight;
        if (directionPressed.Equals(new Vector2(-1, 1)))
            directionPressedNow = DirectionPressed.UpLeft;
        if (directionPressed.Equals(new Vector2(1, -1)))
            directionPressedNow = DirectionPressed.DownRight;
        if (directionPressed.Equals(new Vector2(-1, -1)))
            directionPressedNow = DirectionPressed.DownLeft;

    }

    public bool IsFacingRight()
    {
        return facingRight;
    }
    public DirectionPressed GetDirecdtionPressed()
    {
        return directionPressedNow;
    }






    // functions for animations starting here
    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    void CreateDust()
    {
        dust.Play();
    }

    public void ShakeCamera(float time, float force)
    {
        StartCoroutine(currentRoom.GetComponent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(time, force));
    }

    public void ChangePointerBool()
    {
        showPointer = !showPointer;
        bulletPointer.SetActive(showPointer);
    }

    private void RotatePointer()
    {
        bool pointerActive = false;
        if(CurrentBulletGameObject != null)
        {
            if (!pointerActive)
            {
                bulletPointer.SetActive(true);
                pointerActive = true;
            }
            var pos = transform.position;
            var dir = CurrentBulletGameObject.transform.position - pos;
            var angle = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
            bulletPointer.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
        {
            bulletPointer.SetActive(false);
            pointerActive = false;
        }
    }

    /*
    private void SavePoiner()
    {
        if (showPointer)
            PlayerPrefs.SetInt("BulletPointer", 1);
        else
            PlayerPrefs.SetInt("BulletPointer", -1);
        PlayerPrefs.Save();
    }
    private bool BoolToInt(int i)
    {
        return i == 1 ? true : false;
    } */

}
