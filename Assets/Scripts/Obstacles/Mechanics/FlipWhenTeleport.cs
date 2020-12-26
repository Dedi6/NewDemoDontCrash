using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipWhenTeleport : MonoBehaviour
{
    private MovementPlatformer mp;
    private Transform player;
    private bool top;
    void Start()
    {
        player = GameMaster.instance.playerInstance.transform;
        mp = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();

        mp.teleportedNow += test;
    }

    void test()
    {
        Debug.Log("fddg");
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (!top)
        {
            player.eulerAngles = new Vector3(0, 0, 180f);
            rb.gravityScale = -20f;
        }
        else
        {
            player.eulerAngles = Vector3.zero;
            rb.gravityScale = 3f;
        }
        
        top = !top;
    }

    public void test2()
    {
        Debug.Log("sdsdsdsdsds");
    }
}
