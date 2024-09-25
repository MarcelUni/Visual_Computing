using UnityEngine;

public class CharacterControllerScript : MonoBehaviour
{
    public float moveSpeed = 6f;  // Movement speed of the character
    public float gravity = -9.81f;  // Gravity applied to the character
    public float jumpHeight = 2f;  // Jump height

    public Transform groundCheck;  // Position to check if the character is grounded
    public float groundDistance = 0.4f;  // Radius for the ground check sphere
    public LayerMask groundMask;  // LayerMask to determine what is considered ground

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;  // Lock cursor to the center of the screen

    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleJump();
        HandleRotation();  // New: Rotate player with mouse input
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        // Rotate player around the Y axis (yaw)
        transform.Rotate(Vector3.up * mouseX);
    }

    // Movement handler
    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");  // Input for left/right (A/D or Arrow keys)
        float moveZ = Input.GetAxis("Vertical");    // Input for forward/backward (W/S or Arrow keys)

        // Move the character based on the input axes
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime);  // Apply movement
    }

    // Gravity and grounded check
    private void HandleGravity()
    {
        // Check if the character is grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;  // Reset the downward velocity when grounded
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);  // Apply vertical movement (gravity)
    }

    // Jump handler
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);  // Calculate jump velocity
        }
    }
}
