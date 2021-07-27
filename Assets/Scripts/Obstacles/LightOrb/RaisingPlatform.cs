using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaisingPlatform : MonoBehaviour
{
    public float timeBeforeFalling;
    public float speed, rayDistance;
    public Animator animator;
    private Vector2 originalPos;
    public Transform rayPos;
    private Rigidbody2D rb;
    private bool isActive;

    private void Start()
    {
        originalPos = transform.position;

        rb = GetComponent<Rigidbody2D>();
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == 11 && col.transform.position.y > (transform.position.y + 2)) // 11 = player
        {
            isActive = true;
            col.transform.SetParent(transform);
        }

   //     StartCoroutine("Fall");
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.layer == 11)
        {
            col.transform.SetParent(null);
            isActive = false;
        }
    }

    private void FixedUpdate()
    {
        if(isActive)
        {
           // transform.Translate(Vector2.up * speed, Space.World);
            rb.velocity = Vector2.up * speed;
            RaycastToCieling();
        }
        else if(!isActive && transform.position.y >= originalPos.y)
        {
            //transform.Translate(Vector2.down * speed, Space.World);
            rb.velocity = Vector2.down * speed;
        }
        else if(!isActive && transform.position.y <= originalPos.y)
        {
            rb.velocity = Vector2.zero;
        }

        
    }

    private void RaycastToCieling()
    {
        RaycastHit2D ray = Physics2D.Raycast(rayPos.position, Vector2.up, rayDistance, 1 << 8);

        if(ray)
        {
            Debug.Log("fhit");
            GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>().RespawnAtLatestCheckpoint();
        }
    }

    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(timeBeforeFalling);

        AudioManager.instance.PlaySound(AudioManager.SoundList.FallingPlatformFall);
        animator.SetBool("Fall", true);

        rb.isKinematic = false;
        GetComponent<BoxCollider2D>().enabled = false;

        //Destroy(gameObject ,waitTimeDestroy);
    }

    public void PlayerHasRespawned()
    {
        StopCoroutine("Fall");
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        transform.position = originalPos;
        animator.SetBool("Fall", false);
        GetComponent<BoxCollider2D>().enabled = true;
    }

    private void OnEnable()
    {
        if (GetComponent<BoxCollider2D>().enabled == false)
            PlayerHasRespawned();

        // stop the reset platform coroutine
    }

    private IEnumerator ResetPlatform()
    {
        yield return new WaitForSeconds(15f);

        PlayerHasRespawned();
    }
}
