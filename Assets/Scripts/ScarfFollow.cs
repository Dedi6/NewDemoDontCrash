using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScarfFollow : MonoBehaviour, IRespawnResetable
{
    public Transform pointToFollow;
    public Transform pointIdle;
    public float maxDistance;
    public float speed;
    private float currentSpeed;
    private float currentDistance;
    private MovementPlatformer mp;
    private Vector2 currentPoint;
    public Animator animator;
    private bool isDisabled;

    void Start()
    {
        mp = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
        mp.teleportedNow += TeleportToTarger;
        currentDistance = maxDistance;
        currentPoint = pointToFollow.position;
        currentSpeed = speed;
    }

    private void OnDisable()
    {
        mp.teleportedNow -= TeleportToTarger;
    }

    void Update()
    {
        if (mp.IsStunned())
        {
            currentSpeed = 0;
            return;
        }

        HandleMovement();

        HandleAnimator();
    }

    void HandleMovement()
    {
        if (Vector2.Distance(transform.position, pointToFollow.position) > currentDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, currentPoint, currentSpeed * Time.deltaTime);
        }


        if (mp.rb.velocity.magnitude > 30)
        {
            currentSpeed = 40;
        }
        else
            currentSpeed = speed;


        if (mp.rb.velocity == Vector2.zero)
        {
            currentDistance = 0;
            currentPoint = pointIdle.position;
        }
        else
        {
            currentDistance = maxDistance;
            currentPoint = pointToFollow.position;
        }
    }

    void HandleAnimator()
    {
        if (!mp.canShoot && !mp.canTeleport && !isDisabled)
        {
            isDisabled = true;
            animator.SetBool("Disabled", true);
        }
        else if (mp.canShoot && isDisabled)
        {
            isDisabled = false;
            animator.SetBool("Disabled", false);
        }
    }

    public void TeleportToTarger()
    {
        transform.position = pointToFollow.position;
    }

    public void PlayerHasRespawned()
    {
        TeleportToTarger();
    }
}
