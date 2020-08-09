using UnityEngine;

public class BackgroundSFXHandler : MonoBehaviour
{

    public AudioManager.SoundList theme;
    public float fadeTime;


    private void OnTriggerEnter2D(Collider2D col)
    {
        AudioManager.instance.PlayTheme(theme, fadeTime);
    }

}
