using UnityEngine;
using UnityEngine.Audio;

public class SettingsLoader : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    
    private void Start()
    {
        audioMixer.SetFloat("MasterVolume", PlayerPrefs.GetFloat("MasterVolume", 1));
        audioMixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVolume", 1));
        audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 1));
        audioMixer.SetFloat("UIVolume", PlayerPrefs.GetFloat("UIVolume", 1));

        var fs = PlayerPrefs.GetInt("FullScreen") == 1;
        Screen.fullScreen = fs;
        Screen.SetResolution(PlayerPrefs.GetInt("ResolutionWidth", 1920), PlayerPrefs.GetInt("ResolutionHeight", 1080), fs);
        Destroy(gameObject);
    }
}