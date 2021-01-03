using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorDashThrough : MonoBehaviour
{

    public bool IsDoorLayedVertical;
    private bool outlineOn = false;
    private GameObject player;


    void Start()
    {
        player = GameMaster.instance.playerInstance;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("bullet"))
        {
            Highlight();
            ResetTeleport();
            Destroy(col.gameObject);
        }
    }

    void ResetTeleport()
    {
        player.GetComponent<MovementPlatformer>().CurrentBulletGameObject = gameObject;
        player.GetComponent<MovementPlatformer>().didHitAnEnemy = true;
        player.GetComponent<MovementPlatformer>().bulletHitDoor = true;
    }

    public void Highlight()
    {
        if (!outlineOn)
        {
            GetComponent<SpriteOutline>().enabled = true;
            outlineOn = true;
        }
        else
        {
            GetComponent<SpriteOutline>().enabled = false;
            outlineOn = false;
            player.GetComponent<MovementPlatformer>().CurrentBulletGameObject = null;
        }
    }

}
