using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using AudioType = Utils.AudioType;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private List<Audio> audios;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicSource;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        
        Destroy(gameObject);
    }

    private void Start()
    {
        Play(AudioType.MenuMusic, isMusic: true, fadeIn: 2f);
    }

    public void Play(AudioType audioType, bool interrupt = false, bool isMusic = false, float fadeIn = 0f)
    {
        var a = audios.FirstOrDefault(a => a.audioType == audioType);
        var source = isMusic ? musicSource : audioSource;
        
        if (a == null) return;
        
        if (source.isPlaying)
        {
            if (!interrupt) return;
            source.Stop();
        }

        source.volume = fadeIn != 0 ? 0 : a.volume;
        source.clip = a.clip;
        source.outputAudioMixerGroup = a.audioMixerGroup;
        source.loop = a.loop;
        
        source.Play();

        if (fadeIn != 0)
        {
            LeanTween.value(gameObject, f => source.volume = f, 0, 1, fadeIn);
        }
    }

    public void Interrupt(bool isMusic = false)
    {
        var source = isMusic ? musicSource : audioSource;
git
        if (source.isPlaying)
        {
            source.Stop();
        }
    }
}
