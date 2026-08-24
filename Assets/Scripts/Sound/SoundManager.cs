using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum SoundType
{
    SLASH,
    HURT,
    FOOTSTEP,
    LANDING,
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
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClips = clips[UnityEngine.Random.Range(0, clips.Length)];
        instance.audioSource.PlayOneShot(randomClips, volume);

        //instance.audioSource.PlayOneShot(instance.soundList[(int)sound], volume);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif   

}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip [] sounds;
}