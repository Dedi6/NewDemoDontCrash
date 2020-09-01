using UnityEngine;

public class Action_TriggerHitPlayer : MonoBehaviour
{
    public Animator animator;
    public bool useHitEffect, destroyWhenHit;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11) // player
        {
            if (col.transform.position.x > transform.position.x)
                col.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, true);
            else
                col.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, false);
            col.GetComponent<MovementPlatformer>().GotHitByAnEnemy(1);
            if (useHitEffect)
            {
                animator.SetTrigger("HitPlayer");
                StopMotion();
            }
            else if(destroyWhenHit)
            {
                Destroy(gameObject);
            }
        }
        else if(col.gameObject.layer == 8) // 8 is ground
        {
            if (useHitEffect)
            {
                animator.SetTrigger("HitPlayer");
                StopMotion();
            }
            else if (destroyWhenHit)
            {
                Destroy(gameObject);
            }
        }
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }

    private void StopMotion()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
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
}
