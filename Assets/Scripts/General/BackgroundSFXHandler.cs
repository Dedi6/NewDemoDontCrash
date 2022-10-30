using UnityEngine;
using MyBox;

public class BackgroundSFXHandler : MonoBehaviour, IRespawnResetable
{
    [SearchableEnum]
    public AudioManager.SoundList theme;
    public float fadeTime;
    public bool isTransition;
    [ConditionalField(nameof(isTransition), false, true)] [SearchableEnum] public AudioManager.SoundList transitionTheme;

    public UnityEngine.Events.UnityEvent triggered;
    public bool shouldResetCollider;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == 11) // 11 is player 
            triggered.Invoke();
    }

    public void StartTheme()
    {
        if (!isTransition)
            AudioManager.instance.PlayTheme(theme, fadeTime);
        else
            AudioManager.instance.StartTransitionCoroutine(transitionTheme, theme, fadeTime);
        GetComponent<BoxCollider2D>().enabled = false;
    }

    public void FadeOutTheme()
    {
        AudioManager.instance.FadeOutCurrent(fadeTime);
        GetComponent<BoxCollider2D>().enabled = false;
    }

    public void PlayerHasRespawned()
    {
        if(shouldResetCollider)
            GetComponent<BoxCollider2D>().enabled = true;
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}
