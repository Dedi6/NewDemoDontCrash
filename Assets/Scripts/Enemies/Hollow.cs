using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hollow : MonoBehaviour, ISFXResetable
{

    [Header("General")]
    [Space]
    public Rigidbody2D enemy;
    public Transform rayCheckPointGround;
    private bool facingRight = false;
    private Vector2 raycastDirection;
    public float wallCheckDistance = 1, attackCheckDistance, knockBackTime;
    public float groundCheckDistance = 1;
    public GameObject hollowToSpawn;

    [Header("Attacking")]
    [Space]
    public float speedMulitiplier;
    public float summonCooldown, pauseBeforeAttack, spawnRadius;
    private bool wasTriggered = false;
    //private bool onCooldown = false, delaySkill = true;
    private Vector2 spawnPosition;


    public Animator animator;
    private BoxCollider2D boxCollider;
    GameObject player;

    private State state;

    private enum State
    {
        Normal,
        Attack,
        Summoning,
        Stunned,
        Dead,
    }
    void Start()
    {
        enemy = GetComponent<Rigidbody2D>();
        raycastDirection = new Vector2(-1, 0);
        boxCollider = GetComponent<BoxCollider2D>();
        player = GameMaster.instance.playerInstance;
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
            case State.Attack:
                if (player.transform.position.x > enemy.transform.position.x && !facingRight && (Mathf.Abs(transform.position.x - player.transform.position.x) > 0.5))
                        Flip();
                else if (player.transform.position.x < enemy.transform.position.x && facingRight && (Mathf.Abs(transform.position.x - player.transform.position.x) > 0.5))
                        Flip();
                break;
            case State.Dead:
                break;
        }

       // StateHandler();
        HandleEnemyClassObjects();
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Normal:
                /*
                if (wasTriggered && !onCooldown)
                    StartCoroutine(SummonCoroutine(summonCooldown));*/
                break;
            case State.Attack:
                /*
                if (wasTriggered && !onCooldown)
                    StartCoroutine(SummonCoroutine(summonCooldown));*/
                Attack();
                break;
        }

        

        //    Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //     Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, 0), Vector2.down * (boxCollider.bounds.extents.y + groundCheckDistance));
        //    Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - 0.1f, boxCollider.bounds.extents.y + groundCheckDistance, 0), Vector2.right * (boxCollider.bounds.extents.x + groundCheckDistance));
    }

    private void HandleEnemyClassObjects()
    {
        GetComponent<Enemy>().facingRight = facingRight;
    }
    private void StateHandler()
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, attackCheckDistance, 1 << 11);
        if(col != null)
            state = State.Attack;
        if (state == State.Attack && !wasTriggered)
            wasTriggered = true;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
        raycastDirection = new Vector2(-raycastDirection.x, 0);
    }


    private void Attack()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speedMulitiplier * Time.deltaTime);
    }

    private void SummonNew()
    {
        Vector2 spawnPosCheck = Random.insideUnitCircle * spawnRadius + (Vector2)player.transform.position;
        Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y);
        var tileMap = GameObject.FindGameObjectWithTag("Tilemap");
        Tilemap d = tileMap.GetComponent<Tilemap>();
        if (!d.HasTile(d.WorldToCell(spawnPosCheck)) && (spawnPosCheck - playerPos).magnitude > 3)
            spawnPosition = spawnPosCheck;
        else
            SummonNew();
        animator.SetTrigger("CreatingNew");
    }

    /*
    private IEnumerator SummonCoroutine(float cooldown)
    {
        onCooldown = true;

        if (delaySkill)
        {
            yield return new WaitForSeconds(cooldown/3);
            delaySkill = false;
        }

        state = State.Summoning;
        SummonNew();

        yield return new WaitForSeconds(cooldown);

        onCooldown = false;
    }*/

    public void SetStateNormal()
    {
        state = State.Normal;
      //  delaySkill = true;
    }

    public void SetStateDead()
    {
        state = State.Dead;
    }

    public void BlinkVFX()
    {
        animator.SetTrigger("Blink");
    }

    public void SpawnHollow()
    {
        GameObject hollow = Instantiate(hollowToSpawn, spawnPosition, Quaternion.identity, transform.parent);
        StartCoroutine(hollow.GetComponent<Enemy>().StunEnemy(0.5f));
        hollow.GetComponent<Hollow>().animator.SetTrigger("BeingCreated");
        if (hollow.GetComponent<SpriteOutline>().enabled == true)
            hollow.GetComponent<SpriteOutline>().enabled = false;
    }

    public void DestroyHollow()
    {
        Destroy(gameObject); 
    }

    public void ResetSFXCues()
    {
    }
}
