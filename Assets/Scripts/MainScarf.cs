using UnityEngine;

public class MainScarf : MonoBehaviour
{
   
    private MovementPlatformer mp;
    public Animator animator;
    private bool groundedFlag, isDisabled;

    void Start()
    {
        mp = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
    }

    void Update()
    {
        if (mp.IsStunned())
        {
            animator.speed = 0;
            return;
        }

        if (mp.moveInput == 0)
            animator.SetBool("Run", false);
        else
            animator.SetBool("Run", true);

        if (!mp.isGrounded)
        {
            animator.speed = 0;
            groundedFlag = false;
        }
        else
        {
            animator.speed = 1;
            if (animator.GetBool("Run") && !groundedFlag)
            {
                groundedFlag = true;
                animator.Play("Scarf_Run", 0, 0f);
            }
        }

        if (!mp.canShoot && !mp.canTeleport && !isDisabled)
        {
            isDisabled = true;
            animator.SetBool("Disabled", true);
        }
        else if(mp.canShoot && isDisabled)
        {
            isDisabled = false;
            animator.SetBool("Disabled", false);
        }
    }



}
