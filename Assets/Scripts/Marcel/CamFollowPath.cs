using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CamFollowPath : MonoBehaviour
{
    public List<PathCreator> pathCreators;
    public int currentPathIndex = 0;
    public PlayerController player;
    public float offsetBehindPlayer = 10f;  // Offset behind the player for forward movement
    public float offsetAheadPlayer = 10f;   // Offset ahead of the player for backward movement
    public float maxCameraDistance = 5f;
    public float minCameraDistance;
    public float cameraSpeed = 5f;
    public float distanceTravelled;
    public float lerpSpeed = 5f;

    // Track the current camera offset direction to avoid continuous reapplication
    private float currentCameraOffset = 0f;
    private float desiredCameraOffset = 0f;

    void Start()
    {
        // Initialize distanceTravelled to the player's initial distance
        distanceTravelled = pathCreators[currentPathIndex].path.GetClosestDistanceAlongPath(player.transform.position);
        currentCameraOffset = offsetBehindPlayer; // Default offset when starting forward
        StartCoroutine(SmoothFollow());
    }

    void Update()
    {
        AdjustCameraOffset();
        MoveCam();
    }

    private void MoveCam()
    {
        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        // Adjust camera's distanceTravelled based on player movement and desired camera offset
        float targetDistance = player.distanceTravelled - desiredCameraOffset;

        // Lerp towards the targetDistance to achieve smooth transition
        distanceTravelled = Mathf.Lerp(distanceTravelled, targetDistance, cameraSpeed * Time.deltaTime);

        // Ensure distanceTravelled does not go below zero
        distanceTravelled = Mathf.Max(0, distanceTravelled);
    }

    private void AdjustCameraOffset()
    {
        // Check movement direction of the player and set the desired offset
        if (player.moveForward && !player.moveBackward)
        {
            desiredCameraOffset = offsetBehindPlayer;
        }
        else if (player.moveBackward && !player.moveForward)
        {
            desiredCameraOffset = -offsetAheadPlayer;
        }
        else
        {
            // When player is not moving or changing direction, maintain current offset
            desiredCameraOffset = currentCameraOffset;
        }

        // Update the current camera offset to the new desired offset
        currentCameraOffset = desiredCameraOffset;
    }

    private IEnumerator SmoothFollow()
    {
        while (true)
        {
            if (pathCreators.Count == 0)
            {
                yield return null;
                continue;
            }

            // Calculate the target position on the path considering the distance and offset
            Vector3 targetPosition = pathCreators[currentPathIndex].path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);

            // Smoothly interpolate the camera's position towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
