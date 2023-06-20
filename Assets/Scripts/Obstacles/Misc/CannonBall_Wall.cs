using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall_Wall : MonoBehaviour
{
    public string nameForSave;

    private void Start()
    {
        CheckIfActive(false);
    }

    public void CheckIfActive(bool shouldPlayAudio)
    {
        if (!PlayerPrefs.HasKey(nameForSave))
            KeepOpen();
        else if (!CheckIfInCurrentRoom())
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 21 && collision.tag == "CannonBall") // 21 is friendly action
            BreakWall(true);
    }

    void BreakWall(bool shouldPlayAudio)
    {
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<Animator>().SetBool("Triggered", true);

        if (shouldPlayAudio)
        {
            AudioManager.instance.PlaySound(AudioManager.SoundList.ClankTriggered); // change SFX
        }

        PlayerPrefs.SetInt(nameForSave, 1);
    }

    private bool CheckIfInCurrentRoom()
    {
        return ReferenceEquals(transform.parent.gameObject, GameMaster.instance.currentRoom);
    }

    private void OnEnable()
    {
        if (!PlayerPrefs.HasKey(nameForSave))
            KeepOpen();
    }

    void KeepOpen()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<Animator>().SetTrigger("Lock");
    }
}
