using System.Collections;
using UnityEngine;

public class WaterGunBrother : MonoBehaviour
{

    public float speed, moveSpeedShoot, waterAmountMax, waterFillAmount, waterUseAmount = 3;
    private float waterCurrent, distanceToPlayer, moveX, moveY, aimAngle, timer, offSet;
    private Vector2 dirToPlayer, aimDir;
    private bool isShooting, isReloading;
    private GameObject player;
    private Rigidbody2D rb;
    private bool facingRight;
    private int lastDirection;
    public Transform particleTransform;

    public ParticleSystem particles;
    public ManaBar playerSoakPoints;
    public Animator animator;

    private readonly string[] staticDirections = { "BrotherT_D_IdleTop", "BrotherT_D_IdleTopAngle", "BrotherT_D_IdleRight", "BrotherT_D_IdleDownAngle", "BrotherT_D_IdleDown" };
    private readonly string[] runDirections = { "BrotherTopdownTop", "BrotherTopdownTopAngle", "BrotherTopdownRight", "BrotherTopdownDownAngle", "BrotherTopdownDown" };
    
    private State state;

    private enum State
    {
        Attacking,
        Reloading,
    }

    void Start()
    {
        state = State.Attacking;
        player = GameMaster.instance.playerInstance;
        rb = GetComponent<Rigidbody2D>();
        waterCurrent = waterAmountMax;
        var main = particles.main;  // Set Default lifetime for waterGun
        main.startLifetime = 0;
    }

    void Update()
    {
        GetVectorToPlayer();
        HandleStates();
        FlipStart();

        switch (state)
        {
            case State.Attacking:
                break;
            case State.Reloading:
                break;
        }
        /*
         * states: Reloading - no ammo - needs to reload and running away. Move normally.
         * Attacking - Full ammo, or player is too close && is in distance - shooting at the player, move slow while shooting.
         * Attacking - Full ammo, but not in range - run towards player.
         */
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        SetDirection(new Vector2(Mathf.Abs(moveX), moveY));
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Attacking:
                Attack();
                MoveAttacking();
                break;
            case State.Reloading:
                MoveReloading();
                if (!isReloading)
                    StartCoroutine(Reload());
                break;
        }

