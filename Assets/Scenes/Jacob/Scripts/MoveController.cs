using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MoveController : MonoBehaviour
{

    public float cameraZoomSpeed = 3f; // controls the fov zoom effect
    public float cameraLookAtSpeed = 1f;   // Controls the speed of LookAt transitions

    private Vector3 velocity;
    private CharacterController characterController;

    public CinemachineVirtualCamera playerFollowCamera;
    public CinemachineVirtualCamera enemyFocusCamera;

    public Transform cameraTransform;
    public Transform enemyTransform;

    private bool triggered = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerFollowCamera.Priority = 10;
        enemyFocusCamera.Priority = 0;

        // Set initial damping for player camera
        SetCameraLookAtDamping(playerFollowCamera, cameraLookAtSpeed);
    }

    private void Update()
    {
       

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

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("TriggerEnemyLookat") && triggered == false)
        {
            FocusOnEnemy();
            
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TriggerEnemyLookat") && triggered == false)
        {
            ResetToPlayerCamera();
            triggered = true;
        }
    }

    void FocusOnEnemy()
    {
        enemyFocusCamera.Priority = 20;
        playerFollowCamera.Priority = 0;

        enemyFocusCamera.LookAt = enemyTransform; // Enter the cinemachine component and change the look at taget

        SetCameraLookAtDamping(enemyFocusCamera, cameraLookAtSpeed); // Apply damping

        StartCoroutine(CinematicZoom(enemyFocusCamera, 40f, cameraZoomSpeed)); // Slow zoom in on enemy/target (40f is the target fov)

        
    }

    void ResetToPlayerCamera()
    {
        playerFollowCamera.Priority = 20;
        enemyFocusCamera.Priority = 0;

        // Reset lookat back to player
        playerFollowCamera.LookAt = cameraTransform;

        SetCameraLookAtDamping(playerFollowCamera, cameraLookAtSpeed); // Apply damping

        // reset zoom back to player
        StartCoroutine(CinematicZoom(playerFollowCamera, 60f, cameraZoomSpeed));



       
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