using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CamFollowPath : MonoBehaviour
{
    public List<PathCreator> pathCreators;
    public int currentPathIndex = 0;
    public PlayerController player;
    public float offSet;
    public float maxCameraDistance = 5f;

    public float distanceTravelled;
    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(player.transform.position, transform.position) <= maxCameraDistance)
        {
            distanceTravelled += 0.5f;
        }

        if (currentPathIndex >= pathCreators.Count)
        {
            return;
        }
        distanceTravelled = player.distanceTravelled;
        transform.position = pathCreators[currentPathIndex].path.GetPointAtDistance(distanceTravelled - offSet, EndOfPathInstruction.Stop);
    }
}
