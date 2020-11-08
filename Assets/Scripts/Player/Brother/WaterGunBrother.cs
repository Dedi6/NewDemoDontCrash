using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGunBrother : MonoBehaviour
{

    public float speed, moveSpeedShoot, waterAmountMax, waterFillAmount, waterUseAmount = 3;
    private float waterCurrent, distanceToPlayer, moveX, moveY;
    private Vector2 dirToPlayer;
    private bool isShooting, isReloading;
    private GameObject player;
    private Rigidbody2D rb;

    public ManaBar playerSoakPoints;
    
    private State state;

    private enum State
    {
        Attacking,
        Reloading,
        Defending,
    }

    void Start()
    {
        state = State.Attacking;
        player = GameMaster.instance.playerInstance;
        rb = GetComponent<Rigidbody2D>();
        waterCurrent = waterAmountMax;
    }

    void Update()
    {
        GetVectorToPlayer();
        HandleStates();

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
    }

    private void HandleStates()
    {
        if (waterCurrent <= 0 || (distanceToPlayer > 12 && waterCurrent < waterAmountMax))
            state = State.Reloading;
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

        if (moveX > 0) moveX = 1;
        else if (moveX < 0) moveX = -1;

        if (moveY > 0) moveY = 1;
        else if (moveY < 0) moveY = -1;
    }

    private void MoveReloading()
    {
        rb.velocity = new Vector2(-moveX * speed, -moveY * speed);

        if(distanceToPlayer < 6 && GetCurrentWaterPercent() > 0.5f)     //if the player is close and has some ammo (30%)
            StopReloading();
    }

    private void MoveAttacking()
    {
        float currentSpeed = isShooting ? moveSpeedShoot : speed;
        if(distanceToPlayer > 10)
            rb.velocity = new Vector2(moveX * currentSpeed, moveY * currentSpeed);
    }


    private void Attack()
    {
        if (distanceToPlayer < 12)
        {
            isShooting = true;
            //handle particles
            if (waterCurrent - waterUseAmount >= 0)
            {
                waterCurrent -= waterUseAmount;
            }
            else
            {
                waterCurrent = 0;
                
                // not enought mana sound or somthing idk
            }
            Debug.Log("Shooting");
        }
        else
        {
            isShooting = false;
            // stop particles
            Debug.Log("Stopped Shooting");
        }

    }


    void Shoot()
    {
        
        RaycastHit2D ray = Physics2D.Raycast(transform.position, dirToPlayer, 12, 1 << 12);
        if (ray)
        {
            float distance = Vector2.Distance(transform.position, ray.transform.position);
            float time = distance / 12f;
            StartCoroutine(ShootHit(time * 0.36f, ray.transform.gameObject));
        }
        //  Invoke("SetStateNormalButCanShoot", 0.2f); // for now, later use animation event
    }

    private IEnumerator ShootHit(float time, GameObject enemy)
    {
        yield return new WaitForSeconds(time);

        ManaBar enemySoakPoints = enemy.GetComponent<ManaBar>();
        enemySoakPoints.UseMana(1);
        float currentPoints = enemySoakPoints.GetCurrentMana();

        if (currentPoints <= 0)
            Debug.Log("Soaked");

    }



    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading");
        waterCurrent += waterFillAmount;

        yield return new WaitForSeconds(0.2f);

        if (waterCurrent < waterAmountMax)
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

    private IEnumerator SwitchStateAfterTime(State newState, float time)
    {
        yield return new WaitForSeconds(time);

        state = newState;
    }

}
