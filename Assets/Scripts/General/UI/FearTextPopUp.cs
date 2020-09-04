using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearTextPopUp : MonoBehaviour
{
    public GameObject text;
    public float appearTime;
    public float fadeInTime, fadeOutTime;
    private Animator animator;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == 11) // 11 is player
        {
            text.SetActive(true);
            animator = text.GetComponent<Animator>();
            FadeIn();
        }
    }

    private IEnumerator FadeInAndOut()
    {
        animator.SetTrigger("FadeIn");

        yield return new WaitForSeconds(appearTime);

        animator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(fadeOutTime + 1f);

        text.SetActive(false);
        Destroy(gameObject);
    }

    private void FadeIn()
    {
        animator.SetTrigger("FadeIn");

        Invoke("FadeOut", appearTime);
    }

    private void FadeOut()
    {
        animator.SetTrigger("FadeOut");

        Invoke("DestroyText", fadeOutTime + 1f);
    }

    private void DestroyText()
    {
        text.SetActive(false);
        Destroy(gameObject);
    }
}
