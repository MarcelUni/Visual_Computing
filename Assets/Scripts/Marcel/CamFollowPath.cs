using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CamFollowPath : MonoBehaviour
{
    // opdaterede scriptet til bare at have et constant offset, vi kan gøre sådan,
    // at når man går backwards eller forwards bliver offsettet ændret så man kan se hvor man går
    // istedet for at få player distance, så får den bare player position på pathen direkte.
    // så kan vi bruge math lerp til at lerpe til spillerens position på pathen
    // og så kan vi bruge en coroutine til at lerp til den position

    public List<PathCreator> pathCreators; // List of path creators
    public int currentPathIndex = 0;       // Current path index
    public PlayerController player;       // Reference to the player
    public float offset;                  // Offset to maintain relative to the player
    public float cameraSpeed = 5f;        // Speed at which the camera moves along the path
    public float lerpSpeed = 5f;          // Speed of smooth interpolation

    private float distanceTravelled = 0f; // Current distance travelled along the path

    public bool dynamicCamera = false;    // Toggle dynamic camera

    void Start()
    {
        // Initialize the camera's distance based on the player's starting position
        distanceTravelled = pathCreators[currentPathIndex].path.GetClosestDistanceAlongPath(player.transform.position);
        StartCoroutine(SmoothFollow());
    }

    void Update()
    {
        UpdateDistanceTravelled();
    }

    private void UpdateDistanceTravelled()
    {
        if (dynamicCamera)
        {
            if (player.moveForward)
            {
                offset = -14f;
            }
            else if (player.moveBackward)
            {
                offset = 14f;
            }
        }

        // Align the camera's distance to the player's current position on the path
        float playerDistance = pathCreators[currentPathIndex].path.GetClosestDistanceAlongPath(player.transform.position);

        // Smoothly interpolate `distanceTravelled` towards `playerDistance`
        distanceTravelled = Mathf.Lerp(distanceTravelled, playerDistance + offset, cameraSpeed * Time.deltaTime);
    }

    private IEnumerator SmoothFollow()
    {
        while (true)
        {
            // Get the target position for the camera based on `distanceTravelled`
            Vector3 targetPosition = pathCreators[currentPathIndex].path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);

            // Smoothly move the camera towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
