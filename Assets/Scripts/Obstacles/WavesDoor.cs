using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavesDoor : MonoBehaviour, IRespawnResetable
{

    public Animator animator;
    public bool boolValue = true, shouldOpenWhenRespawn;

    public void OpenDoor()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.DoorClose);
        GetComponent<BoxCollider2D>().enabled = false;
        animator.SetBool("IsOpen", true);
        boolValue = true;
    }

    public void CloseDoor()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.DoorClose);
        GetComponent<BoxCollider2D>().enabled = true;
        animator.SetBool("IsOpen", false);
        boolValue = false;
    }

    void OnEnable()
    {
        if(boolValue)
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
        if (boolValue)
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
        boolValue = b;  
    }

    public void PlayerHasRespawned()
    {
        if(shouldOpenWhenRespawn)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            animator.SetBool("IsOpen", true);
            boolValue = true;
        }
    }
}
