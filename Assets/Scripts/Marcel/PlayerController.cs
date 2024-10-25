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
    public bool moveForward;
    public bool moveBackward;
    public bool canMove;
    public bool isJumping = false;
    [HideInInspector] public bool canMoveForward = true; // If player reaches a closed door.

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

    // Keep track of trigger entry counts
    private Dictionary<string, int> triggerEntryCounts = new Dictionary<string, int>();

    void Start()
    {
        canSwitchPath = false;
        canMove = true;
        isAtPathChoice = false;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isAtPathChoice)
        {
            // Disable movement during path choice
            canMove = false;

            // Check for input to switch path or continue
            if (Input.GetKeyDown(KeyCode.V))
            {
                StartCoroutine(SmoothSwitchPath(true)); // Switch to the next path smoothly
                isAtPathChoice = false; // Player has made a decision
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                isAtPathChoice = false; // Player chooses to continue on the current path
                canMove = true; // Re-enable movement
            }
        }
        else
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Interact") || anim.GetCurrentAnimatorStateInfo(0).IsName("InteractOut"))
                canMove = false;
            else
                canMove = true;
        }

        UpdateAnimations();

        if(isDead)
        {
            canMove = false;
        }
    }

    /// <summary>
    /// Ik fuck med den her method eow
    /// </summary>
    /// <param name="switchToNext"></param>
    /// <returns></returns>
    private IEnumerator SmoothSwitchPath(bool switchToNext)
    {
        isTransitioning = true;
        canMove = false;

        int previousPathIndex = currentPathIndex;

        if (switchToNext)
        {
            currentPathIndex = (currentPathIndex + 1) % pathCreators.Count; // Switch to the next path
            camFollowPath.currentPathIndex = (camFollowPath.currentPathIndex + 1) % camFollowPath.pathCreators.Count; // Switch camera path
            yield return null;
        }

        isTransitioning = false;
        canMove = true;
    }

    private void FixedUpdate()
    {
        if (!canMove || pathCreators.Count == 0)
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
        if(canMoveForward == false || isJumping == true)
            return;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, speed, ref currentVelocity, speedUpAndSlowDownTime);
        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        UpdatePositionAndRotation();
    }

    private void MoveBackward(float speed)
    {
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
        if(canMoveForward == false)
        {
             anim.SetBool("IsMoving", false);
            return; 
        }
        
        anim.SetBool("IsMoving", isMoving);
            
        anim.SetBool("IsCrouching", isSneaking);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PathTrigger"))
        {
            // Use the trigger's unique identifier
            string triggerID = other.gameObject.GetInstanceID().ToString();

            // Increment the entry count
            if (!triggerEntryCounts.ContainsKey(triggerID))
            {
                triggerEntryCounts[triggerID] = 1;
            }
            else
            {
                triggerEntryCounts[triggerID]++;
            }

            // Check if the entry count is odd
            if (triggerEntryCounts[triggerID] % 2 == 1)
            {
                canSwitchPath = true;
                isAtPathChoice = true;
                currentSpeed = 0; // Stop the player
            }
            else
            {
                // Even entry count; do not activate path choice
                canSwitchPath = false;
                isAtPathChoice = false;
                canMove = true;
            }
        }
        if(other.CompareTag("Final Door") || other.CompareTag("Puzzle"))
        {
            canMoveForward = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PathTrigger"))
        {
            // You can remove this if not needed
            // canSwitchPath = false;
            // isAtPathChoice = false;
        }
        if(other.CompareTag("Puzzle") || other.CompareTag("Final Door"))
        {
            canMoveForward = true;
        }
    }
}
