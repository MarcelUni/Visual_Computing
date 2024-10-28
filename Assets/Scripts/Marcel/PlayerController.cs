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

    void Start()
    {
        canSwitchPath = false;
        canMove = true;
        isAtPathChoice = false;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
       
        if (canMove)
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
    public IEnumerator SmoothSwitchPath(int pathIndex)
    {
        isTransitioning = true;

        if (pathIndex == currentPathIndex)
                yield return null;

        currentPathIndex = pathIndex; // Switch to the specified path
        camFollowPath.currentPathIndex = pathIndex; // Switch camera path
        

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
        if (other.CompareTag("PathTrigger") && !moveBackward)
        {
                canSwitchPath = true;
                isAtPathChoice = true;
        }
        else
        {
            return;
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
            isAtPathChoice = false;
        }
        if(other.CompareTag("Puzzle") || other.CompareTag("Final Door"))
        {
            canMoveForward = true;
        }
    }
}
