using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MoveController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float jumpSpeed = 5f;
    public float runMultiplier = 2f;
    public float sneakMultiplier = 0.5f;
    public float gravity = -9.81f;

    public float cameraTransitionSpeed = 3f; // controls the fov zoom effect
    public float cameraLookAtDamping = 1f;   // Controls the speed of LookAt transitions

    private Vector3 velocity;
    private CharacterController characterController;

    public CinemachineVirtualCamera playerFollowCamera;
    public CinemachineVirtualCamera enemyFocusCamera;

    public Transform cameraTransform;
    public Transform enemyTransform;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerFollowCamera.Priority = 10;
        enemyFocusCamera.Priority = 0;

        // Set initial damping for player camera
        SetCameraLookAtDamping(playerFollowCamera, cameraLookAtDamping);
    }

    private void Update()
    {
        HandleMovement();

        // Cinematic Event, use trigger colliders
        if(Input.GetKeyDown(KeyCode.E))
        {
            FocusOnEnemy();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToPlayerCamera();
        }
    }

    void FocusOnEnemy()
    {
        enemyFocusCamera.Priority = 20;
        playerFollowCamera.Priority = 0;

        enemyFocusCamera.LookAt = enemyTransform; // Enter the cinemachine component and change the look at taget

        SetCameraLookAtDamping(enemyFocusCamera, cameraLookAtDamping); // Apply damping

        StartCoroutine(CinematicZoom(enemyFocusCamera, 40f, cameraTransitionSpeed)); // Slow zoom in on enemy/target

        
    }

    void ResetToPlayerCamera()
    {
        playerFollowCamera.Priority = 10;
        enemyFocusCamera.Priority = 0;


        // reset zoom back to player
        StartCoroutine(CinematicZoom(playerFollowCamera, 60f, cameraTransitionSpeed));

        // Reset lookat and follow back to player
        enemyFocusCamera.LookAt = cameraTransform;
        enemyFocusCamera.Follow = cameraTransform;

        SetCameraLookAtDamping(enemyFocusCamera, cameraLookAtDamping); // Apply damping
    }

    private void HandleMovement()
    {
        // Handle gravity
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Keeps the player grounded
        }

        // Get input for movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Flatten the camera's forward and right vectors to avoid upward/downward movement
        forward.y = 0f;
        right.y = 0f;

        // Normalize the vectors
        forward.Normalize();
        right.Normalize();

        // Calculate the movement direction based on camera orientation
        Vector3 movement = (forward * z + right * x).normalized;

        // Determine movement speed (normal, running, or sneaking)
        float currentSpeed = movementSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= runMultiplier;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed *= sneakMultiplier;
        }

        // Apply movement
        characterController.Move(movement * currentSpeed * Time.deltaTime);

        // Handle jumping
        if (Input.GetButton("Jump") && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpSpeed * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Move character with gravity applied
        characterController.Move(velocity * Time.deltaTime);
    }

    // Coroutine for creating smooth zoom (if spamming the E and R buttons it glitches as it tries to player both zoom effects)

    IEnumerator CinematicZoom(CinemachineVirtualCamera virtualCamera, float targetFOV, float duration)
    {
        float startFOV = virtualCamera.m_Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / duration);
            yield return null;
        }
        virtualCamera.m_Lens.FieldOfView = targetFOV;
    }

    // make the cinemachine lookat have more damping in the transitions between the player and another target
    void SetCameraLookAtDamping(CinemachineVirtualCamera virtualCamera, float dampingValue)
    {
        var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
        if(composer != null)
        {
            composer.m_HorizontalDamping = dampingValue;
            composer.m_VerticalDamping = dampingValue;
        }
    }

}