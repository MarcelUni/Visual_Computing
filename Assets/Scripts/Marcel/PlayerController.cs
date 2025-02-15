using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Move parameters")]
    public List<PathCreator> pathCreators;
    public int currentPathIndex = 0;
    [SerializeField] private float normalMoveSpeed;
    [SerializeField] private float sneakMoveSpeed;
    [SerializeField] private float speedUpAndSlowDownTime;
    [SerializeField] private float rotateSpeed;
    [SerializeField] public GameObject playerModelObject;

    private Vector3 resetPosition = new Vector3(0.122f, -0.89f, -0.155f); // Desired reset position

    [SerializeField] private Collider switchPathCollider;
    [SerializeField] private float switchPathCooldownTime = 2f;

    [Header("Light Path Animation")]
    public GameObject lightPrefab; // Prefab for the light that will animate
    public float lightSpeed = 10f; // Speed of the light moving along the path
    public int lightLoopCount = 3; // Number of times the light loops the path

    public bool moveForward;
    public bool moveBackward;
    public bool canMove;
    public bool isJumping = false;
    public bool canMoveForward = true; // If player reaches a closed door.

    [Header("Behavior bools")]
    public bool isSneaking;
    public bool isMoving;
    public bool isDead;
    public bool canSwitchPath = false;
    public bool isAtPathChoice = false;

    private float currentVelocity;
    private float currentSpeed = 0;
    private bool isTransitioning = false; // Indicates if a path transition is happening

    [HideInInspector] public float distanceTravelled = 8;
    private Rigidbody rb;
    public Animator anim;
    [SerializeField] private CamFollowPath camFollowPath;

    // Reference to the AudioManager
    private AudioManager audioManager;

    [Header("Footstep Timing")]
    [SerializeField] private float footstepInterval = 0.5f; // Time between footstep sounds when walking
    [SerializeField] private float sneakFootstepInterval = 0.8f; // Time between footstep sounds when sneaking
    private float footstepTimer = 0f;

    [Header("Events")]
    public UnityEvent deathEvent;

    void Start()
    {
        audioManager = AudioManager.instance;

        canSwitchPath = false;
        canMove = true;
        isAtPathChoice = false;
        rb = GetComponent<Rigidbody>();

        anim.SetBool("IsMoving", false);

        footstepTimer = 0f;
    }

    private void Update()
    {

        // Update the footstep timer
       /* if (isMoving && canMove && !isDead)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayFootstep();
                if (isSneaking)
                {
                    footstepTimer = sneakFootstepInterval;
                }
                else
                {
                    footstepTimer = footstepInterval;
                }
            }
        }
        else
        {
            footstepTimer = 0f;
        }*/

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Interact") || anim.GetCurrentAnimatorStateInfo(0).IsName("InteractOut"))
            canMove = false;
        else
            canMove = true;
        

        UpdateAnimations();

        if(isDead)
        {
            canMove = false;
        }
    }

    SurfaceType DetectSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.5f))
        {
            // Assume the surface type is Default unless determined otherwise
            SurfaceType surfaceType = SurfaceType.Default;

            // Determine the surface type based on the hit collider's tag, layer, or material
            if (hit.collider.CompareTag("Grass"))
                surfaceType = SurfaceType.Grass;
            else if (hit.collider.CompareTag("Stone"))
                surfaceType = SurfaceType.Stone;
            else if (hit.collider.CompareTag("Wood"))
                surfaceType = SurfaceType.Wood;

            return surfaceType;
        }

        return SurfaceType.Default;
    }

    public void PlayFootstep()
    // noget man kan g�re at at lave en animation event p� animationerne
    // som kalder den her method, fordi s� kan vi kalde den pr�cis n�r foden rammer jorden
    {
        SurfaceType currentSurface = DetectSurface();
        audioManager.PlayFootstepSound(currentSurface);
    }


    /// <summary>
    /// Ik fuck med den her method eow
    /// </summary>
    /// <param name="switchToNext"></param>
    /// <returns></returns>
    public IEnumerator SmoothSwitchPath(int pathIndex)
    {
        isTransitioning = true;
        isAtPathChoice = false;  // Disable path choice prompt during transition
        canSwitchPath = false;   // Temporarily disable path switching
                                 // Trigger the light animation along the chosen path
        if (lightPrefab != null)
        {
            StartCoroutine(AnimateLightOnPath(pathCreators[pathIndex]));
        }
        StartCoroutine(UpdatePathChoiceCooldown());
        if (pathIndex == currentPathIndex)
        {
            isTransitioning = false;

            yield break;
        }

        // Get the player's current position
        Vector3 playerPosition = transform.position;

        // Find the closest point on the new path to the player's current position
        float newDistanceTravelled = pathCreators[pathIndex].path.GetClosestDistanceAlongPath(playerPosition);

        // Update the currentPathIndex and camera path index
        currentPathIndex = pathIndex;
        camFollowPath.currentPathIndex = pathIndex;

        // Update the distanceTravelled to the new distance
        distanceTravelled = newDistanceTravelled;

        // Move the player slightly along the new path to avoid retriggering the collider
        distanceTravelled += 0.1f; // Adjust the value as needed

        isTransitioning = false;
        canMove = true;

        

        yield break;
    }

    private IEnumerator UpdatePathChoiceCooldown()
    {
        switchPathCollider.enabled = false;
        yield return new WaitForSeconds(switchPathCooldownTime);
        switchPathCollider.enabled = true;

    }

    // Add a persistent variable to track the last animated path
    private PathCreator lastAnimatedPath = null;

    /// <summary>
    /// Animate a light along the specified path with an upward offset, starting at the correct position.
    /// </summary>
    private IEnumerator AnimateLightOnPath(PathCreator pathCreator)
    {
        // Check if the path is the same as the last animated path
        if (pathCreator == lastAnimatedPath)
        {
            yield break; // Do not play if the same path was used
        }

        // Update the last animated path to the current one
        lastAnimatedPath = pathCreator; // bare så man ikke kan spille den samme hele tiden

        if (pathCreator == null)
        {
            Debug.LogError("PathCreator is null. Cannot animate light.");
            yield break;
        }

        // Use the player's exact travelled distance on the current path as the light's starting position
        float lightDistance = distanceTravelled; // Directly use distanceTravelled to align with the player

        // Get the start position from the path and apply upward offset
        Vector3 startPosition = pathCreator.path.GetPointAtDistance(lightDistance, EndOfPathInstruction.Stop);
        float upwardOffset = 7.0f; // Adjust for the desired height
        float traverseDistance = 50f; // Adjust for the desired distance
        float fadeOutDuration = 1.0f; // Time it takes for the light to fade out

        startPosition.y += upwardOffset;

        // Instantiate the light prefab at the aligned position
        GameObject lightObject = Instantiate(lightPrefab, startPosition, Quaternion.identity);
        Light lightComponent = lightObject.GetComponent<Light>();
        if (lightComponent == null)
        {
            Debug.LogError("LightPrefab must have a Light component.");
            Destroy(lightObject);
            yield break;
        }

        float totalDistance = pathCreator.path.length;

        // Loop through the path multiple times
        for (int loop = 0; loop < lightLoopCount; loop++)
        {
            float startDistance = lightDistance;
            float endDistance = Mathf.Min(startDistance + traverseDistance, pathCreator.path.length); // End after 50 units or path length

            // Move the light along the path
            while (lightDistance < endDistance)
            {
                lightDistance += lightSpeed * Time.deltaTime;

                // Get the new position along the path
                Vector3 newPosition = pathCreator.path.GetPointAtDistance(lightDistance, EndOfPathInstruction.Loop);

                // Apply upward offset
                newPosition.y += upwardOffset;

                // Update the light's position
                lightObject.transform.position = newPosition;

                yield return null;
            }
            // Start fading out the light
            float fadeTimer = fadeOutDuration;
            float initialIntensity = lightComponent.intensity;

            while (fadeTimer > 0f)
            {
                fadeTimer -= Time.deltaTime;
                lightComponent.intensity = Mathf.Lerp(0f, initialIntensity, fadeTimer / fadeOutDuration);
                yield return null;
            }

            // reset lightDistance to start next loop
            lightDistance = startDistance;
            lightComponent.intensity = initialIntensity; // Reset for next loop
        }

        // Destroy the light object after the animation
        Destroy(lightObject);
    }




    private void FixedUpdate()
    {
        if (!isMoving)
        {
            // Reset the player model's position
          //  playerModelObject.transform.localPosition = resetPosition;
        }
        if (canMove == false || pathCreators.Count == 0)
            return;

        if (isTransitioning)
        {
            // During transition, update position and rotation
            UpdatePositionAndRotation();
            return;
        }

        if (moveForward)
        {
            if (isSneaking)
            {
                MoveForward(sneakMoveSpeed);
            }
            else
            {
                MoveForward(normalMoveSpeed);
            }
        }
        else if (moveBackward)
        {
            if (isSneaking)
            {
                MoveBackward(sneakMoveSpeed);
            }
            else
            {
                MoveBackward(normalMoveSpeed);
            }
        }
        else
        {
            Decelerate();
        }
    }

    private void UpdatePositionAndRotation()
    {
        // Check if the player has reached the end of the path
        if (distanceTravelled >= pathCreators[currentPathIndex].path.length || distanceTravelled <= 1)
        {
            //Debug.Log("Reached the end of the path");
            return;
        }

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreators[currentPathIndex].path.GetPointAtDistance(distanceTravelled);

        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));
        Rotate();
    }

    private void MoveForward(float speed)
    {
        if (isAtPathChoice == true || isJumping == true || canMoveForward == false)
            return;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, speed, ref currentVelocity, speedUpAndSlowDownTime);
        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        UpdatePositionAndRotation();
    }

    private void MoveBackward(float speed)
    {
        if (isAtPathChoice == true || isJumping == true)
            return;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, -speed, ref currentVelocity, speedUpAndSlowDownTime);
        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        UpdatePositionAndRotation();
    }

    private void Decelerate()
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, 0, ref currentVelocity, speedUpAndSlowDownTime);
        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        UpdatePositionAndRotation();
    }

    private void Rotate()
    {
        Vector3 pathDirection = pathCreators[currentPathIndex].path.GetDirectionAtDistance(distanceTravelled);
        Quaternion lookRotation;
        Vector3 newDirection;

        if (currentSpeed > 0)
        {
            newDirection = new Vector3(pathDirection.x, 0, pathDirection.z);
            lookRotation = Quaternion.LookRotation(newDirection);
        }
        else
        {
            newDirection = new Vector3(-pathDirection.x, 0, -pathDirection.z);
            lookRotation = Quaternion.LookRotation(newDirection);
        }

        Quaternion currentRot = playerModelObject.transform.rotation;
        playerModelObject.transform.rotation = Quaternion.Slerp(currentRot, lookRotation, rotateSpeed * Time.deltaTime);
    }


    private void UpdateAnimations()
    {
        if(canMoveForward == false || canMove == false)
        {
            anim.SetBool("IsMoving", false);
            return; 
        }
        
        anim.SetBool("IsMoving", isMoving); // vitterligt et mysterie

        anim.SetBool("IsCrouching", isSneaking);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PathTrigger") && !moveBackward && !isTransitioning)
        {
            canSwitchPath = true;
            isAtPathChoice = true;
            canMove = false;
            other.GetComponent<Collider>();
            switchPathCollider = other;
        }

        if (other.CompareTag("Final Door") || other.CompareTag("Puzzle"))
        {
            canMoveForward = false;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PathTrigger"))
        {
            // You can remove this if not needed
            canSwitchPath = false;
            isAtPathChoice = false;
        }
        if(other.CompareTag("Puzzle") || other.CompareTag("Final Door"))
        {
            canMoveForward = true;
        }
    }
}