using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CamFollowPath : MonoBehaviour
{
    public PathCreator pathCreator;
    public PlayerController player;
    public float offSet;

    // Update is called once per frame
    void Update()
    {
        transform.position = pathCreator.path.GetPointAtDistance(player.distanceTravelled - offSet, EndOfPathInstruction.Stop);
    }
}
