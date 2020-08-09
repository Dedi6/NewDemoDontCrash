using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCrank : MonoBehaviour, IRespawnResetable
{
   // [HideInInspector]
    public bool isTriggered = false;
    public Animator animator;
    public UnityEngine.Events.UnityEvent CrankTriggered;

    public void Triggered()
    {
        CrankTriggered.Invoke();
        animator.SetBool("IsTriggered", true);
        isTriggered = true;
        GetComponent<CircleCollider2D>().enabled = false;
        GameMaster gm = GameMaster.instance;
        gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.ClankTriggered);
    }

    public void PlayerDiedAfterClank()
    {
        if(isTriggered)
            CrankTriggered.Invoke();
    }

    void OnEnable()
    {
        if (isTriggered)
        {
            CrankTriggered.Invoke();
            animator.SetBool("IsTriggered", true);
        }
    }

    public void PlayerHasRespawned()
    {
        PlayerDiedAfterClank();
    }
}
