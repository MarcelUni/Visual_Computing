using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera playerFollowCamera;
    public CinemachineVirtualCamera pathFollowCamera;

    public CinemachineSmoothPath cinematicPath; // Reference to the CinemachineSmoothPath
    public PlayerController pc;

    public float cinematicSpeed = 2f; // Speed at which the camera moves along the path
    private float pathPosition = 0f;  // Current position on the path

    private bool isCinematicActive = false;  // To track if we are in cinematic mode
    private bool isPlayerFollowActive = true;  // To track if we are in player-follow mode
    private bool hasCinematicPlayed = false;  // To ensure cinematic is played only once

    private void Update()
    {
        // Check if player should be on the cinematic path and if the cinematic has not played yet
        if (pc.currentPathIndex == 1 && !isCinematicActive && !hasCinematicPlayed)
        {
            SwitchToCinematic();
        }
        // Check if player should follow the player path
        else if (pc.currentPathIndex != 1 && !isPlayerFollowActive)
        {
            SwitchToPlayerFollow();
        }

        // If the camera is on the cinematic path, move it along the path
        if (isCinematicActive)
        {
            // Move the camera along the path by increasing the path position based on time and speed
            pathPosition += cinematicSpeed * Time.deltaTime;

            // Clamp the path position to ensure it doesn't go beyond the end of the path
            pathPosition = Mathf.Clamp(pathPosition, 0f, cinematicPath.MaxPos);

            // Update the camera's path position
            pathFollowCamera.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition = pathPosition;

            // Check if the camera has reached the end of the path
            if (pathPosition >= cinematicPath.MaxPos)
            {
                SwitchToPlayerFollow(); // Automatically switch back to player-follow camera
                hasCinematicPlayed = true; // Mark cinematic as played
            }
        }
    }

    // Switch to path camera for cinematic
    public void SwitchToCinematic()
    {
        playerFollowCamera.Priority = 0;
        pathFollowCamera.Priority = 10;

        // Reset the path position to start from the beginning
        pathPosition = 0f;

        // Set the flags to indicate we are in cinematic mode
        isCinematicActive = true;
        isPlayerFollowActive = false;
    }

    // Switch back to player-follow camera
    public void SwitchToPlayerFollow()
    {
        playerFollowCamera.Priority = 10;
        pathFollowCamera.Priority = 0;

        // Set the flags to indicate we are in player-follow mode
        isCinematicActive = false;
        isPlayerFollowActive = true;
    }
}
