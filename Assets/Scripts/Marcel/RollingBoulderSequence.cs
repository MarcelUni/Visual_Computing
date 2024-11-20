using System.Collections;
using UnityEngine;
using PathCreation;

[CreateAssetMenu(fileName = "NewRollingBoulderSequence", menuName = "Traps/Rolling Boulder Sequence")]
public class RollingBoulderSequence : ScriptableObject
{
    public GameObject boulderPrefab; // The boulder prefab
    public float initialSpeed = 2.0f; // Starting speed
    public float maxSpeed = 10.0f; // Maximum speed
    public float acceleration = 0.5f; // Base acceleration per second
    public float gravityBoost = 2.0f; // Additional speed multiplier for steep slopes
    public float boulderLifetime = 10.0f; // Time before the boulder is destroyed

    public IEnumerator ActivateBoulder(PathCreator pathCreator)
    {
        if (pathCreator == null)
        {
            Debug.LogError("PathCreator is null. Cannot play Rolling Boulder Sequence.");
            yield break;
        }

        VertexPath path = pathCreator.path;

        // Instantiate the boulder at the start of the path
        GameObject boulder = Instantiate(boulderPrefab, path.GetPointAtTime(0), Quaternion.identity);

        float currentSpeed = initialSpeed;
        float distanceTraveled = 0f;

        while (boulder != null && distanceTraveled < path.length)
        {
            // Get the current position and the next position on the path
            Vector3 currentPosition = path.GetPointAtDistance(distanceTraveled, EndOfPathInstruction.Stop);
            Vector3 nextPosition = path.GetPointAtDistance(distanceTraveled + 0.1f, EndOfPathInstruction.Stop); // Small step ahead

            // Calculate the slope based on the change in y between the two points
            float slope = nextPosition.y - currentPosition.y;

            // Adjust speed based on slope
            if (slope < 0) // Downward slope
            {
                float slopeFactor = Mathf.Abs(slope); // The steeper the slope, the greater the factor
                currentSpeed = Mathf.Min(currentSpeed + slopeFactor * gravityBoost * Time.deltaTime, maxSpeed);
            }
            else if (slope >= 0) // Flat or upward slope
            {
                // Reduce speed slightly to simulate the boulder losing momentum
                currentSpeed = Mathf.Max(currentSpeed - gravityBoost * Time.deltaTime, initialSpeed);
            }

            // Update the distance traveled along the path
            distanceTraveled += currentSpeed * Time.deltaTime;

            // Get the position on the path
            Vector3 position = path.GetPointAtDistance(distanceTraveled, EndOfPathInstruction.Stop);

            // Move the boulder to the new position
            boulder.transform.position = position;

            // Rotate the boulder based on its movement
            float rotationAmount = currentSpeed * Time.deltaTime / (Mathf.PI * boulder.transform.localScale.x);
            boulder.transform.Rotate(Vector3.Cross(Vector3.up, nextPosition - currentPosition), rotationAmount * 360f, Space.World);

            yield return null;
        }

        // Destroy the boulder after it finishes the path
        Destroy(boulder, boulderLifetime);
    }
}
