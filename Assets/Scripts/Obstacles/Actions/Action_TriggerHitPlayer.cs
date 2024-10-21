using UnityEngine;
using MyBox;

public class Action_TriggerHitPlayer : MonoBehaviour
{
    public Animator animator;
    [SerializeField]
    private int damage = 4;
    
    public bool useHitEffect, destroyWhenHit;
    [SerializeField] private bool useHitSFX, useUniqueGroundHitEffect, useScreenShake;
    [ConditionalField("useHitSFX", false)] [SearchableEnum] public AudioManager.SoundList hitSound;
    [ConditionalField("useScreenShake")] public float shakeTime, shakeForce;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11) // player
        {
            if (col.transform.position.x > transform.position.x)
                col.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, true);
            else
                col.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, false);
            col.GetComponent<MovementPlatformer>().GotHitByAnEnemy(damage);
            HandleHitChecks(false);
        }
        else if (col.gameObject.layer == 8) // 8 is ground
            HandleHitChecks(true);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        OnTriggerEnter2D(col);
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }

    private void StopMotion()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        GetComponent<CircleCollider2D>().enabled = false;
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }

    public void SetMovement(Vector2 direction, float speed)
    {
        GetComponent<Rigidbody2D>().velocity = direction * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetPositionAndMovement(Vector2 direction, float speed, Vector2 position)
    {
        transform.position = position;
        SetMovement(direction, speed);
    }

    public void EnableBoxCollider()
    {
        GetComponent<BoxCollider2D>().enabled = true;
    }

    public void DisableBoxCollider()
    {
        GetComponent<BoxCollider2D>().enabled = false;
    }

    void HandleHitChecks(bool hitGround)
    {
        if (useHitEffect)
        {
            if (useUniqueGroundHitEffect && hitGround)
                animator.SetTrigger("HitGround");
            else
            {
                if (useScreenShake)
                    GameMaster.instance.ShakeCamera(shakeTime, shakeForce);
                animator.SetTrigger("HitPlayer");
            }
            StopMotion();
        }
        else if (destroyWhenHit)
        {
            Destroy(gameObject);
        }
        if (useHitSFX)
            AudioManager.instance.PlaySound(hitSound);
    }

    public void TriggerNow()
    {
        if (useScreenShake)
            GameMaster.instance.ShakeCamera(shakeTime, shakeForce);
        animator.SetTrigger("HitPlayer");
        StopMotion();
    }
}
