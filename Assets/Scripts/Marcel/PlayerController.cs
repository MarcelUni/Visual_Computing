using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;


public class PlayerController : MonoBehaviour
{
    [Header("Move parameters")]
    public PathCreator pathCreator;
    [SerializeField] private float normalMoveSpeed;
    [SerializeField] private float sneakMoveSpeed;
    [SerializeField] private float speedUpAndSlowDownTime;
    public bool moveForward;
    public bool moveBackward;
    [SerializeField] private GameObject playerModelObject;


    [Header("Test inputs")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode sneakKey = KeyCode.LeftShift;

    [Header("Behavior bools")]
    public bool isSneaking;
    public bool isMoving;
    public bool isDead;

    private float currentVelocity;
    private float currentSpeed = 0;
    [HideInInspector] public float distanceTravelled;
    private Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();        
    }

    private void Update()
    {
        if (Input.GetKey(forwardKey))
        {
            moveForward = true;
            moveBackward = false;
            isMoving = true;
        }
        else if (Input.GetKey(backwardKey))
        {
            moveForward = false;
            moveBackward = true;
            isMoving = true;
        }
        else
        {
            isMoving = false;
            moveBackward = false;
            moveForward = false;
        }

        if(Input.GetKeyDown(sneakKey))
        {
            ToggleSneak();
        }
    }

    public void ToggleSneak()
    {
        isSneaking = !isSneaking;
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
        currentSpeed = Mathf.SmoothDamp(currentSpeed, speed, ref currentVelocity , speedUpAndSlowDownTime);

        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled);
 
        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));  
        SmoothRotate();
    }

    private void MoveBackward(float speed)
    {   
        currentSpeed = Mathf.SmoothDamp(currentSpeed, -speed, ref currentVelocity , speedUpAndSlowDownTime);

        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled);

        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));

        SmoothRotate();
    }

    private void Decelerate()
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, 0, ref currentVelocity , speedUpAndSlowDownTime);
            
        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled);

        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));
        
    }

    private void SmoothRotate()
    {
        Vector3 pathDirection = pathCreator.path.GetDirectionAtDistance(distanceTravelled);

        Quaternion pathRotation;
        Vector3 newDirection;

        if(currentSpeed > 0)
        {
            // Create a new direction vector that only considers the y-axis rotation
            newDirection = new Vector3(pathDirection.x, 0, pathDirection.z);
            pathRotation = Quaternion.LookRotation(newDirection);
        }
        else
        {
            // Create a new direction vector that only considers the y-axis rotation
            newDirection = new Vector3(-pathDirection.x, 0, -pathDirection.z);
            pathRotation = Quaternion.LookRotation(newDirection);
        }
            
        playerModelObject.transform.rotation = pathRotation;
    }
}
