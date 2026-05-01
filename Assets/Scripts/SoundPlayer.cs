using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioClip[] sound;

    public void PlaySound(int i)
    {
        if(sound[i] != null)SoundFXManager.Instance.Play(sound[i], transform.position, 1f);
    }
}
