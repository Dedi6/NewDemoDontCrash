﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class FallingPlatform : MonoBehaviour, IRespawnResetable
{
    public float timeBeforeFalling, timeBeforeRespawning;
    public float waitTimeDestroy;
    public Animator animator;
    private Vector2 originalPos;
    private Rigidbody2D rb;
    [SerializeField]
    private bool ShouldIgnoreEnable;

    public bool isOnFearOfHeights = false;
    [ConditionalField("isOnFearOfHeights")] public bool isOnFar;
    [ConditionalField("isOnFearOfHeights")] public LayerSwitcher layerSwitcher;

    private void Start()
    {
        originalPos = transform.localPosition;
        Vector2 sizeCol = GetComponent<SpriteRenderer>().size;
        GetComponent<BoxCollider2D>().size = sizeCol;
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == 11 && col.transform.position.y > (transform.position.y + 1.5f)) // 11 = player
            StartCoroutine("Fall");
    }

    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(timeBeforeFalling);

        AudioManager.instance.PlaySound(AudioManager.SoundList.FallingPlatformFall);
        animator.SetBool("Fall", true);

        rb.isKinematic = false;
        GetComponent<BoxCollider2D>().enabled = false;

        yield return new WaitForSeconds(timeBeforeRespawning);

        //animation for popping back up
        PlayerHasRespawned();
    }

    public void PlayerHasRespawned()
    {
        StopCoroutine("Fall");
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        transform.localPosition = originalPos;
        animator.SetBool("Fall", false);
        GetComponent<BoxCollider2D>().enabled = true;
        HandleHeightsCollider();
    }

    void HandleHeightsCollider()
    {
        if(isOnFearOfHeights && layerSwitcher.ShouldDisableCollider(isOnFar))
            GetComponent<BoxCollider2D>().enabled = false;
    }

    private void OnEnable()
    {
        if(!ShouldIgnoreEnable)
        {
            if (GetComponent<BoxCollider2D>().enabled == false)
                PlayerHasRespawned();
        }

       // stop the reset platform coroutine
    }

    private IEnumerator ResetPlatform()
    {
        yield return new WaitForSeconds(15f);

        PlayerHasRespawned();
    }
}
