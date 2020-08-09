using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{

    public float bulletSpeed =20f;
    public Rigidbody2D rb;
    public bool canThrowNew;
    private GameObject player;
    public GameObject bulletObject;
    private Vector2 direction, originalSpeed;
    public bool didHitAnEnemy = false;
    public Animator animator;

   
    private int finalMask = (1 << 15) | (1 << 8); 
    private RaycastHit2D hitInfo;
    
    void Start()
    {
        player = GameObject.Find("Dirt");
        direction = player.GetComponent<MovementPlatformer>().directionPressed;
        
        if (direction.Equals(new Vector2(0, 0)))
        {
            rb.velocity = transform.right * bulletSpeed;
            hitInfo = Physics2D.Raycast(transform.position, transform.right, 120f, finalMask);
        }
        else
        {
            rb.velocity = direction.normalized * bulletSpeed;
            hitInfo = Physics2D.Raycast(transform.position, direction, 120f, finalMask);
        }
        RotateBullet();
        player.GetComponent<MovementPlatformer>().shouldICheckIsGrounded = false;
        player.GetComponent<MovementPlatformer>().canShoot = false;
    }

    void Update()
    {
        if (hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Flower"))
                TeleportInstantly();
        }


    }
   
    private void OnCollisionEnter2D(Collision2D col)
    {
        AudioManager audioManager = AudioManager.instance;
        if (col.gameObject.layer != 12) //not hitting enemy
            audioManager.PlaySound(AudioManager.SoundList.BulletHitWall);
        rb.transform.SetParent(col.transform);
        animator.SetBool("IsStuck", true);
        rb.velocity = new Vector2(0,0);
        rb.isKinematic = true;
    } 

    
    private void TeleportInstantly()
    {
        player.transform.position = hitInfo.transform.position;
        player.GetComponent<MovementPlatformer>().rb.velocity = new Vector2(0, 0);
        player.GetComponent<MovementPlatformer>().canTeleport = false;
        Destroy(bulletObject);
    }

    private void RotateBullet()
    {
        Vector2 v = rb.velocity;
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SlowBullet(float speedModifier)
    {
        originalSpeed = rb.velocity;
        rb.velocity = rb.velocity.normalized * speedModifier;
    }

    public void SetSpeedNormal()
    {
        rb.velocity = originalSpeed;
    }

}