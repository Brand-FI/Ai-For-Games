using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;
    private CharacterController controller;
    public float speed = 3f; 
    private Vector2 moveInput;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        Vector3 moveDir = cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;
        moveDir.y = 0;
        controller.Move(moveDir.normalized * speed * Time.deltaTime);
    }
}
