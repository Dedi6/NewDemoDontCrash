using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Movement2 : MonoBehaviour
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
    public float speedMulitiplier, fallSpeed, groundCheckDistance;
    public float lowJumpMultiplayer, fallMultiplayer, trailPauseTime;

    [Header("Teleport")]
    public GameObject tileMapFar;
    public GameObject tileMapClose;
    private bool onDefaultMap = true;
    public float sizeClose;
    public Color colorClose;
    private Color orgColor;
    private Vector2 orgSize;


    [HideInInspector]
    public bool facingRight = true;
    private bool isGrounded;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    GameMaster gm;
    public LayerMask whatIsGround;
    public ParticleSystem dust;
    public TrailRenderer trail;
    private Animator animator;
    private LayerMask currentGroundcheck;
    void Start()
    {
        GameMaster.instance.lastCheckPointPosition = transform.position;
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        gm = GameMaster.instance;
        orgColor = GetComponent<SpriteRenderer>().color;
        orgSize = transform.localScale;
        animator = GetComponent<Animator>();
        //audioM.PlayTheme(AudioManager2.SoundList.Song1, 0.3f);
        Application.targetFrameRate = 50;
    }

    void Update()
    {
        MoveStart();
        JumpMemory();
        FlipStart();

        if (Input.GetKeyDown(KeyCode.R))
            RespawnReal();
    }

    private void FixedUpdate()
    {
        JumpNow();
        Move();
        RayCastsAndChecks();
        JumpAssist();
    }


    void JumpMemory()
    {

        if (jumpMemory > 0)
            jumpMemory -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
        {
            jumpMemory = 0.2f;
           
        }
        if (isGrounded)
        {
            groundedMemory = 0.05f;
            // animator.SetBool("IsFalling", false);
            // CreateDust();
            //  OnLandEvent.Invoke();
        }
        else
        {
            if (groundedMemory > 0)
                groundedMemory -= Time.deltaTime;
        }



    }       // preparing the conditions to the jump


    private void MoveStart()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        moveInputVertical = Input.GetAxisRaw("Vertical");

        /* if(moveInput != 0)
             CreateDust();*/
    }


    private void RayCastsAndChecks()
    {
        isGrounded = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(boxCollider.bounds.size.x - 0.01f, boxCollider.bounds.size.y), 0, Vector2.down, groundCheckDistance, whatIsGround);

    }

    public void JumpNow()  // the action of jumping
    {
        if ((jumpMemory > 0) && (groundedMemory > 0))
        {
            groundedMemory = 0;
            jumpMemory = 0;

            rb.velocity = new Vector2(rb.velocity.x, 1 * jumpV);
            CreateDust();
        }
    }
    private void JumpAssist()
    {
        if (rb.velocity.y > 0 && NotPressingJump())
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplayer - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplayer - 1) * Time.deltaTime;
            //rb.GetComponent<MovementPlatformer>().animator.SetBool("IsJumping", true);
        }
    }

    private bool NotPressingJump()
    {
        if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.Space))
            return true;
        else
            return false;
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


    }



    private void FlipStart()
    {
        if (facingRight == false && moveInput > 0)     //flip the character while switching directions
            Flip();

        else if (facingRight == true && moveInput < 0)
            Flip();
    }
    void Flip()
    {/*
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);*/
    }


    public void RespawnAtLatestCheckpoint() // not really now, it teleports 
    {
        //animator play animation
        StopMovement();
        StartCoroutine(PauseTrail());
        animator.SetTrigger("Teleport");
    }

    public void TeleportNow()
    {
        ReleaseMovement();
        StartCoroutine(PauseTrail());
        // switch colliders
        if (onDefaultMap)
        {
            gameObject.layer = 12;
            GetComponent<SpriteRenderer>().color = colorClose;
            transform.localScale = new Vector3(sizeClose, sizeClose, 0);
            fHorDmpStopping += 0.1f;
            GetComponent<BlurControl>().SwitchBlur(true);
            whatIsGround = 1 << 10;
        }
        else
        {
            gameObject.layer = 13;
            GetComponent<SpriteRenderer>().color = orgColor;
            transform.localScale = orgSize;
            fHorDmpStopping -= 0.1f;
            GetComponent<BlurControl>().SwitchBlur(false);
            whatIsGround = 1 << 11;
        }
        onDefaultMap = !onDefaultMap;
        //Teleport player up
        rb.velocity = Vector2.zero;
        transform.position = new Vector3(transform.position.x, 13.4f);
    }

    void RespawnLayerReset()
    {
        gameObject.layer = 13;
        GetComponent<SpriteRenderer>().color = orgColor;
        transform.localScale = orgSize;
        fHorDmpStopping -= 0.1f;
        GetComponent<BlurControl>().SwitchBlur(false);
        whatIsGround = 1 << 11;
        onDefaultMap = true;
    }

    public void RespawnReal()
    {
        rb.velocity = Vector2.zero;
        StartCoroutine(PauseTrail());
        if (!onDefaultMap)
            RespawnLayerReset();
        StartCoroutine(RespawnCoroutine());
        transform.position = gm.lastCheckPointPosition;

    }

    private IEnumerator RespawnCoroutine()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        gameObject.layer = 8;

        yield return new WaitForSeconds(0.2f);

        GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.5f);
        
        gameObject.layer = 13;
    }

    private IEnumerator PauseTrail()
    {
        trail.enabled = false;

        yield return new WaitForSeconds(trailPauseTime);

        trail.enabled = true;
    }

    void CreateDust()
    {
        dust.Play();
    }

    void StopMovement()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
    }
    void ReleaseMovement()
    {
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}