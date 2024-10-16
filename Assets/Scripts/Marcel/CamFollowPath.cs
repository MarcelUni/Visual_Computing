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

    void Update()
    {
        MoveCam();
    }

    private void MoveCam()
    {   
        // If the distance is farther than the max allowed distance
        if (Vector3.Distance(player.transform.position, transform.position) >= maxCameraDistance)
        {
            distanceTravelled += cameraSpeed * Time.deltaTime;
        }

        else if(Vector3.Distance(player.transform.position, transform.position) < maxCameraDistance)
        {
            distanceTravelled -= cameraSpeed * Time.deltaTime;
        }

        transform.position = pathCreators[currentPathIndex].path.GetPointAtDistance(distanceTravelled - offSet, EndOfPathInstruction.Stop);
    }
}
