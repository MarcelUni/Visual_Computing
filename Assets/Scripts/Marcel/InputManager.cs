using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private KeyCode currentKey;

    private PlayerInteract playerInteract;
    private PlayerController pc;
    private PickupObjects pickupObjects;

    void Start()
    {
        pc = GetComponent<PlayerController>();
        playerInteract = GetComponent<PlayerInteract>();   
        pickupObjects = GetComponent<PickupObjects>();
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

            // Check for input to switch path or continue
            if (Input.GetKeyDown(switchPathIndex1Key))
            {
                StartCoroutine(pc.SmoothSwitchPath(1)); // Switch to the next path smoothly
                pc.isAtPathChoice = false; // Player has made a decision
            }
            else if (Input.GetKeyDown(switchPathIndex0Key))
            {
                StartCoroutine(pc.SmoothSwitchPath(0)); // Switch to the next path smoothly
                pc.isAtPathChoice = false; // Player chooses to continue on the current path
            }
        }
        
        else if (Input.GetKey(moveForwardKey))
         {
             MoveForward();
         }
         else if (Input.GetKey(moveBackwardKey))
         {
             MoveBackward();
         }
         else if (Input.GetKey(sneakForwardKey))
         {
             ForwardSneak();
         }
         else if (Input.GetKey(sneakBackwardKey))
         {
             BackwardSneak();
         }
        
        else
        {
            NoInput();
        }

         if(Input.GetKeyDown(interactKey))
         {
             playerInteract.Interact();
         }
    }

    public void ReceiveInput(string inputString)
    {
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
        if (pc.isAtPathChoice)
            return;

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
        pc.moveForward = false;
        pc.moveBackward = true;
        pc.isMoving = true;
        pc.isSneaking = false;
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
        pc.moveForward = true;
        pc.moveBackward = false;
        pc.isMoving = true;
        pc.isSneaking = true;
    }

    /// <summary>
    /// Moving backwards while sneaking
    /// </summary>
    public void BackwardSneak()
    {
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
        playerInteract.Interact();
        // pickupObjects.PickupAndDrop();
    }
}
