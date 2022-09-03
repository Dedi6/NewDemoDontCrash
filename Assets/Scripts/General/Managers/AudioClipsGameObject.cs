using UnityEngine;

public class AudioClipsGameObject : MonoBehaviour
{
    [SerializeField]
    AudioSource[] clips;

   
    public void PlayAudioSource(int clipPos)
    {
        clips[clipPos].Play();
    }

    public void StopAudioSource(int clipPos)
    {
        clips[clipPos].Stop();
    }
}
