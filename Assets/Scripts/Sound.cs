using UnityEngine;

public class Sound : MonoBehaviour
{
    public AudioClip soundClip;
    public float soundDuration = 1f;

    private AudioSource audioSource;
    private int defaultLayer;
    private int soundLayer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        defaultLayer = gameObject.layer;
        soundLayer = LayerMask.NameToLayer("Sound");
    }

    public void MakeSound()
    {
        if (audioSource != null && soundClip != null)
        {
            audioSource.PlayOneShot(soundClip);
        }

        gameObject.layer = soundLayer;
        CancelInvoke();
        Invoke(nameof(StopSound), soundDuration);
    }

    private void StopSound()
    {
        gameObject.layer = defaultLayer;
    }
}