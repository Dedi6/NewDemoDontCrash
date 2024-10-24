using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatePortal : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    private bool isActive;

    [SerializeField]
    private FadeIn overlay;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == 11 || col.gameObject.layer == 6)
        {
            animator.SetTrigger("Activate");
            isActive = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == 11 || col.gameObject.layer == 6)
        {
            animator.SetTrigger("Deactivate");
            isActive = false;
        }
    }

    private void Update()
    {
        if (isActive && InputManager.instance.KeyDown(Keybindings.KeyList.Up))
        {
            isActive = false;
            StartCoroutine(SwitchScenes());
        }
    }

    private IEnumerator SwitchScenes()
    {
        StartCoroutine(overlay.FadeTo(0.5f, 0.25f));
        yield return new WaitForSeconds(0.25f);
        PauseMenu.GoToNextLevel();
    }

    public void StartSceneFade()
    {
        overlay.SetFull();
        StartCoroutine(overlay.FadeTo(0f, 0.75f));
    }
}
