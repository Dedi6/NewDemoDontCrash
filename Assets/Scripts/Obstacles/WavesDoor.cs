using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavesDoor : MonoBehaviour, IRespawnResetable
{

    public Animator animator;
    public bool shouldOpen = true, shouldOpenWhenRespawn;

    public void OpenDoor()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.DoorClose);
        GetComponent<BoxCollider2D>().enabled = false;
        animator.SetBool("IsOpen", true);
        shouldOpen = true;
    }

    public void CloseDoor()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.DoorClose);
        GetComponent<BoxCollider2D>().enabled = true;
        animator.SetBool("IsOpen", false);
        shouldOpen = false;
    }

    void OnEnable()
    {
        if(shouldOpen)
        {
            animator.SetBool("IsOpen", true);
            GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            animator.SetBool("IsOpen", false);
            GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    void OnDisable()
    {
        if (shouldOpen)
        {
            animator.SetBool("IsOpen", true);
            GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            animator.SetBool("IsOpen", false);
            GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    public void PlayerHasRespawned(bool b)
    {
        shouldOpen = b;  
    }

    public void PlayerHasRespawned()
    {
        if(shouldOpenWhenRespawn)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            animator.SetBool("IsOpen", true);
            shouldOpen = true;
        }
    }
}
