 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;


public class PlayerController : MonoBehaviour
{
    [Header("Move parameters")]
    public List<PathCreator> pathCreators;
    private int currentPathIndex = 0;
    [SerializeField] private float normalMoveSpeed;
    [SerializeField] private float sneakMoveSpeed;
    [SerializeField] private float speedUpAndSlowDownTime;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private GameObject playerModelObject;
    public bool moveForward;
    public bool moveBackward;
    private bool canMove;

    [Header("Behavior bools")]
    public bool isSneaking;
    public bool isMoving;
    public bool isDead;
    public bool canSwitchPath = false;

    private float currentVelocity;
    private float currentSpeed = 0;
    [HideInInspector] public float distanceTravelled = 8;
    private Rigidbody rb;
    public Animator anim;



    void Start()
    {
        canSwitchPath = false;
        canMove = true;
        rb = GetComponent<Rigidbody>();   
    }

    private void Update()
    {
        if(anim.GetCurrentAnimatorStateInfo(0).IsName("Interact") || anim.GetCurrentAnimatorStateInfo(0).IsName("InteractOut"))
            canMove = false;
        else
            canMove = true;

        //check for path switching input
        if (Input.GetKeyDown(KeyCode.V))
        {
            SwitchPath();
        }

        UpdateAnimations();   
    }

    public void ToggleSneak()
    {
        isSneaking = !isSneaking;
    }

    private void SwitchPath()
    {
       if(pathCreators.Count > 1 && canSwitchPath == true)
        {
            currentPathIndex = (currentPathIndex + 1) % pathCreators.Count; // Cycle to the next path
            distanceTravelled = 0; // Reset distance travelled

        }
    }

    private void FixedUpdate()
    {
        if (moveForward)
        {
            if(isSneaking)
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
            if(isSneaking)
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

    private void MoveForward(float speed)
    {
        if(canMove != true || pathCreators.Count == 0)
            return;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, speed, ref currentVelocity , speedUpAndSlowDownTime);

        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreators[currentPathIndex].path.GetPointAtDistance(distanceTravelled);
 
        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));  
        Rotate();
    }

    private void MoveBackward(float speed)
    {   
        if(canMove != true || pathCreators.Count == 0)
            return;
        
        currentSpeed = Mathf.SmoothDamp(currentSpeed, -speed, ref currentVelocity , speedUpAndSlowDownTime);

        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreators[currentPathIndex].path.GetPointAtDistance(distanceTravelled);

        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));

        Rotate();
    }

    private void Decelerate()
    {
        if (pathCreators.Count == 0) return;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, 0, ref currentVelocity , speedUpAndSlowDownTime);
            
        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreators[currentPathIndex].path.GetPointAtDistance(distanceTravelled);

        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));
        
    }

    private void Rotate()   
    {
        if (pathCreators.Count == 0) return;
        Vector3 pathDirection = pathCreators[currentPathIndex].path.GetDirectionAtDistance(distanceTravelled);

        Quaternion lookRotation;
        Vector3 newDirection;

        if(currentSpeed > 0)
        {
            // Create a new direction vector that only considers the y-axis rotation
            newDirection = new Vector3(pathDirection.x, 0, pathDirection.z);
            lookRotation = Quaternion.LookRotation(newDirection);
        }
        else
        {
            // Create a new direction vector that only considers the y-axis rotation
            newDirection = new Vector3(-pathDirection.x, 0, -pathDirection.z);
            lookRotation = Quaternion.LookRotation(newDirection);
        }   

        Quaternion currentRot = playerModelObject.transform.rotation;

        playerModelObject.transform.rotation = Quaternion.Slerp(currentRot, lookRotation, rotateSpeed * Time.deltaTime);
    }

    private void UpdateAnimations()
    {
        anim.SetBool("IsMoving", isMoving);
        anim.SetBool("IsCrouching", isSneaking);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Logic for when the player enters a path trigger collider
        if (other.CompareTag("PathTrigger"))
        {
            canSwitchPath = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Logic for when the player exits a path trigger collider
        if (other.CompareTag("PathTrigger"))
        {
            canSwitchPath = false;
        }
    }
}
