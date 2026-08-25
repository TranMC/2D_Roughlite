using System;
using UnityEngine;
using UnityEngine.Audio;
using Roguelite.Core;

public enum SoundType
{
    SLASH,
    HURT,
    FOOTSTEP,
    LANDING,
    PLAYER_DEATH,
    BOSS_DEATH,
    PERK_SELECT,
    GAME_OVER,
    VICTORY,
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    public const string VERSION = "1.1.0";
    [SerializeField] private SoundList[] soundList;
    private static SoundManager instance;
    public static SoundManager Instance => instance;
    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (instance != this)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.ignoreListenerPause = true;
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
#endif
        if (Application.isPlaying)
        {
            GameManager.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.GameOver || state == GameState.Victory)
        {
            StopMusic();
            PlaySound(state == GameState.Victory ? SoundType.VICTORY : SoundType.GAME_OVER);
        }
    }

    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        if (instance == null)
        {
            return;
        }

        if (instance.audioSource == null)
        {
            instance.audioSource = instance.GetComponent<AudioSource>();
            if (instance.audioSource == null)
            {
                return;
            }
        }

        if (instance.soundList == null)
        {
            return;
        }

        int index = (int)sound;
        if (index < 0 || index >= instance.soundList.Length)
        {
            return;
        }

        AudioClip[] clips = instance.soundList[index].Sounds;
        if (clips == null || clips.Length == 0)
        {
            return;
        }

        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        if (randomClip == null)
        {
            return;
        }

        instance.audioSource.PlayOneShot(randomClip, volume);
    }

    public static void StopMusic()
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];
            if (source == null || !source.isPlaying)
            {
                continue;
            }

            if (IsMusicSource(source))
            {
                source.Stop();
            }
        }
    }

    private static bool IsMusicSource(AudioSource source)
    {
        if (source.gameObject.name == "MusicManager")
        {
            return true;
        }

        AudioMixerGroup mixerGroup = source.outputAudioMixerGroup;
        return mixerGroup != null && mixerGroup.name == "Music";
    }
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}
