using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CamTargetFollowPath : MonoBehaviour
{
    private PlayerController pc;
    public PathCreator pathCreator;

    private void Start()
    {
        pc = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        // Get the rotation from the path at the current distance travelled by the player
        Quaternion pathRotation = pathCreator.path.GetRotationAtDistance(pc.distanceTravelled);

        // Preserve the original z rotation
        Quaternion currentRotation = transform.rotation;
        Quaternion newRotation = Quaternion.Euler(pathRotation.eulerAngles.x, pathRotation.eulerAngles.y, currentRotation.eulerAngles.z);
        
        // Apply the combined rotation to the object's transform
        transform.rotation = newRotation;

    //     Quaternion pathDirection = pathCreator.path.GetRotationAtDistance(pc.distanceTravelled);

    //     Vector3 newDirection = new Vector3(0, 0, pathDirection.z);

    //     Quaternion pathRotation = Quaternion.LookRotation(newDirection);
        
    //     transform.rotation = pathRotation;
    }
}
