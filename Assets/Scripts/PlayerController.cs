using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;
   // private CharacterController controller;
    public float speed = 3f; 
    private Vector2 moveInput;
    private Rigidbody rb;
    private GameObject lastHit;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    void FixedUpdate()
    {
        Vector3 moveDir = cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;
        moveDir.y = 0;
        moveDir.Normalize();
        rb.linearVelocity = moveDir * speed;
    }
    /*
    void Update()
    {
        Vector3 moveDir = cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;
        moveDir.y = 0;
        controller.Move(moveDir.normalized * speed * Time.deltaTime);
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Gnome"))
        {
            Debug.Log("YOU WIN!");
            Time.timeScale = 0f;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Interactable"))
        {
            Sound sound = other.gameObject.GetComponent<Sound>();

            if (sound != null)
            {
                sound.MakeSound();
            }
        }
    }
}
