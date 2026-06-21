using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    private Sound soundManager;
    private void Start()
    {
        soundManager = GetComponent<Sound>();
    }
    public void Interact()
    {
        soundManager.MakeSound();
        Debug.Log("Playing Sound");
    }
}
