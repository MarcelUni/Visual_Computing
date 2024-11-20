using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRollingBoulderSequence", menuName = "Traps/Rolling Boulder Sequence")]
public class RollingBoulderSequence : ScriptableObject
{
    public GameObject boulderPrefab; // The boulder prefab
    public float initialSpeed = 2.0f; // Starting speed
    public float maxSpeed = 10.0f; // Maximum speed
    public float acceleration = 0.5f; // Acceleration per second
    public float boulderLifetime = 10.0f; // Time before the boulder is destroyed

    public IEnumerator ActivateBoulder(Transform spawnPoint, Transform endPoint)
    {
        // Instantiate the boulder at the spawn point
        GameObject boulder = Instantiate(boulderPrefab, spawnPoint.position, Quaternion.identity);

        float currentSpeed = initialSpeed;

        // Let the boulder roll towards the end point
        while (boulder != null && Vector3.Distance(boulder.transform.position, endPoint.position) > 0.5f)
        {
            // Gradually increase the speed up to the max speed
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

            // Move the boulder towards the end point
            boulder.transform.position = Vector3.MoveTowards(
                boulder.transform.position,
                endPoint.position,
                currentSpeed * Time.deltaTime
            );

            // Rotate the boulder based on its movement
            float rotationAmount = currentSpeed * Time.deltaTime / (Mathf.PI * boulder.transform.localScale.x);
            boulder.transform.Rotate(Vector3.back, rotationAmount * 360f, Space.World);

            yield return null;
        }

        // Destroy the boulder after it reaches the end or after a set time
        Destroy(boulder, boulderLifetime);
    }
}
