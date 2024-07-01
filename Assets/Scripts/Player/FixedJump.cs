using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedJump : MonoBehaviour
{
    [SerializeField]
    private float fallMultiplayer = 3f, lowJumpMultiplayer = 2.5f;
    private float lowJumpBase;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lowJumpBase = lowJumpMultiplayer;
    }

    void FixedUpdate()
    {
        if (rb.velocity.y > 0 && !InputManager.instance.GetKey(Keybindings.KeyList.Jump) || rb.GetComponent<MovementPlatformer>().thrustHappening)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplayer - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplayer - 1) * Time.deltaTime;
            //rb.GetComponent<MovementPlatformer>().animator.SetBool("IsJumping", true);
        }
    }

    public void Set_Multiply_Equal()
    {
        lowJumpMultiplayer = fallMultiplayer;
    }

    public void ResetMultiplayer()
    {
        lowJumpMultiplayer = lowJumpBase;
    }
}
