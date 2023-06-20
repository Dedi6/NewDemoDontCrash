using UnityEngine;
using MyBox;

public class FriendlyAction : MonoBehaviour
{
    public Animator animator;
    public int damage;

    [SearchableEnum]
    public AudioManager.SoundList soundEffect;

    public bool useHitEffect, destroyWhenHit, changeSpeed, shouldPassThroughEnemies;
    [SerializeField] private bool useHitSFX;
    [ConditionalField("useHitSFX", false)] [SearchableEnum] public AudioManager.SoundList hitSound;
    [ConditionalField("changeSpeed", false)] [SerializeField] private float newSpeed;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == 12) // 12 is enemies
        {
            AudioManager.instance.PlaySound(soundEffect);
            col.GetComponent<Enemy>().TakeDamage(damage);
            if(!shouldPassThroughEnemies)
                HandleHitChecks();
        }
        else if (col.gameObject.layer == 8) // 8 is ground
            HandleHitChecks();
    }

    public void SetMovement(Vector2 direction, float speed)
    {
        if (changeSpeed) speed = newSpeed;
        GetComponent<Rigidbody2D>().velocity = direction * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetPositionAndMovement(Vector2 direction, float speed, Vector2 position)
    {
        transform.position = position;
        SetMovement(direction, speed);
    }

    void HandleHitChecks()
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
        if (useHitSFX)
            AudioManager.instance.PlaySound(hitSound);
    }

    private void StopMotion()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        GetComponent<CircleCollider2D>().enabled = false;
    }
}
