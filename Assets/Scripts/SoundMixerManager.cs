using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundMixerManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider master;
    public Slider music;
    public Slider soundFX;
    public Slider voice;

    void Start()
    {
        audioMixer.SetFloat("volMaster", Mathf.Log10(PlayerPrefs.GetFloat("volMaster", 1) * 20f));
        audioMixer.SetFloat("volMusic", Mathf.Log10(PlayerPrefs.GetFloat("volMusic", 1) * 20f));
        audioMixer.SetFloat("volSoundFX", Mathf.Log10(PlayerPrefs.GetFloat("volSoundFX", 1) * 20f));
        audioMixer.SetFloat("volVoice", Mathf.Log10(PlayerPrefs.GetFloat("volVoice", 1) * 20f));
        master.value = PlayerPrefs.GetFloat("volMaster", 1f);
        music.value = PlayerPrefs.GetFloat("volMusic", 1f);
        soundFX.value = PlayerPrefs.GetFloat("volSoundFX", 1f);
        voice.value = PlayerPrefs.GetFloat("volVoice", 1f);
    }
    
    public void SetMaster(float vol)
    {
        audioMixer.SetFloat("volMaster", Mathf.Log10(vol) * 20f);
        PlayerPrefs.SetFloat("volMaster", vol);
        PlayerPrefs.Save();
    }

    public void SetSoundFX(float vol)
    {
        audioMixer.SetFloat("volSoundFX", Mathf.Log10(vol) * 20f);
        PlayerPrefs.SetFloat("volSoundFX", vol);
        PlayerPrefs.Save();
    }

    public void SetMusic(float vol)
    {
        audioMixer.SetFloat("volMusic", Mathf.Log10(vol) * 20f);
        PlayerPrefs.SetFloat("volMusic", vol);
        PlayerPrefs.Save();
    }

    public void SetVoice(float vol)
    {
        audioMixer.SetFloat("volVoice", Mathf.Log10(vol) * 20f);
        PlayerPrefs.SetFloat("volVoice", vol);
        PlayerPrefs.Save();
    }
}
