using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTriggers : MonoBehaviour
{
    private GameObject doorToOpen;
    private bool isTriggered;
    public Animator animator;
    void Start()
    {
        doorToOpen = transform.parent.gameObject; 
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("player") && !isTriggered)
        {
            GameMaster gm = GameMaster.instance;
            gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.DoorTrigger);
            isTriggered = true;
            animator.SetBool("IsTriggered", true);
            doorToOpen.GetComponent<DoorOpenWithTriggers>().OpenDoor();
        }
    }

    private void ResetTrigger()
    {
        isTriggered = false;
        animator.SetBool("IsTriggered", false);
    }

    void OnDisable()
    {
        isTriggered = false;
        animator.SetBool("IsTriggered", false);
    }
}
