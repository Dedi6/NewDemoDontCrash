using System.Collections;
using UnityEngine;

public class RageBoss : MonoBehaviour, ISFXResetable, IKnockbackable, IPhaseable<float>
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPointGround;
    private bool facingRight = false;
    private RaycastHit2D wallCheckRaycast;
    private RaycastHit2D groundCheckRaycast;
    private Vector2 raycastDirection;
    public float wallCheckDistance = 1, attackCheckDistance, knockBackTime, turnAroundTimer;
    public float groundCheckDistance = 1, stallAnimation, phaseTwoHp, phaseThreeHp;
    private int layerMaskGround = 1 << 8, currentPhase = 1;
    private Collider2D waitingCast;

    [Header("Attacking")]
    [Space]
    public float pauseBeforeAttack;
    public float slashCooldown, slashMoveForce, slashTimer, slashInvisTime, slashAppearDist, meteorSpeed, fieldSlowExpand, fielFastExpand, smallFieldRadius;
    public float stompShakeTime, stompShakeForce, trackSpeed, meteorShootSpeed, projSpeed, spreadRadius, punchKnockbackForce;
    public float meteorFallAngle, metoerFallDelay, currentAngle, interlaceAngle1, interlaceAngle2, interlaceDelay, numberOfWaves, currentInterlaceNum;
    public int numOfProjectiles, metoerFallNum, interlaceNumber;
    private int amountOfSlashes = 1, currentSkillsInt, currentMeteorCount;
    public Transform summonPoint, punchPoint, meteorRainPoint, meteorRainPoint2, meteorWavePoint;
    private Vector2 currentSpawnPoint;

    [Header("Cooldowns")]
    [Space]
    public float meteorsCooldown;
    public float summonMeteorsCD, summonFrogCD, shootMeteorCD, punchCD, slashCD, circleCD, meteorRainCD, interlaceCD;


    private System.Action currentSkill;
    public Animator animator;
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
        Dead,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        raycastDirection = new Vector2(-1, 0);
        boxCollider = GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        GetComponent<Enemy>().usePhases = true;
        currentSkillsInt = 45;
        bool goRight = GetComponent<Enemy>().goRight;
        if ((goRight && !facingRight) || (!goRight && facingRight))
            Flip();
    }

    private void Awake()
    {
        state = State.Waiting;
    }

    void Update()
    {

        if(state == State.Normal)
        {
            if (Mathf.Abs(transform.position.x - player.transform.position.x) < 8 && turnAroundTimer <= 0 && (Mathf.Abs(transform.position.y - player.transform.position.y) < 2))
            {
                ForceFlip();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animator.SetTrigger("Slash");
            amountOfSlashes = SetSlashInt();
            animator.SetBool("FirstSlash", SetSlashBool());
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
            Flip();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            ShootInACircle();
        if (Input.GetKeyDown(KeyCode.Alpha4))
            StartCoroutine(MetoerInterlace());
        if (Input.GetKeyDown(KeyCode.Alpha5))
            StartCoroutine(MeteorFall());
        if (Input.GetKeyDown(KeyCode.Alpha6))
            animator.SetTrigger("Zone"); ;

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
        if (waitingCast != null && waitingCast.transform.gameObject.layer == 11) // 11 is player
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

    private void Flip()
    {
        turnAroundTimer = 0.3f;
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
        raycastDirection = new Vector2(-raycastDirection.x, 0);
    }

    public void ResetSFXCues()
    {
        state = State.Waiting;
        currentSkillsInt = 60;
        if(currentPhase == 3)
            GetComponentInChildren<LightZone>().Shrink();
        currentPhase = 1;
    }

    public void PlayHitGroundSFX()
    {
        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(stompShakeTime, stompShakeForce));
        AudioManager.instance.PlaySound(AudioManager.SoundList.EnemyHitGround);
    }

    private IEnumerator CooldownCoroutine(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);

        if (state != State.Dead)
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
        int min = IsCloseToWall() ? 41 : 1; // check if close to wall. if so, don't use Enrage
        int i = Random.Range(min, max); // range is X to Y -1 for ints. [1-3]
        
        if(1 <= i && i <= 40)
        {
            animator.SetTrigger("Enrage");
            AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossPrep);
            SetEnrageSkill();
        }
        else if (41 <= i && i <= 65)
        {
            animator.SetTrigger("Slash");
            amountOfSlashes = SetSlashInt();
            animator.SetBool("FirstSlash", SetSlashBool());
        }
        else if (66 <= i && i <= 100)
        {
            animator.SetTrigger("PunchStart");
            AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossPrepPunch);
        }
    }

    private bool IsCloseToWall()
    {
        int sign = facingRight ? 1 : -1;
        RaycastHit2D ray = Physics2D.Raycast(player.transform.position, new Vector2(sign, 0), 12, 1 << 8);  // 12 is distance, 1 << 8 is ground's layermask
        return ray ? true : false;
    }

    private bool SetSlashBool()
    {
        int r = Random.Range(1, 3);
        return r == 1 ? true : false;
    }

    private int SetSlashInt()
    {
        if (currentPhase > 2)
            return Random.Range(2, 4);
        else
            return Random.Range(1, 3);
    }

    private void SetEnrageSkill()
    {

        if (currentPhase != 3)             // not final phase
        {
            int i = Random.Range(1, currentSkillsInt + 1); // range is X to Y -1 for ints. 
            if (1 <= i && i <= 15)
            {
                currentSkill = SummonRedFrogs;
                currentSpawnPoint = summonPoint.transform.position;
            }
            else if (16 <= i && i <= 30)
            {
                currentSkill = ShootInACircle;
                currentSpawnPoint = new Vector2(player.transform.position.x, player.transform.position.y + 5f);
            }
            else if (31 <= i && i <= 45)
            {
                currentSkill = ShootMeteor;
                currentSpawnPoint = summonPoint.transform.position;
            }
        }
        else                               // in final phase
        {
            int i = Random.Range(1, 3); // range is X to Y -1 for ints.  so 1-2 here.
            if (i == 1)
            {
                //Meteor side to side
                currentSkill = StartMeteorFall;
            }
            else
            {
                //Meteor interlace
                currentSkill = StartMeteorInterlace;
            }
        }
    }

    private void StartMeteorFall()
    {
        StartCoroutine(MeteorFall());
    }
    private void StartMeteorInterlace()
    {
        StartCoroutine(MetoerInterlace());
    }

    public IEnumerator StartSkill()
    {
        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(stompShakeTime, stompShakeForce));
        GameObject pref = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.RageBossSummon);
        GameObject summonEffect = Instantiate(pref, currentSpawnPoint, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        currentSkill();
    }

    //summon Red Frogs
    private void SummonRedFrogs()
    {
        GameObject pref = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.RedFrog);
        Vector2 pos = new Vector2(summonPoint.position.x, summonPoint.position.y);
        GameObject redFrog = Instantiate(pref, pos, Quaternion.identity, GameMaster.instance.currentRoom.transform);
        StartCoroutine(CooldownCoroutine(summonFrogCD));
    }

    private void ShootMeteor()
    {
        GameObject pref = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.Meteor);
        Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y + 0.3f);
        Vector2 atkPoint = new Vector2(summonPoint.transform.position.x, summonPoint.transform.position.y);
        Vector2 direction = (playerPos - atkPoint).normalized;
        GameObject meteor = Instantiate(pref, atkPoint, Quaternion.identity);
        meteor.GetComponent<Meteor>().SetDirection(direction, meteorShootSpeed);
        StartCoroutine(CooldownCoroutine(shootMeteorCD));
    }

    public void ShootInACircle()
    {
        float angleStep = 360f / numOfProjectiles;
        float angle = 0;
        for (int i = 0; i < numOfProjectiles; i++)
        {
            Vector2 atkPoint = currentSpawnPoint;
            float dirX = atkPoint.x + Mathf.Sin((angle * Mathf.PI) / 180) * spreadRadius;
            float diry = atkPoint.y + Mathf.Cos((angle * Mathf.PI) / 180) * spreadRadius;
            Vector2 projectileVector = new Vector2(dirX, diry);
            Vector2 direction = (projectileVector - atkPoint).normalized; // * speed was removed
            GameObject pref = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.Meteor);
            GameObject proj = Instantiate(pref, atkPoint, Quaternion.identity, transform.parent);
            proj.GetComponent<Meteor>().SetDirection(direction, projSpeed);

            angle += angleStep;
        }
        StartCoroutine(CooldownCoroutine(circleCD));
    }

    public IEnumerator MeteorFall()
    {
        float angleStep = meteorFallAngle / metoerFallNum;
        if (!facingRight)
            angleStep = -angleStep;

        if (currentMeteorCount == 0)
            currentAngle = facingRight ? 110 : 250;


        Vector2 atkPoint = meteorWavePoint.transform.position;
        float dirX = atkPoint.x + Mathf.Sin((currentAngle * Mathf.PI) / 180) * spreadRadius;
        float diry = atkPoint.y + Mathf.Cos((currentAngle * Mathf.PI) / 180) * spreadRadius;
        Vector2 projectileVector = new Vector2(dirX, diry);
        Vector2 direction = (projectileVector - atkPoint).normalized; // * speed was removed
        GameObject pref = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.Meteor);
        GameObject proj = Instantiate(pref, atkPoint, Quaternion.identity, transform.parent);
        proj.GetComponent<Meteor>().SetDirection(direction, meteorSpeed);

        currentAngle += angleStep;

        currentMeteorCount++;

        yield return new WaitForSeconds(metoerFallDelay);

        if (currentMeteorCount < metoerFallNum)
            StartCoroutine(MeteorFall());
        else
        {
            currentMeteorCount = 0;
            StartCoroutine(CooldownCoroutine(meteorRainCD));
        }
    }
    
    public IEnumerator MetoerInterlace()
    {
        float angleStep = meteorFallAngle / interlaceNumber;
        Vector2 atkPoint;

        if (IsRightSide())
        {
            atkPoint = meteorRainPoint.transform.position;
            currentAngle = currentAngle == interlaceAngle1 + angleStep * interlaceNumber ? interlaceAngle2 : interlaceAngle1;
        }
        else
        {
            atkPoint = meteorRainPoint2.transform.position;
            currentAngle = currentAngle == interlaceAngle1 + angleStep * interlaceNumber - 90 ? interlaceAngle2 - 90 : interlaceAngle1 - 90;
        }

        for (int i = 0; i < interlaceNumber; i++)
        {
            //Vector2 atkPoint = GetRainPos();
            float dirX = atkPoint.x + Mathf.Sin((currentAngle * Mathf.PI) / 180) * spreadRadius;
            float diry = atkPoint.y + Mathf.Cos((currentAngle * Mathf.PI) / 180) * spreadRadius;
            Vector2 projectileVector = new Vector2(dirX, diry);
            Vector2 direction = (projectileVector - atkPoint).normalized; // * speed was removed
            GameObject pref = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.Meteor);
            GameObject proj = Instantiate(pref, atkPoint, Quaternion.identity, transform.parent);
            proj.GetComponent<Meteor>().SetDirection(direction, meteorSpeed);

            currentAngle += angleStep;
        }

        currentInterlaceNum++;

        yield return new WaitForSeconds(interlaceDelay);

        if (currentInterlaceNum < numberOfWaves)
        {
            StartCoroutine(MetoerInterlace());

        }
        else
        {
            currentInterlaceNum = 0;
            StartCoroutine(CooldownCoroutine(interlaceCD));
        }
    }


    private bool IsRightSide()
    {
        float aDistance = Vector2.Distance(player.transform.position, meteorRainPoint.transform.position);
        float bDistance = Vector2.Distance(player.transform.position, meteorRainPoint2.transform.position);
        if (aDistance < bDistance)
            return true;
        else
            return false;
    }


    public void Slash()
    {
        int sign = facingRight ? -1 : 1;
        RaycastHit2D rayToWall = Physics2D.Raycast(player.transform.position, new Vector2(sign, 0), slashAppearDist, 1 << 8); // 1 << 8 is gorund
        if (rayToWall)
        {
            sign = -sign;
            Flip();
        }
        transform.position = new Vector2(player.transform.position.x + sign * slashAppearDist, player.transform.position.y);
        ToggleColliders();
    }

    public IEnumerator PunchStart()
    {
        int sign = facingRight ? 1 : -1;
        RaycastHit2D rayToWall = Physics2D.Raycast(player.transform.position, new Vector2(sign, 0), slashAppearDist, 1 << 8); // 1 << 8 is gorund
        if(!rayToWall)
            transform.position = new Vector2(player.transform.position.x + sign * slashAppearDist, player.transform.position.y);
        else
        {
            Flip();
            transform.position = new Vector2(player.transform.position.x + -sign * slashAppearDist, player.transform.position.y);
        }
        ToggleColliders();

        yield return new WaitForSeconds(0.1f);

    }

    public void Punch()
    {
        Collider2D hitEnemies = Physics2D.OverlapCircle(punchPoint.position, 5f, 1 << 11); // 1 << 11 is player
        bool b = facingRight ? false : true;
        if (hitEnemies != null)
        {
            hitEnemies.GetComponent<MovementPlatformer>().KnockBackPlayer(punchKnockbackForce, 1, 0, b);
            hitEnemies.GetComponent<MovementPlatformer>().GotHitByAnEnemy(1);
            StartCoroutine(PunchHitSFX());
        }
        StartCoroutine(CooldownCoroutine(punchCD));
    }

    private IEnumerator PunchHitSFX()
    {
        AudioManager audio = AudioManager.instance;
        audio.PlaySound(AudioManager.SoundList.RageBossPunch);
        yield return new WaitForSeconds(0.25f);
        audio.PlaySound(AudioManager.SoundList.PlayerTossedIntoWall);
    }

    public void Dissapear()
    {
        ToggleColliders();
        AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossTeleport);
        StartCoroutine(StallAnimationForTime(slashInvisTime));
    }

    public void ToggleColliders()
    {
        GetComponent<Enemy>().ToggleTriggerCollider();
        gameObject.layer = gameObject.layer == 12 ? 13 : 12;
    }

    public IEnumerator SlashStart()
    {
        int sign = facingRight ? 1 : -1;
        enemy.velocity = new Vector2(slashMoveForce * sign, 0);

        yield return new WaitForSeconds(slashTimer);

        enemy.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.1f);

        if(amountOfSlashes > 1)
        {
            amountOfSlashes--;
            animator.SetTrigger("Slash");
            animator.SetBool("FirstSlash", SetSlashBool());
        }
        else
            StartCoroutine(CooldownCoroutine(slashCD));
    }

    public IEnumerator ExpandZone()
    {
        GetComponentInChildren<LightZone>().SetExpand(smallFieldRadius, fieldSlowExpand);

        yield return new WaitForSeconds(1.2f);

        StartCoroutine(GetComponentInParent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(stompShakeTime * 3, stompShakeForce));
        GetComponentInChildren<LightZone>().SetExpand(60, fielFastExpand);
    }

    private void DodgeStart()
    {
        if (Vector2.Distance(transform.position, player.transform.position) > 10)
            return;
        int i = Random.Range(1, 11); // 1 - 10.
        if(i < 5)
            animator.SetTrigger("Dodge");
    }

    public void Dodge()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.RageBossTeleport);
        transform.position = new Vector2(GetDodgePointX(), transform.position.y);
        ForceFlip();
    }

    private float GetDodgePointX()
    {
        Vector2 pos = transform.position;
        RaycastHit2D rayRight = Physics2D.Raycast(pos, Vector2.right, 100, 1 << 8); // 8 is ground.
        RaycastHit2D rayLeft = Physics2D.Raycast(pos, Vector2.left, 100, 1 << 8); // 8 is ground.
        if (Vector2.Distance(pos, rayRight.point) > Vector2.Distance(pos, rayLeft.point))
            return rayRight.point.x - 7;
        else
            return rayLeft.point.x + 7;
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

    private IEnumerator StallAnimationCoroutine()
    {
        animator.speed = 0;

        yield return new WaitForSeconds(stallAnimation);

        animator.speed = 1;
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
            currentSkillsInt = 45;
        }
        if (hp < phaseThreeHp && currentPhase == 2)
        {
            currentPhase++;
            currentSkillsInt = 130;
            animator.SetTrigger("Zone");
        }
    }

    private void OnDisable()
    {
        if (state == State.Dead)
            Destroy(gameObject);
    }
}
