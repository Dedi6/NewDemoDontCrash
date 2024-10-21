using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Collectible_Comic : MonoBehaviour
{
    [SerializeField] Image comicImage;
    [SerializeField] private float fadeT;
    private bool isActive;

    private void Update()
    {
        if (!isActive)
            return;

        if (InputManager.instance.KeyDown(Keybindings.KeyList.Jump))
            StartCoroutine(Fade(false));
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11) // 11 is player 
        {
            GetComponent<CircleCollider2D>().enabled = false;
            StartCoroutine(Fade(true));

          
            /*if (shouldDisableTrigger)
                gameObject.SetActive(false);*/
        }
    }

    private IEnumerator Fade(bool shouldFadeIn)
    {

        if (shouldFadeIn)
        {
            while (comicImage.color.a < 1f)
            {
                comicImage.color += new Color(0, 0, 0, Time.deltaTime / fadeT);
                yield return null;
            }
            isActive = true;
            GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            while (comicImage.color.a > 0f)
            {
                comicImage.color -= new Color(0, 0, 0, Time.deltaTime / fadeT);
                yield return null;
            }
            Destroy(gameObject);
        }

    }

}
