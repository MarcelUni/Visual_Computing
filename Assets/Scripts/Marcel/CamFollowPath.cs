using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using Unity.VisualScripting;

public class CamFollowPath : MonoBehaviour
{
    public List<PathCreator> pathCreators;
    public int currentPathIndex = 0;
    public PlayerController player;
    public float offSet;
    public float maxCameraDistance = 5f;
    
    public float cameraSpeed;
    public float distanceTravelled;

    void Start()
    {
        // Initialize distanceTravelled to the player's initial distance from the start of the path
        distanceTravelled = pathCreators[currentPathIndex].path.GetClosestDistanceAlongPath(player.transform.position);
    }

    void Update()
    {
        MoveCam();
    }

    private void MoveCam()
    {
        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        // If the distance is farther than the max allowed distance
        if (distanceToPlayer >= maxCameraDistance)
        {
            distanceTravelled += cameraSpeed * Time.deltaTime;
        }
        if (distanceToPlayer < maxCameraDistance)
        {
            distanceTravelled -= cameraSpeed * Time.deltaTime;
        }

        // Ensure distanceTravelled does not go below zero
        distanceTravelled = Mathf.Max(0, distanceTravelled);

        transform.position = pathCreators[currentPathIndex].path.GetPointAtDistance(distanceTravelled - offSet, EndOfPathInstruction.Stop);
    }
}
