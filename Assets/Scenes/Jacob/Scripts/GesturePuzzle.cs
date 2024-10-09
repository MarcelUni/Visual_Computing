using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GesturePuzzle : MonoBehaviour
{
    // der skal lige gøres sådan at cameraShake ikke bliver spillet hele tiden (skal stoppe når dørenen har åbnet helt)
    public GameObject leftDoor;
    public GameObject rightDoor;

    private bool inPuzzleRange = true;

    public bool input1Activated = false; 
    public bool input2Activated = false; 
    public bool input3Activated = false;

    private bool start = true;

    private GameObject player;

    private void Update()
    {
        if (inPuzzleRange == true)
        {
            Interact();
        }

        if (inPuzzleRange == false)
        {
            input1Activated = false;
            input2Activated = false;
            input3Activated = false;
            Interact();
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inPuzzleRange = true; 
            player = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inPuzzleRange = false;
        }
    }

    public void Interact()
    {
        Debug.Log("Interacting with puzzle");
        // Get player inputs
        bool input1 = GetPlayerInput1();
        bool input2 = GetPlayerInput2();
        bool input3 = GetPlayerInput3();

        // Perform action based on inputs
        if (input1 && input2 && input3)
        {
            // All inputs are true
            StartCoroutine(MoveDoorSmoothly(true));
            StartCoroutine(MoveDoorSmoothly(false));

            StartCoroutine(CameraShake());
            
            
        }
        else if (input1 || input2 || input3)
        {
            // At least one input is true
        }
        else
        {
            // No inputs are true
        }
    }
   IEnumerator CameraShake()
    {
        float shakeDuration = 1f;
        Vector3 startPostion = player.transform.GetChild(1).transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            player.transform.GetChild(1).transform.position = startPostion + Random.insideUnitSphere;

            yield return null;
        }

        player.transform.GetChild(1).transform.position = startPostion;


    }
    private IEnumerator MoveDoorSmoothly(bool leftdoor)
    {
        if (leftdoor)
        {
            Vector3 targetPosition = leftDoor.transform.position + new Vector3(0, 0, 2);
            float duration = 1.0f;
            float elapsedTime = 0.0f;
            Vector3 startingPosition = leftDoor.transform.position;

            while (elapsedTime < duration)
            {
                leftDoor.transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            leftDoor.transform.position = targetPosition;
        }
        else
        {
            Vector3 targetPosition = rightDoor.transform.position + new Vector3(0, 0, -2);
            float duration = 1.0f;
            float elapsedTime = 0.0f;
            Vector3 startingPosition = rightDoor.transform.position;

            while (elapsedTime < duration)
            {
                rightDoor.transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            rightDoor.transform.position = targetPosition;
        }
    }

    private bool GetPlayerInput1()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            input1Activated = true; // Set input1Activated to true when B is pressed
        }

        return input1Activated; // Return true if B has been pressed
    }

    private bool GetPlayerInput2()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            input2Activated = true; // Set input2Activated to true when N is pressed
        }

        return input2Activated; // Return true if N has been pressed
    }

    private bool GetPlayerInput3()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            input3Activated = true; // Set input3Activated to true when M is pressed
        }

        return input3Activated; // Return true if M has been pressed
    }
}
