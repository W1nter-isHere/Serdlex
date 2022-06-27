using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Utils
{
    [Serializable]
    public class Audio
    {
        public AudioType audioType;
        public AudioClip clip;
        public AudioMixerGroup audioMixerGroup;
        [Range(0, 1)] public float volume = 1;
        public bool loop;
    }
}