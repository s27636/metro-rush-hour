using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Ruch")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("Kamera / mysz")]
    public Camera cam;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    private CharacterController controller;
    private float verticalVelocity = 0f;
    private float pitch = 0f; // obrót kamery góra/dół

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cam == null) cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Obrót całego ciała w poziomie
        transform.Rotate(Vector3.up * mouseX);

        // Obrót kamery w pionie (z ograniczeniem)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        if (cam != null)
            cam.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal"); // A/D
        float z = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.right * x + transform.forward * z;
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Grawitacja / skok
        if (controller.isGrounded)
        {
            verticalVelocity = -1f; // lekko dociska do podłoża
            if (Input.GetKeyDown(KeyCode.Space))
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}