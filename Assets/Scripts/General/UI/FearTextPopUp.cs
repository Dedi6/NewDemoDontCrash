using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearTextPopUp : MonoBehaviour
{
    public GameObject textUp, textDown;
    public float appearTime;
    public float fadeInTime, fadeOutTime, shakeTimer;
    private Animator animatorUp, animatorDown;
    private bool isActive;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(!isActive && col.gameObject.layer == 11) // 11 is player
        {
            isActive = true;
            textUp.transform.parent.gameObject.SetActive(true);
            textDown.transform.parent.gameObject.SetActive(true);
            FadeIn();
        }
    }


    private void Start()
    {
        animatorUp = textUp.GetComponent<Animator>();
        animatorDown = textDown.GetComponent<Animator>();
    }

    /* private IEnumerator FadeInAndOut()
    {
        animator.SetTrigger("FadeIn");

        yield return new WaitForSeconds(appearTime);

        animator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(fadeOutTime + 1f);

        text.SetActive(false);
        Destroy(gameObject);
    }*/

    private void FadeIn()
    {
        animatorDown.Play("FadeIn");
        animatorUp.Play("FadeIn");

        Invoke("FadeOut", appearTime);
    }

    private void FadeOut()
    {
        animatorDown.Play("FadeOut");
        animatorUp.Play("FadeOut");

        Invoke("SetBack", fadeOutTime + 1f);
    }

    private void SetBack()
    {
        animatorDown.Play("TransperentCD");
        animatorUp.Play("TransperentCD");
        Destroy(gameObject);
    }

    public void FadeCompleted(float waitTime)
    {
        StartCoroutine(CompletedCoroutine(waitTime));
    }

    private IEnumerator CompletedCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        animatorDown.Play("Fade_CompletedUp");
        animatorUp.Play("Fade_CompletedDown");

        yield return new WaitForSeconds(0.5f);

        AudioManager.instance.PlaySound(AudioManager.SoundList.UnlockedSkill);

        yield return new WaitForSeconds(shakeTimer - 0.5f);

        GameMaster.instance.ShakeCamera(0.1f, 2f);
        //sfx
    }
}
