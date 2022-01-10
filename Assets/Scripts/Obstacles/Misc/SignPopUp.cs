using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SignPopUp : MonoBehaviour
{

    private Animator animator, textAnimator;
    private bool signActive, textActive;
    private MovementPlatformer player;
    [SerializeField]
    private GameObject text, background;
    [SerializeField]
    private float fadeOutTime, bgAlpha;
    private Image img;

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
        textAnimator = text.GetComponent<Animator>();
        img = background.GetComponent<Image>();
    }

    private void Update()
    {
        if (IsPressingUp() && signActive && !textActive)
        {
            ShowText();

        }
        if(InputManager.instance.KeyDown(Keybindings.KeyList.Jump) && textActive)
            StartCoroutine(FadeText());
        if(!signActive && textActive)
            StartCoroutine(FadeText());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            animator.SetTrigger("Pop");
            signActive = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            animator.SetTrigger("Fade");
            signActive = false;
        }
    }

    private void ShowText()
    {
        textActive = true;
        StartCoroutine(FadeImage(false));
        text.SetActive(true);
        background.SetActive(true);
        textAnimator.SetTrigger("FadeIn");
    }

    private IEnumerator FadeText()
    {
        StartCoroutine(FadeImage(true));
        textAnimator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(fadeOutTime);

        text.SetActive(false);
        background.SetActive(false);
        textActive = false;

    }
    private IEnumerator FadeImage(bool fadeAway)
    {
        // fade from opaque to transparent
        if (fadeAway)
        {
            // loop over 1 second backwards
            for (float i = bgAlpha; i >= 0; i -= Time.deltaTime * 1.2f)
            {
                // set color with i as alpha
                img.color = new Color(0, 0, 0, i);
                yield return null;
            }
        }
        // fade from transparent to opaque
        else
        {
            // loop over 1 second
            for (float i = 0; i <= bgAlpha; i += Time.deltaTime * 1.2f)
            {
                // set color with i as alpha
                img.color = new Color(0, 0, 0, i);
                yield return null;
            }
        }
    }

    private bool IsPressingUp()
    {
        return player.directionPressedNow == MovementPlatformer.DirectionPressed.Up;
    }
}
