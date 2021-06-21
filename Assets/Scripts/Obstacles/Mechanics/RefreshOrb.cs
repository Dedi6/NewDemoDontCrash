using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RefreshOrb : MonoBehaviour, IRespawnResetable
{
    MovementPlatformer player;
    public float respawnTime;
    public Animator animator;
    public UnityEvent functionToDo;

    void Start()
    {
        player = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 11) // 11 is player
        {
            functionToDo.Invoke();
        }
    }

    public void RefreshCanShoot()
    {
        if (!player.canShoot)
        {
            player.SetCanShootAgain();
            GetComponent<CircleCollider2D>().enabled = false;
            animator.SetTrigger("Triggered");
            StartCoroutine(AppearAgain());
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

    private void OnEnable()
    {
        GetComponent<CircleCollider2D>().enabled = true;
    }

    public void PlayerHasRespawned()
    {
        Respawn();
    }

    public void TriggerGhostOrb(float time)
    {

        player.SetCanShootAgain();
        player.SwitchOrbType(MovementPlatformer.OrbType.Ghost, time);
        GetComponent<CircleCollider2D>().enabled = false;
        animator.SetTrigger("Triggered");
        StartCoroutine(AppearAgain());

    }


}
