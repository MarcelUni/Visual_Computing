using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using Unity.VisualScripting;
using System.Runtime.InteropServices;

public class CamFollowPath : MonoBehaviour
{
    public List<PathCreator> pathCreators;
    public int currentPathIndex = 0;
    public PlayerController player;
    public float offSet;
    public float maxCameraDistance = 5f;
    public float minCameraDistance;
    public float cameraSpeed;
    public float distanceTravelled;
    public float lerpSpeed;

    void Start()
    {
        // Initialize distanceTravelled to the player's initial distance from the start of the path
        distanceTravelled = pathCreators[currentPathIndex].path.GetClosestDistanceAlongPath(player.transform.position);

        StartCoroutine(SmoothFollow());
    }

    void Update()
    {
        MoveCam();
    }

    private void MoveCam()
    {
        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        // If the distance is farther than the max allowed distance
        if (distanceToPlayer > maxCameraDistance)
        {
            distanceTravelled += cameraSpeed * Time.deltaTime;
        }

        if (distanceToPlayer < minCameraDistance)
        {
            distanceTravelled -= cameraSpeed * Time.deltaTime;
        }

        // Ensure distanceTravelled does not go below zero
        distanceTravelled = Mathf.Max(0, distanceTravelled);
    }
    private IEnumerator SmoothFollow()
    {
        while (true)
        {
            // Calculate the target position
            Vector3 targetPosition = pathCreators[currentPathIndex].path.GetPointAtDistance(distanceTravelled - offSet, EndOfPathInstruction.Stop);

            // Smoothly interpolate the camera's position towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
