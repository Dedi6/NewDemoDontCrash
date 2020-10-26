using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    public float moveSpeed, moveSpeedShoot, idleTimerMax = 0.05f;
    private float moveX, moveY, idleTimer, aimAngle;
    private Rigidbody2D rb;
    private State state;
    private bool usingKeyboard, facingRight = true;
    private int lastDirection;
    private Vector2 dirPressedAbs, mousePos;

    private readonly string[] staticDirections = { "IdleAngleDown", "IdleAngleTop", "IdleRight", "IdleUp", "IdleDown" };
    private readonly string[] runDirections = { "RunDownAngle", "RunTopAngle", "RunRight", "RunUp", "RunDown" };

    public bool canShoot;
    public Transform aimObject;
    public Camera cam;
    private Animator animator;
    private InputManager input;

    private enum State
    {
        Normal,
        Attacking,
        NormalButCanShoot,
    }

   
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        state = State.Normal;
        animator = GetComponent<Animator>();
        input = InputManager.instance;
        usingKeyboard = input.IsUsingKeyboard();
        GetCurrentClip(Vector2.down);
        SetDirection(Vector2.down);
    }

    void Update()
    {
        switch(state)
        {
            case State.Normal:
                FlipStart();
                ShootStart();
                break;
        }

        HandleMoveInputs();
        AimCursor();

        if (idleTimer > 0)
            idleTimer -= Time.deltaTime;

        if (usingKeyboard)
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        else
        {
            Vector2 inputDir = new Vector2(Input.GetAxis("rightStickHor"), Input.GetAxis("rightStickVert"));
            mousePos = inputDir;
        }

        if (Input.GetKeyDown(KeyCode.L))
            input.ChangeKeybindings();
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Normal:
                Move();
                break;
            case State.Attacking:
                MoveWhileShooting();
                break;
        }
    }

    private void HandleMoveInputs()
    {
        if (usingKeyboard)
        {

            if (input.GetKey(Keybindings.KeyList.Up))
            {
                moveY = 1;
            }
            if (input.GetKey(Keybindings.KeyList.Down))
            {
                moveY = -1;
            }
            if (input.GetKey(Keybindings.KeyList.Right))
            {
                moveX = 1;
            }
            if (input.GetKey(Keybindings.KeyList.Left))
            {
                moveX = -1;
            }
            if (input.KeyUp(Keybindings.KeyList.Up))
            {
                moveY = 0;
                idleTimer = idleTimerMax;
            }
            if (input.KeyUp(Keybindings.KeyList.Down))
            {
                moveY = 0;
                idleTimer = idleTimerMax;
            }
            if (input.KeyUp(Keybindings.KeyList.Right))
            {
                moveX = 0;
                idleTimer = idleTimerMax;
            }
            if (input.KeyUp(Keybindings.KeyList.Left))
            {
                moveX = 0;
                idleTimer = idleTimerMax;
            }
        }
        else
        {
            moveX = Input.GetAxisRaw("Horizontal");
            moveY = Input.GetAxisRaw("Vertical");

            if (moveX > 0) moveX = 1;
            else if (moveX < 0) moveX = -1;

            if (moveY > 0) moveY = 1;
            else if (moveY < 0) moveY = -1;
        }

        

    }

    private void FlipStart()
    {
        if (facingRight && moveX < 0)
            Flip();
        else if (!facingRight && moveX > 0)
            Flip();
    }

    private void Move()
    {
        Vector2 moveVector = new Vector2(moveX, moveY).normalized;
        rb.velocity = new Vector2(moveVector.x * moveSpeed, moveVector.y * moveSpeed);
        Vector2 dirPressed = new Vector2(moveX, moveY);
        dirPressedAbs = new Vector2(Mathf.Abs(moveX), moveY);
        if(idleTimer <= 0)
            GetCurrentClip(dirPressedAbs);
        SetDirection(dirPressed);


        /*Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg + 180f;  /////// look at mouse
        GetCurrentClipMouse(angle);
        SetDirection(dirPressed);*/
    }

    private void Flip()
    {
        facingRight = !facingRight;
        int f = facingRight ? 1 : -1;
        transform.localScale = new Vector3(f, transform.localScale.y, transform.localScale.z);
        aimAngle += 180f;
    }

    private void SetDirection(Vector2 direction)
    {
        string[] directionArray = null;

         // if(state = State.Normal)    Thats to switch animations array. The else is the attack array
        if(direction.magnitude < 0.1f)
        {
            directionArray = staticDirections;
        }
        else
        {
            directionArray = runDirections;
        }

        animator.Play(directionArray[lastDirection]);
    }


    void GetCurrentClip(Vector2 v)
    {
        Vector2 downAngleV = new Vector2(1, -1);
        Vector2 upAngleV = new Vector2(1, 1);
        Vector2 rightV = new Vector2(1, 0);
        Vector2 upV = new Vector2(0, 1);
        Vector2 downV = new Vector2(0, -1);

        if (v == downAngleV)
            lastDirection = 0;
        if (v == upAngleV)
            lastDirection = 1;
        if (v == rightV)
            lastDirection = 2;
        if (v == upV)
            lastDirection = 3;
        if (v == downV)
            lastDirection = 4;

    }

    void GetCurrentClipMouse(float angle)
    {
        if (angle >= 270.1 && angle <= 360f)
        {
            angle = 270 - (angle - 270);
            if(facingRight && angle <= 247.51f)
                Flip();
        }
        else if (angle >= 0 && angle <= 89.9f)
        {
            angle = 90 + (90 - angle);
            if (facingRight && angle >= 112.51f)
                Flip();
        }
        else
        {
            if (!facingRight)
            {
                if (angle > 180 && angle <= 247.51f) // up section
                    Flip();
                else if (angle < 180 && angle >= 112.51f) // down section
                    Flip();
            }
        }

        if(angle >= 157.5f && angle <= 202.5f)      //right
            lastDirection = 2;
        else if ((angle >= 202.6f && angle <= 247.5f))    //up angle
            lastDirection = 1;
        else if(angle >= 247.6f && angle <= 292.5f)     //up 
            lastDirection = 3;
        else if((angle >= 112.5f && angle <= 157.4f))  // down angle
            lastDirection = 0;
        else if(angle >= 90f && angle <= 112.4f) // down
            lastDirection = 4;

    }

    private void ShootStart()
    {

        if(usingKeyboard)
        {
            if (Input.GetMouseButtonDown(0))
                Shoot();
        }
        else
        {
            if (input.GetKey(Keybindings.KeyList.Skill2))
                ShootController();
        }
    }
    
    void Shoot()
    {
        state = State.Attacking;
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg + 180f;
        GetCurrentClipMouse(angle);
        SetDirection(lookDir); // change to animator.play(AttackArray[lastDirection]);
        Invoke("SetStateNormal", 0.2f); // for now, later use animation event
    }

    void ShootController()
    {
        state = State.Attacking;
        float angle = Mathf.Atan2(mousePos.x, mousePos.y) * Mathf.Rad2Deg + 90f;
        if (angle < 0)
            angle += 360f;
        GetCurrentClipMouse(angle);
        SetDirection(mousePos); // change to animator.play(AttackArray[lastDirection]);
        Invoke("SetStateNormal", 0.2f); // for now, later use animation event
    }

    private void MoveWhileShooting()
    {
        Vector2 moveVector = new Vector2(moveX, moveY).normalized;
        rb.velocity = new Vector2(moveVector.x * moveSpeedShoot, moveVector.y * moveSpeedShoot);
    }

    public void SetStateNormal()
    {
        state = State.Normal;
    }

    public void SwitchToOrFromJoystick()
    {
        usingKeyboard = !usingKeyboard;
    }

    private void AimCursor()
    {
         if (usingKeyboard)
        {
            Vector2 lookDir = mousePos - rb.position;
            aimAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg + 180f;
            if (!facingRight)
                aimAngle += 180f;
        }
        else 
        {
            if (mousePos.magnitude > 0.05f)
            {
                aimAngle = Mathf.Atan2(mousePos.x, mousePos.y) * Mathf.Rad2Deg + 90f;
                if (!facingRight)
                    aimAngle += 180f;
            }
        }

        aimObject.rotation = Quaternion.Euler(0, 0, aimAngle);
    }

}
