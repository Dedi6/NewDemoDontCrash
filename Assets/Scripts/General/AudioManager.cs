using UnityEngine.Audio;
using UnityEngine;
using System.Collections;
using MyBox;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private bool stopTransition;
    private Coroutine transitionCoroutine;

    [System.Serializable]
    public class Sounds
    {
        public string title;
        [SearchableEnum]
        public SoundList SoundFor;
        public ClipsHelper[] arrayOfClips;

        public bool loop;

        [HideInInspector]
        public AudioSource source;
        public AudioManagerList AudioManagerType;

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
        [ConditionalField("randomizeVolumeAndPitch")] public float randomVolumeRate; // the float is the number to either go up or down.
        [ConditionalField("randomizeVolumeAndPitch")] public float randomPitchRate;

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
        RageBossPunch,
        PlayerTossedIntoWall,
        MoonBossBreathing,
        DoorTriggerTriggered,
        RespawnSound,
        FallingPlatformFall,
        MoonBossAttack,
        RageBossTeleport,
        RageBossPrep,
        RageBossPrepPunch,
        UnlockedSkill,
        Heal,
        MoonBossRun,
        MoonBossZap,
        RageBossScream,
        RageBossSlash,
        RageBossMeteors,
        RageBossEnrage,
        FrogJump,
        FrogLand,
        BigFrogLick,
        SwordDrakeDash,
        StompWave,
        RedDrakeFireBall,
        SummonMinions,
        ArrowShoot,
        MoonBossTheme,
        MoonBossTransition,
        FearThemeTransition,
        FearThemeLoop,
        FearThemeEnding,
        Save,
        Load,
        SmokeBomb,
        PlayerSpeak,
        BrotherSpeak,
        DefaultDialogueSpeak,
        HeightBossTeleport,
        HeightBossCannon,
        HeightBossSword,
        HeightBossFan,
        HeightBossDashStart,
        HeightBossDashing,
        HeightBossQuake,
        HeightBossQuakeStart,
        HeightBossSummon,
        HeightBossPortal,
        HeightBossDodge,
        HeightBossDead,
        HeightFear_BossBG,
        DrillBoss_BottomDrill,
        DrillBoss_Explode,
        DrillBoss_Prepare,
        DrillBoss_Shoot,
        DrillBoss_StopDrill,
        DrillBoss_Move,
        DrillBoss_DrillDown,
        DrillBoss_SideDrill,
        DrillBoss_FloaterProjectile,
        Floater_Move,
        Floater_Attack,
        Floater_Died,
        Floater_Projectile,
        Gate_Activate,
        Gate_Active,
        Gate_Deactivate,
        HealCrystal_Hit,
        HealCrystal_Create,
        Tp_Switch,
        DrillBoss_bg_Start,
        DrillBoss_bg_loop,
        DrillBoss_bg_ending,
    }

    public enum AudioManagerList
    {
        SFX,
        Music,
    }

    private AudioSource currentTheme;

    public float fadeTime, BgMusicVolume;
    private float mixerMaster, mixerMusic, mixerSFX;
    public Sounds[] ListOfSounds;
    public AudioMixer mixer;
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
                currentSound.source.outputAudioMixerGroup = GetAudioMixerGroup(currentSound.AudioManagerType);
            }
            //currentSound.source.volume = currentSound.volume;
            //currentSound.source.pitch = currentSound.pitch;
        }

        StartCoroutine(LoadMixerStats());
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

    public void ThreeDSound(SoundList sound, Vector3 position)
    {
        Sounds s = GetAudioClip(sound);
        int index = Random.Range(0, s.arrayOfClips.Length);
        GameObject soundObject = new GameObject("Sound");
        soundObject.transform.position = position;
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = s.arrayOfClips[index].audioClip;
        audioSource.volume = s.arrayOfClips[index].volume;
        audioSource.pitch = s.arrayOfClips[index].pitch;
        audioSource.maxDistance = 200f;
        audioSource.spatialBlend = 0.5f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0f;
        if (s.arrayOfClips[index].randomizeVolumeAndPitch)
        {
            float rndVolume = s.arrayOfClips[index].randomVolumeRate;
            float rndPitch = s.arrayOfClips[index].randomPitchRate;
            audioSource.volume = Random.Range(audioSource.volume - rndVolume, audioSource.volume + rndVolume);
            audioSource.pitch = Random.Range(s.source.pitch - rndPitch, s.source.pitch + rndPitch);
        }
        audioSource.Play();
        Destroy(soundObject, audioSource.clip.length + 1f);
    }

    public void ThreeDSoundAdvanced(SoundList sound, Vector3 position, float maxDistance, Transform parent, float spatialBlend)
    {
        Sounds s = GetAudioClip(sound);
        int index = Random.Range(0, s.arrayOfClips.Length);
        GameObject soundObject = new GameObject("Sound");
        soundObject.transform.SetParent(parent);
        soundObject.transform.position = position;
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = s.arrayOfClips[index].audioClip;
        audioSource.volume = s.arrayOfClips[index].volume;
        audioSource.pitch = s.arrayOfClips[index].pitch;
        audioSource.maxDistance = maxDistance;
        audioSource.spatialBlend = spatialBlend;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0f;
        if (s.arrayOfClips[index].randomizeVolumeAndPitch)
        {
            float rndVolume = s.arrayOfClips[index].randomVolumeRate;
            float rndPitch = s.arrayOfClips[index].randomPitchRate;
            audioSource.volume = Random.Range(s.source.volume - rndVolume, s.source.volume + rndVolume);
            audioSource.pitch = Random.Range(s.source.pitch - rndPitch, s.source.pitch + rndPitch);
        }
        audioSource.Play();
        Destroy(soundObject, audioSource.clip.length + 1f);
    }

    public void AddSoundToGameObject(SoundList sound, Transform parent, float playTime)
    {
        StartCoroutine(StartAndStopSound(sound, parent, playTime));
    }

    private IEnumerator StartAndStopSound(SoundList sound, Transform parent, float playTime)
    {
        Sounds s = GetAudioClip(sound);
        int index = Random.Range(0, s.arrayOfClips.Length);
        GameObject soundObject = new GameObject("Sound");
        soundObject.transform.position = parent.position;
        soundObject.transform.SetParent(parent);
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = s.arrayOfClips[index].audioClip;
        audioSource.volume = s.arrayOfClips[index].volume;
        audioSource.pitch = s.arrayOfClips[index].pitch;
        audioSource.maxDistance = 200f;
        audioSource.spatialBlend = 0.5f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0f;
        if (s.arrayOfClips[index].randomizeVolumeAndPitch)
        {
            float rndVolume = s.arrayOfClips[index].randomVolumeRate;
            float rndPitch = s.arrayOfClips[index].randomPitchRate;
            audioSource.volume = Random.Range(audioSource.volume - rndVolume, audioSource.volume + rndVolume);
            audioSource.pitch = Random.Range(s.source.pitch - rndPitch, s.source.pitch + rndPitch);
        }
        audioSource.Play();

        yield return new WaitForSeconds(playTime);

        Destroy(soundObject);
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
        stopTransition = true;
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
        stopTransition = false;

        yield return new WaitForSecondsRealtime(clip.length);

        if(!stopTransition)
            PlayTheme(theme, 0);
    }

    public void StartTransitionCoroutine(SoundList transition, SoundList theme, float fadeT)
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(PlayTransitionTheme(transition, theme, fadeT));
    }

    
    private IEnumerator FadeIn(SoundList sound, float fadeT)
    {
        AudioSource source = GetAudioClip(sound).source;
        source.ignoreListenerPause = true;

        while (source.volume < BgMusicVolume)
        {
            source.volume += Time.deltaTime / fadeT;
            yield return null;
        }

        source.volume = BgMusicVolume;
    }

    private IEnumerator FadeInFromSource(AudioSource source, float fadeT)
    {
        while (source.volume < BgMusicVolume)
        {
            source.volume += Time.deltaTime / fadeT;
            yield return null;
        }

        source.volume = BgMusicVolume;
    }

    public void StartFadeOutSource(AudioSource source, float fadeT)
    {
        StartCoroutine(FadeOut(source, fadeT));
    }

    public void StartFadeInSource(AudioSource source, float fadeT)
    {
        StartCoroutine(FadeInFromSource(source, fadeT));
    }

    private IEnumerator FadeOut(AudioSource source, float fadeT)
    {
        while(source.volume > 0.01f)
        {
            source.volume -= Time.deltaTime / fadeT;
            yield return null;
        }

        source.volume = 0f;
       // source.Stop();
    }

    public void FadeOutCurrent(float fadeT)
    {
        StartCoroutine(FadeOut(currentTheme, fadeT));
    }

    private AudioMixerGroup GetAudioMixerGroup(AudioManagerList type)
    {
        switch(type)
        {
            case AudioManagerList.SFX:
                return mixer.FindMatchingGroups("SFX")[0];
            case AudioManagerList.Music:
                return mixer.FindMatchingGroups("Music")[0];
        }
        return null;
    }

    public void SetVolumeMaster(float volume)
    {
        mixer.SetFloat("Volume", volume);
        mixerMaster = volume;
    }
    public void SetVolumeMusic(float volume)
    {
        mixer.SetFloat("MusicVolume", volume);
        mixerMusic = volume;
    }
    public void SetVolumeSFX(float volume)
    {
        mixer.SetFloat("SFXVolume", volume);
        mixerSFX = volume;
    }

    public void OpenedPauseMenu()
    {
        mixer.SetFloat("MusicVolume", mixerMusic - 10f);
    }

    public void ClosedPauseMenu()
    {
        mixer.SetFloat("MusicVolume", mixerMusic);
    }

    public void SaveMixerStats()
    {
        PlayerPrefs.SetFloat("MixerMaster", mixerMaster);
        PlayerPrefs.SetFloat("MixerMusic", mixerMusic);
        PlayerPrefs.SetFloat("MixerSFX", mixerSFX);
    }


    private IEnumerator LoadMixerStats()
    {
        yield return null;
        SetVolumeMaster(PlayerPrefs.GetFloat("MixerMaster"));
        SetVolumeMusic(PlayerPrefs.GetFloat("MixerMusic"));
        SetVolumeSFX(PlayerPrefs.GetFloat("MixerSFX"));
    }
}