        AimWater();
        
    }

    private void HandleStates()
    {
        if (waterCurrent <= 0 || (distanceToPlayer > 12 && waterCurrent < waterAmountMax))
        {
            state = State.Reloading;
            StopWater();
        }
     //   else if (distanceToPlayer < 12 || waterCurrent >= waterAmountMax)
         //   state = State.Attacking;
    }

    private void GetVectorToPlayer()
    {
        distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        dirToPlayer = (player.transform.position - transform.position).normalized;

        HandleMovement();
    }

    private void HandleMovement()
    {
        moveX = dirToPlayer.x;
        moveY = dirToPlayer.y;

        if(moveX != 0 && Mathf.Abs(moveX) > 0.1f)
        {
            if (moveX > 0) moveX = 1;
            else if (moveX < 0) moveX = -1;
        }

        if(moveY != 0 && Mathf.Abs(moveY) > 0.1f)
        {
            if (moveY > 0) moveY = 1;
            else if (moveY < 0) moveY = -1;
        }
    }

    private void MoveReloading()
    {
        rb.velocity = new Vector2(-moveX * speed, -moveY * speed);

        if(distanceToPlayer < 3 && GetCurrentWaterPercent() > 0.5f)     //if the player is close and has some ammo (30%)
            StopReloading();

        if (isReloading && waterCurrent <= 0)
            StartCoroutine(Reload());

    }

    private void MoveAttacking()
    {
        float currentSpeed = isShooting ? moveSpeedShoot : speed;
        if (distanceToPlayer > 6)
            rb.velocity = new Vector2(moveX * currentSpeed, moveY * currentSpeed);
        else
            rb.velocity = Vector2.zero;

    }


    private void Attack()
    {
        if (distanceToPlayer < 12)
        {
            isShooting = true;
            //handle particles
            var main = particles.main;  // Set Default lifetime for waterGun
            main.startLifetime = 0.36f;
            if (waterCurrent - waterUseAmount >= 0)
            {
                waterCurrent -= waterUseAmount;
                Shoot();
            }
            else
            {
                waterCurrent = 0;
                
                // not enought mana sound or somthing idk
            }
        }
        else
        {
            isShooting = false;
            // stop particles
            StopWater();
        }

    }

    void StopWater()
    {
        var main = particles.main;  // Set Default lifetime for waterGun
        main.startLifetime = 0;
    }


    void Shoot()
    {
       // Debug.DrawRay(transform.position, aimDir * 12, Color.black);

        RaycastHit2D ray = Physics2D.Raycast(transform.position, aimDir, 12, 1 << 11);
        if (ray)
        {
            float distance = Vector2.Distance(transform.position, ray.transform.position);
            float time = distance / 12f * 0.36f; // 0.36f is lifetime
            StartCoroutine(ShootHit(time, ray.transform.gameObject, aimDir));
            var main = particles.main;
            main.startLifetime = time;
        }
        //  Invoke("SetStateNormalButCanShoot", 0.2f); // for now, later use animation event
    }

    private bool IsHittingTarget(Vector2 direction)
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, direction, 12, 1 << 11);
        return ray ? true : false;
    }

    private IEnumerator ShootHit(float time, GameObject enemy, Vector2 dir)
    {
        yield return new WaitForSeconds(time);

        if(IsHittingTarget(dir))
        {
            playerSoakPoints.UseMana(1);
            float currentPoints = playerSoakPoints.GetCurrentMana();
            if (currentPoints <= 0)
                Debug.Log("Soaked");
        }

    }



    private IEnumerator Reload()
    {
        isReloading = true;
        if (waterCurrent + waterFillAmount >= waterAmountMax)
            waterCurrent = waterAmountMax;
        else
            waterCurrent += waterFillAmount;
        
        StopWater(); ///////// maybe useless

        float time = Random.Range(0.2f, 0.4f);
        yield return new WaitForSeconds(time);

        if (waterCurrent < waterAmountMax && state == State.Reloading)
            StartCoroutine(Reload());
        else if (waterCurrent >= waterAmountMax)
        {
            waterCurrent = waterAmountMax;
            isReloading = false;
            state = State.Attacking;
        }
    }

    private void StopReloading()
    {
        StopCoroutine("Reload");
        state = State.Attacking;
        isReloading = false;
    }

    private float GetCurrentWaterPercent()
    {
        return waterCurrent / waterAmountMax * 100f;
    }

    private void AimWater()
    {
        Vector2 lookDir = (Vector2)player.transform.position - rb.position;
        if (timer <= 0)
        {

            offSet = Random.Range(-11, 11);
            timer = 2.5f;
        }
        
        aimAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        var shape = particles.shape;
        shape.rotation = new Vector3(0, 0, aimAngle + offSet);

        aimDir = (Vector2)(Quaternion.Euler(0, 0, aimAngle) * Vector2.right);
    }

    private void FlipStart()
    {
 
        if (facingRight && moveX < 0)
            Flip();
        else if (!facingRight && moveX > 0)
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        int f = facingRight ? 1 : -1;
        Vector3 t = new Vector3(f, transform.localScale.y, transform.localScale.z);
        transform.localScale = t;
        particleTransform.localScale = t;
    }



    // ****** animation

    private void SetDirection(Vector2 direction)
    {
        string[] directionArray = null;
        GetCurrentClip(direction);

        // if(state = State.Normal)    Thats to switch animations array. The else is the attack array
        if (direction.magnitude < 0.1f)
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
        if (Mathf.Abs(v.x) < 1)
            v = new Vector2(0, v.y);
        if (Mathf.Abs(v.y) < 1)
            v = new Vector2(v.x, 0);
        Vector2 downAngleV = new Vector2(1, -1);
        Vector2 upAngleV = new Vector2(1, 1);
        Vector2 rightV = new Vector2(1, 0);
        Vector2 upV = new Vector2(0, 1);
        Vector2 downV = new Vector2(0, -1);

        if (state == State.Reloading)
        {
            v = new Vector2(v.x, -v.y);
            if(facingRight && moveX > 0)
                Flip();
            else if (!facingRight && moveX < 0)
                Flip();
        }

        if (v == upV)
            lastDirection = 0;
        if (v == upAngleV)
            lastDirection = 1;
        if (v == rightV)
            lastDirection = 2;
        if (v == downAngleV)
            lastDirection = 3;
        if (v == downV)
            lastDirection = 4;

    }

}
