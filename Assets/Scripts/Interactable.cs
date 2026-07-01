using UnityEngine;
using UnityEngine.Audio;

public class Interactable : MonoBehaviour
{
    private Sound soundManager;
    private void Start()
    {
        soundManager = GetComponent<Sound>();
    }
    void OnControllerColliderHit()
    {
        soundManager.MakeSound();
        Debug.Log("Playing Sound");
    }

}
