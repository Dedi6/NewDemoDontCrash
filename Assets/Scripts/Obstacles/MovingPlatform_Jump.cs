using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform_Jump : MonoBehaviour
{
    public float speed;
    private Vector2 originalPos;
    private Vector2 targetPos;
    public Transform endPos;
    private bool shouldMove = false, goReverse = false;
    private MovementPlatformer mp;

    void Start()
    {
        originalPos = transform.position;
        targetPos = endPos.position;
        GetComponent<BoxCollider2D>().size = GetComponent<SpriteRenderer>().size;

        mp = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();

        mp.jumpedNow += SwitchBool;
    }

    void Update()
    {

        if(shouldMove)
        {
            if (!goReverse)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                if (Vector2.Distance(transform.position, targetPos) <= 0)
                {
                    shouldMove = false;
                    goReverse = true;
                }
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, originalPos, speed * Time.deltaTime);
                if (Vector2.Distance(originalPos, transform.position) <= 0)
                {
                    shouldMove = false;
                    goReverse = false;
                }
            }
        }
    }

    private void SwitchBool()
    {
        if (mp.groundedMemory >= 0)
            shouldMove = true;
    }
}
