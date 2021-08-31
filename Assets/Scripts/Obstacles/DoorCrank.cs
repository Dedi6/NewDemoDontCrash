using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCrank : MonoBehaviour, IRespawnResetable
{
   // [HideInInspector]
    public bool isTriggered = false;
    public Animator animator;
    public UnityEngine.Events.UnityEvent CrankTriggered;
    public string nameForSave;

    private void Start()
    {
        CheckIfActive(false);
    }

    public void CheckIfActive(bool shouldPlayAudio)
    {
        if (PlayerPrefs.HasKey(nameForSave))
            Triggered(shouldPlayAudio);
        else if (!CheckIfInCurrentRoom())
            gameObject.SetActive(false);
    }

    public void Triggered(bool shouldPlayAudio)
    {
        CrankTriggered.Invoke();
        animator.SetBool("IsTriggered", true);
        isTriggered = true;
        GetComponent<CircleCollider2D>().enabled = false;

        if (shouldPlayAudio)
        {
            GameMaster gm = GameMaster.instance;
            gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.ClankTriggered);
        }

        PlayerPrefs.SetInt(nameForSave, 1);
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

    private bool CheckIfInCurrentRoom()
    {
        return ReferenceEquals(transform.parent.gameObject, GameMaster.instance.currentRoom);
    }
}
