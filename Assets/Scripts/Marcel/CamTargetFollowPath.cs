using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CamTargetFollowPath : MonoBehaviour
{
    private PlayerController pc;
    public List<PathCreator> pathCreators;
    public int currentPathIndex = 0;

    private void Start()
    {
        pc = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        // Get the rotation from the path at the current distance travelled by the player
        Quaternion pathRotation = pathCreators[currentPathIndex].path.GetRotationAtDistance(pc.distanceTravelled);

        // Preserve the original z rotation
        Quaternion currentRotation = transform.rotation;

        // Create a new rotation based in the pathrotation and z currentrotation
        Quaternion newRotation = Quaternion.Euler(pathRotation.eulerAngles.x, pathRotation.eulerAngles.y, currentRotation.eulerAngles.z);

        // Apply the combined rotation to the camera's transform
        transform.rotation = newRotation;
    }
}
