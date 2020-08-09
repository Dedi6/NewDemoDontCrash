﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour, IRespawnResetable
{
    public float timeBeforeFalling;
    public float waitTimeDestroy;
    public Animator animator;
    private Vector2 originalPos;
    private Rigidbody2D rb;

    private void Start()
    {
        originalPos = transform.position;
        Vector2 sizeCol = GetComponent<SpriteRenderer>().size;
        GetComponent<BoxCollider2D>().size = sizeCol;
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == 11 && col.transform.position.y > transform.position.y) // 11 = player
            StartCoroutine(Fall());
    }

    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(timeBeforeFalling);

        animator.SetBool("Fall", true);

        rb.isKinematic = false;
        GetComponent<BoxCollider2D>().enabled = false;

        //Destroy(gameObject ,waitTimeDestroy);
    }

    public void PlayerHasRespawned()
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        transform.position = originalPos;
        animator.SetBool("Fall", false);
        GetComponent<BoxCollider2D>().enabled = true;
    }

    private void OnEnable()
    {
        if(GetComponent<BoxCollider2D>().enabled == false)
            animator.SetBool("Fall", true);
    }
}