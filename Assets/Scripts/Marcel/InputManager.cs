using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    public bool useKeys;

    // Keycodes
    public KeyCode moveForwardKey;
    public KeyCode moveBackwardKey;
    public KeyCode sneakForwardKey;
    public KeyCode sneakBackwardKey;
    public KeyCode interactKey;
    public KeyCode switchPathIndex1Key;
    public KeyCode switchPathIndex0Key;

    private PlayerInteract playerInteract;
    private PlayerController pc;
    private InputUIManager inputUIManager;
    public string inputPerformedString;
    public UnityEvent<int> PathChosenEvent;

    void Start()
    {
        pc = GetComponent<PlayerController>();
        playerInteract = GetComponent<PlayerInteract>();  
        inputUIManager = FindFirstObjectByType<InputUIManager>();
    }

    private void ChoosePath(int index)
    {
        pc.isAtPathChoice = false; // Player has made a decision
        StartCoroutine(pc.SmoothSwitchPath(index)); // Switch to the next path smoothly
        PathChosenEvent.Invoke(index);
    }

    // Update is called once per frame
    void Update()
    {
        if(useKeys == false)
            return;

        if (pc.isAtPathChoice)
        {
            // Disable movement during path choice
            pc.canMove = false;
        }
        
        if (Input.GetKey(moveForwardKey))
        {
            MoveForward();
            inputPerformedString = "Forward";
        }
        else if (Input.GetKey(moveBackwardKey))
        {
           MoveBackward();
           inputPerformedString = "Backward";
        }
        else if (Input.GetKey(sneakForwardKey))
        {
            ForwardSneak();
            inputPerformedString = "ForwardSneak";
        }
        else if (Input.GetKey(sneakBackwardKey))
        {
            inputPerformedString = "BackwardSneak";
            BackwardSneak();
        }
        else
        {
            inputPerformedString = "Stop";
            NoInput();
        }

        if(Input.GetKey(interactKey))
        {
            inputPerformedString = "Interact";
           Interact();
        }
    }

    public void ReceiveInput(string inputString)
    {
        if(useKeys == true)
        {
            return;
        }
        
        inputPerformedString = inputString;
        switch (inputString)
        {
            case "Forward":
                MoveForward();
                break;
            case "Backward":
                MoveBackward();
                break;
            case "ForwardSneak":
                ForwardSneak();
                break;
            case "BackwardSneak":
                BackwardSneak();
                break;
            case "Interact":
                Interact();
                break;
            case "Stop":
                NoInput();
                break;
            default:
                NoInput();
                break;
        }
    }

    /// <summary>
    /// Has to be called for moving forward gesture
    /// </summary>
    public void MoveForward()
    {
        if(inputUIManager != null)
        {
            inputUIManager.NotifyInput("Forward");
        }
        pc.moveForward = true;
        pc.moveBackward = false;
        pc.isMoving = true;
        pc.isSneaking = false;
    }

    /// <summary>
    /// Move backward gesture
    /// </summary>
    public void MoveBackward()
    {
        if(inputUIManager != null)
        {
            inputUIManager.NotifyInput("Backward");
        }
        else
        {
            pc.moveForward = false;
            pc.moveBackward = true;
            pc.isMoving = true;
            pc.isSneaking = false;
        }
    }

    /// <summary>
    /// When no gesture is true
    /// </summary>
    public void NoInput()
    {
        pc.isMoving = false;
        pc.moveBackward = false;
        pc.moveForward = false;
    }

    /// <summary>
    /// Moving forward while sneaking
    /// </summary>
    public void ForwardSneak()
    {
        if(inputUIManager != null)
        {
            inputUIManager.NotifyInput("ForwardSneak");
        }
        if (pc.isAtPathChoice)
        {
            ChoosePath(0);
        }
        else
        {
            pc.moveForward = true;
            pc.moveBackward = false;
            pc.isMoving = true;
            pc.isSneaking = true;
        }
    }

    /// <summary>
    /// Moving backwards while sneaking
    /// </summary>
    public void BackwardSneak()
    {
        if (inputUIManager != null)
        {
            inputUIManager.NotifyInput("BackwardSneak");
        }
        pc.moveForward = false;
        pc.moveBackward = true;
        pc.isMoving = true;
        pc.isSneaking = true;   
    }

    /// <summary>
    /// Call once for interacting with a puzzle
    /// </summary>
    public void Interact()
    {
        if (inputUIManager != null)
        {
            inputUIManager.NotifyInput("Interact");
        }
        if (pc.isAtPathChoice)
        {
            ChoosePath(1);
        }
        else
        {
            pc.moveForward = false;
            playerInteract.Interact();
            // pickupObjects.PickupAndDrop();
        }
    }
}
