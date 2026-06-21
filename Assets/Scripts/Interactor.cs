using UnityEngine;
using UnityEngine.InputSystem;

interface IInteractable
{
    public void Interact();
}
public class Interactor : MonoBehaviour
{
    public Transform interactorSource;
    public float interactRange;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Ray r = new Ray(interactorSource.position, interactorSource.forward);
            if (Physics.Raycast(r, out RaycastHit hitInfo, interactRange))
            {
                if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable objInteract))
                {
                    objInteract.Interact();
                }
            }
        }
    }
}
