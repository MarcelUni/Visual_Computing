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

    

    // Update is called once per frame
    void Update()
    {
        if (currentPathIndex >= pathCreators.Count)
        {
            return;
        }

        transform.position = pathCreators[currentPathIndex].path.GetPointAtDistance(player.distanceTravelled - offSet, EndOfPathInstruction.Stop);
    }
}
