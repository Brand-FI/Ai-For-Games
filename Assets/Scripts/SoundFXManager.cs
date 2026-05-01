using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;
    public AudioSource soundFX;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void Play(AudioClip ac, Vector3 tf, float volume)
    {
        AudioSource audioSource = Instantiate(soundFX, tf, Quaternion.identity);
        audioSource.clip = ac;
        audioSource.volume = volume;
        audioSource.Play();
        float end = audioSource.clip.length;
        Destroy(audioSource.gameObject, end);
    }
    
    public void Play(AudioClip ac, Vector3 tf, float volume, float pitch = 10f)
    {
        AudioSource audioSource = Instantiate(soundFX, tf, Quaternion.identity);
        audioSource.clip = ac;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
        float end = audioSource.clip.length;
        Destroy(audioSource.gameObject, end);
    }
}
