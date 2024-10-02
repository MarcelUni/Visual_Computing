using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PathCreator pathCreator;
    [SerializeField] private float normalMoveSpeed;
    [SerializeField] private float sneakMoveSpeed;
    [SerializeField] private float speedUpAndSlowDownTime;
    public bool moveForward;
    public bool moveBackward;


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
    private float distanceTravelled;
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
        }
        else if (Input.GetKey(backwardKey))
        {
            moveForward = false;
            moveBackward = true;
        }
        else
        {
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
    }

    private void MoveBackward(float speed)
    {   
        currentSpeed = Mathf.SmoothDamp(currentSpeed, -speed, ref currentVelocity , speedUpAndSlowDownTime);

        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled);

        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));
    }

    private void Decelerate()
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, 0, ref currentVelocity , speedUpAndSlowDownTime);
            
        distanceTravelled += currentSpeed * Time.fixedDeltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled);

        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));
        
    }
}
