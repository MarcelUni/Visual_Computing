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
    [SerializeField] private float rotateSpeed;
    [SerializeField] private GameObject playerModelObject;
    public bool moveForward;
    public bool moveBackward;

    [Header("Behavior bools")]
    public bool isSneaking;
    public bool isMoving;
    public bool isDead;

    private float currentVelocity;
    private float currentSpeed = 0;
    [HideInInspector] public float distanceTravelled = 8;
    private Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();        
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
        Rotate();
    }

    private void MoveBackward(float speed)
    {   
        currentSpeed = Mathf.SmoothDamp(currentSpeed, -speed, ref currentVelocity , speedUpAndSlowDownTime);

        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled);

        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));

        Rotate();
    }

    private void Decelerate()
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, 0, ref currentVelocity , speedUpAndSlowDownTime);
            
        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled);

        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));
        
    }

    private void Rotate()   
    {
        Vector3 pathDirection = pathCreator.path.GetDirectionAtDistance(distanceTravelled);

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
}
