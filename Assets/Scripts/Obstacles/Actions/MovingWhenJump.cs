using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWhenJump : MonoBehaviour
{
    public float speed;
    private Vector2 originalPos;
    private Vector2 targetPos;
    public Transform endPos;
    private bool shouldMove = false, goReverse = false;
    private GameObject player;

    void Start()
    {
        originalPos = transform.position;
        targetPos = endPos.position;
        player = GameMaster.instance.playerInstance;
    }

    void Update()
    {
        if (InputManager.instance.KeyDown(Keybindings.KeyList.Jump) && !shouldMove)
        {
            if (player.GetComponent<MovementPlatformer>().groundedMemory > 0)
                shouldMove = true;
        }

        if (shouldMove)
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
}
