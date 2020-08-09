﻿using UnityEngine.Audio;
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [System.Serializable]
    public class Sounds
    {
        public SoundList SoundFor;
        public ClipsHelper[] arrayOfClips;

        public bool loop;

        [HideInInspector]
        public AudioSource source;

    }

    [System.Serializable]
    public class ClipsHelper
    {
        public AudioClip audioClip;

        [Range(0f, 1f)]
        public float volume;
        [Range(.1f, 3f)]
        public float pitch;
        public bool randomizeVolumeAndPitch;
        public float randomVolumeRate; // the float is the number to either go up or down.
        public float randomPitchRate; // same ^here
    }

    public enum SoundList
    {
        StepGrass,
        StepWood,
        PlayerAttack,
        PlayerHit,
        PlayerDie,
        PlayerLand,
        PlayerJump,
        BulletFired,
        Teleport,
        EnemyHit,
        EnemyDie,
        DoorOpen,
        DoorClose,
        ClankTriggered,
        DoorTrigger,
        HiddenDoorOpen,
        StepStone,
        BulletHitWall,
        BulletHitEnemy,
        EnemyHitGround,
        HitWall,
        LandGrass,
        LandStone,
        FirstTestSong,
        Fear1,
        Fear2,
        Fear3,
        Fear4,
        Fear5,
    }

    private AudioSource currentTheme;

    public float fadeTime;
    public Sounds[] ListOfSounds;
    private AudioClip currentClip;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
            Destroy(gameObject);

        foreach (Sounds currentSound in ListOfSounds)
        {
            for (int i = 0; i < currentSound.arrayOfClips.Length; i++)
            {
                currentSound.source = gameObject.AddComponent<AudioSource>();
                currentSound.source.clip = currentSound.arrayOfClips[i].audioClip;
                currentSound.source.loop = currentSound.loop;
            }
            //currentSound.source.volume = currentSound.volume;
            //currentSound.source.pitch = currentSound.pitch;
        }
    }


    public void PlaySound(SoundList sound)
    {
        Sounds s = GetAudioClip(sound);
        int index = Random.Range(0, s.arrayOfClips.Length);
        s.source.clip = s.arrayOfClips[index].audioClip;
        s.source.volume = s.arrayOfClips[index].volume;
        s.source.pitch = s.arrayOfClips[index].pitch;
        if (s.arrayOfClips[index].randomizeVolumeAndPitch)
        {
            float rndVolume = s.arrayOfClips[index].randomVolumeRate;
            float rndPitch = s.arrayOfClips[index].randomPitchRate;
            s.source.volume = Random.Range(s.source.volume - rndVolume, s.source.volume + rndVolume);
            s.source.pitch = Random.Range(s.source.pitch - rndPitch, s.source.pitch + rndPitch);
        }
        s.source.Play();
    }

    private Sounds GetAudioClip(SoundList sound)
    {
        foreach (Sounds currentSound in ListOfSounds)
        {
            if (currentSound.SoundFor == sound)
            {
                return currentSound;
            }
        }
        Debug.LogError("Sound" + sound + "not found");
        return null;
    }

    public void PlayTheme(SoundList theme, float fadeT)
    {
        PlaySound(theme);
        if (currentTheme != null)
            StartCoroutine(FadeOut(currentTheme, fadeT));
        StartCoroutine(FadeIn(theme, fadeT));
        currentTheme = GetAudioClip(theme).source;
    }

    public IEnumerator PlayTransitionTheme(SoundList transition, SoundList theme, float fadeT)
    {
        PlayTheme(transition, fadeT);
        AudioClip clip = GetAudioClip(transition).arrayOfClips[0].audioClip;

        yield return new WaitForSeconds(clip.length);

        PlayTheme(theme, 0);
    }

    private IEnumerator FadeIn(SoundList sound, float fadeT)
    {
        AudioSource source = GetAudioClip(sound).source;

        while (source.volume < 1f)
        {
            source.volume += Time.deltaTime / fadeT;
            yield return null;
        }

        source.volume = 1f;
    }

    private IEnumerator FadeOut(AudioSource source, float fadeT)
    {
        while(source.volume > 0.01f)
        {
            source.volume -= Time.deltaTime / fadeT;
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }

}
