using UnityEngine;

public class AudioUIHandler : MonoBehaviour
{
    public AudioManager.SoundList selectSoundUI;
    public AudioManager.SoundList moveSoundsUI;

    public void PlaySelectSound()
    {
        AudioManager.instance.PlaySound(selectSoundUI);
    }

    public void PlayMoveSound()
    {
        AudioManager.instance.PlaySound(moveSoundsUI);
    }
}
