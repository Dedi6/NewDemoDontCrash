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
        player = GameObject.Find("Dirt");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("bullet"))
        {
            Highlight();
            player.GetComponent<MovementPlatformer>().CurrentBulletGameObject = this.gameObject;
            player.GetComponent<MovementPlatformer>().didHitAnEnemy = true;
            player.GetComponent<MovementPlatformer>().bulletHitDoor = true;
            Destroy(col.gameObject);
        }
    }

    public void Highlight()
    {
        if (!outlineOn)
        {
            this.GetComponent<SpriteOutline>().enabled = true;
            outlineOn = true;
        }
        else
        {
            this.GetComponent<SpriteOutline>().enabled = false;
            outlineOn = false;
        }
    }
}
