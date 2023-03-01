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
    public Color activeColor;
    private Color defaultColor;
    private SpriteRenderer sprite;

    void Start()
    {
        originalPos = transform.localPosition;
        targetPos = endPos.localPosition;
        GetComponent<BoxCollider2D>().size = GetComponent<SpriteRenderer>().size;
        sprite = GetComponent<SpriteRenderer>();
        defaultColor = sprite.color;

        mp = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();

        mp.jumpedNow += SwitchBool;
    }

    void Update()
    {

        if(shouldMove)
        {
            if(gameObject.layer != 13)
            {
                gameObject.layer = 13;
                ChangeChildLayer(13);
                sprite.color = activeColor;
            }
            if (!goReverse)
            {
                transform.localPosition = Vector2.MoveTowards(transform.localPosition, targetPos, speed * Time.deltaTime);
                if (Vector2.Distance(transform.localPosition, targetPos) <= 0)
                {
                    RevertToNormal(true);
                }
            }
            else
            {
                transform.localPosition = Vector2.MoveTowards(transform.localPosition, originalPos, speed * Time.deltaTime);
                if (Vector2.Distance(originalPos, transform.localPosition) <= 0)
                {
                    RevertToNormal(false);
                }
            }
        }
    }

    private void ChangeChildLayer(int newLayer)
    {
        foreach(Transform child in transform)
        {
            if(child.gameObject.layer != 10)
                child.gameObject.layer = newLayer;
        }
    }

    private void RevertToNormal(bool isReverse)
    {
        gameObject.layer = 8;
        shouldMove = false;
        goReverse = isReverse;
        sprite.color = defaultColor;
        ChangeChildLayer(8);
    }

    private void SwitchBool()
    {
        if (mp.groundedMemory >= 0)
            shouldMove = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, endPos.position);
    }
}
