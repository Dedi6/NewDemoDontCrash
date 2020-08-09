using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShockWave : MonoBehaviour
{
    public Animator animator;
    [HideInInspector]

    private int dir;
    private float wallXPosition, spacingB;
    private bool toRight;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11) // player
        {
            if (col.transform.position.x > transform.position.x)
                col.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, true);
            else
                col.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, false);
            col.GetComponent<MovementPlatformer>().GotHitByAnEnemy(1);
        }
    }

    public void MoveWave(int direction, float wallXPos, bool right, float spacing)
    {
        dir = direction;
        wallXPosition = wallXPos;
        spacingB = spacing;
        toRight = right;
    }

    public void FinishedAnimation()
    {
        if ((!toRight && transform.position.x > wallXPosition) || (toRight && transform.position.x < wallXPosition))
        {
            transform.position = new Vector3(transform.position.x + spacingB * dir, transform.position.y, 0);
        }
        else
            Destroy(gameObject);
    }
}
