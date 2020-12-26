using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefreshOrb : MonoBehaviour
{
    MovementPlatformer player;
    public float respawnTime;
    public Animator animator;

    void Start()
    {
        player = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 11)
        {
            if(!player.canShoot)
            {
                player.SetCanShootAgain();
                GetComponent<CircleCollider2D>().enabled = false;
                animator.SetTrigger("Triggered");
                StartCoroutine(AppearAgain());
            }
        }
    }

    private IEnumerator AppearAgain()
    {
        yield return new WaitForSeconds(respawnTime);

        Respawn();
    }

    void Respawn()
    {
        GetComponent<CircleCollider2D>().enabled = true;
        animator.SetTrigger("Reset");
    }
}
