using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BulletGhost : MonoBehaviour
{

    public float bulletSpeed = 20f;
    public Rigidbody2D rb;
    public bool canThrowNew;
    private GameObject player;
    private Vector2 direction, originalSpeed;
    public bool didHitAnEnemy = false;
    public Animator animator;
    private int wentThroughWall = 2;
    private bool shouldReset;
    public float vfxInterval;

    PrefabManager prefabM;
    private int finalMask = (1 << 15) | (1 << 8);
    private RaycastHit2D hitInfo;

    void Start()
    {
        player = GameMaster.instance.playerInstance;
        prefabM = PrefabManager.instance;
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

    

    private void OnCollisionEnter2D(Collision2D col)
    {           /// 31 is room triggers
        AudioManager audioManager = AudioManager.instance;
        if (col.gameObject.layer != 12) //not hitting enemy
            audioManager.PlaySound(AudioManager.SoundList.BulletHitWall);
        rb.transform.SetParent(col.transform);
        animator.SetBool("IsStuck", true);
        rb.velocity = new Vector2(0, 0);
        rb.isKinematic = true;
        /*else if(col.gameObject.layer == 8 && wentThroughWall > 0)
        {
            StartCoroutine(SetBackToNormalLayer());
        }*/
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 8)
        {
            StartCoroutine(TrailEffect());
            wentThroughWall--;
            if(wentThroughWall == 0)
            {
                GetComponent<CircleCollider2D>().isTrigger = false;
                shouldReset = true;
                StopAllCoroutines();
            }
        }
        
    }

    
    private IEnumerator SetBackToNormalLayer() // 10 is bullet
    {
        yield return new WaitForSeconds(0.05f);

        GetComponent<CircleCollider2D>().isTrigger = false;
    }


    private void TeleportInstantly()
    {
        player.transform.position = hitInfo.transform.position;
        player.GetComponent<MovementPlatformer>().rb.velocity = new Vector2(0, 0);
        player.GetComponent<MovementPlatformer>().canTeleport = false;
        Destroy(gameObject);
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

    public void DestroyBulletAndReset()
    {
        player.GetComponent<MovementPlatformer>().KillBulletObject();
        // add sound effect
    }

    public void TeleportGhost()
    {
        if(CanTeleport())
        {
            player.transform.position = new Vector2(transform.position.x, transform.position.y + 0.5f);
            if (shouldReset)
                player.GetComponent<MovementPlatformer>().SetCanShootAgain();
        }
        else
            AudioManager.instance.PlaySound(AudioManager.SoundList.DoorClose); // play sound for cant teleport
    }
    public bool CanTeleport()
    {
        var tileMap = GameObject.FindGameObjectWithTag("Tilemap");
        Tilemap d = tileMap.GetComponent<Tilemap>();
        if (!d.HasTile(d.WorldToCell(transform.position)))
            return true;
        else
            return false;
    }

    private IEnumerator TrailEffect()
    {
        prefabM.PlayVFX(PrefabManager.ListOfVFX.GhostBulletTrail, transform.position);

        yield return new WaitForSeconds(vfxInterval);

        StartCoroutine(TrailEffect());
    }

}