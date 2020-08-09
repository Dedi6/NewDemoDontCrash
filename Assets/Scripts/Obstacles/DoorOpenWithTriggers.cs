using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenWithTriggers : MonoBehaviour, IRespawnResetable
{

    public int numberOfTriggers;
    public int numberOfTriggersRemaining;
    private bool isClosed = true;
    public bool isClankTriggered = false;
    public Animator animator;

    void Start()
    {
        numberOfTriggersRemaining = numberOfTriggers;
    }

    public void ResetTriggersAndDoor()
    {
        if (!isClankTriggered)
        {
            ResetDoor();
            if(transform.childCount > 0)
                BroadcastMessage("ResetTrigger");
        }
    }

    public void OpenDoor()
    {
        numberOfTriggersRemaining--;
        if (numberOfTriggersRemaining <= 0 && isClosed)
        {
            GameMaster gm = GameMaster.instance;
            gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.DoorOpen);
            isClosed = false;
            GetComponent<BoxCollider2D>().enabled = false;
            animator.SetBool("DoorOpen", true);
        }
    }

    private void ResetDoor()
    {
        if (!isClankTriggered)
        {
            numberOfTriggersRemaining = numberOfTriggers;
            isClosed = true;
            animator.SetBool("DoorOpen", false);
            GetComponent<BoxCollider2D>().enabled = true;
        }
    }
    void OnDisable()
    {
        if(!isClankTriggered)
        {
            ResetDoor();
        }
    }
    void OnEnable()
    {
        if(isClankTriggered)
        {
            animator.SetBool("DoorOpen", true);
            animator.SetBool("DoorRemainOpen", true);
            isClosed = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
    public void Clanked()
    {
        if (isClosed)
        {
           numberOfTriggersRemaining = 0;
            OpenDoor();
        }

        isClankTriggered = true;
    }

    public void PlayerHasRespawned()
    {
        ResetTriggersAndDoor();
    }
}
