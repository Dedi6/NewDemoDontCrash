using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavesDoor : MonoBehaviour
{

    public Animator animator;
    public bool boolValue = true;

    public void OpenDoor()
    {
        GameMaster gm = GameMaster.instance;
        gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.DoorClose);
        GetComponent<BoxCollider2D>().enabled = false;
        animator.SetBool("IsOpen", true);
        boolValue = true;
    }

    public void CloseDoor()
    {
        GameMaster gm = GameMaster.instance;
        gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.DoorClose);
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
        Debug.Log("b " + b);
        boolValue = b;  
    }
}
