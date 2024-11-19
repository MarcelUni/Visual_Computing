using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GesturePuzzle : MonoBehaviour
{
    public GameObject[] buttons; // Assign button objects in the correct order
    public GameObject leftDoor;
    public GameObject rightDoor;

    public List<int> correctSequence = new List<int>(); // The correct order of button presses
    private List<int> currentSequence = new List<int>(); // Tracks the player's inputs

    public float buttonPushDistance = 0.2f; // Distance to move button on the Y-axis when pushed
    public float buttonPushSpeed = 2f;     // Speed of button movement
    public float resetDelay = 1f;          // Delay before resetting buttons on wrong input

    private bool puzzleComplete = false;
    private bool isResetting = false; // Prevents input during reset
    private bool inPuzzleRange = false; // Tracks if the player is inside the trigger
    private Vector3[] originalPositions; // Stores original positions of buttons

    void Start()
    {
        // Store the original positions of the buttons
        originalPositions = new Vector3[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            originalPositions[i] = buttons[i].transform.localPosition;
        }
    }

    void Update()
    {
        // Only detect hand signs if the player is in range and not resetting
        if (inPuzzleRange && !puzzleComplete && !isResetting)
        {
            DetectHandSigns();
        }
    }

    private void DetectHandSigns()
    {
        // Replace with actual hand sign detection logic
        if (Input.GetKeyDown(KeyCode.B))
        {
            PushButton(0); // Simulate hand sign 1
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            PushButton(1); // Simulate hand sign 2
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            PushButton(2); // Simulate hand sign 3
        }
    }

    private void PushButton(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= buttons.Length)
            return;

        // Animate button push
        StartCoroutine(MoveButton(buttons[buttonIndex], originalPositions[buttonIndex] + new Vector3(buttonPushDistance, 0, 0)));

        // Check if this button is the correct one in the sequence
        currentSequence.Add(buttonIndex);

        if (currentSequence[currentSequence.Count - 1] != correctSequence[currentSequence.Count - 1])
        {
            // Incorrect sequence, reset buttons
            StartCoroutine(ResetButtons());
        }
        else if (currentSequence.Count == correctSequence.Count)
        {
            // Correct sequence completed
            puzzleComplete = true;
            StartCoroutine(OpenDoor());
        }
    }

    private IEnumerator MoveButton(GameObject button, Vector3 targetPosition)
    {
        Vector3 startPosition = button.transform.localPosition;
        float elapsedTime = 0;

        while (elapsedTime < 1 / buttonPushSpeed)
        {
            button.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime * buttonPushSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        button.transform.localPosition = targetPosition;
    }

    private IEnumerator ResetButtons()
    {
        isResetting = true; // Prevent further input

        yield return new WaitForSeconds(resetDelay);

        currentSequence.Clear();

        // Reset all buttons to their original positions
        for (int i = 0; i < buttons.Length; i++)
        {
            StartCoroutine(MoveButton(buttons[i], originalPositions[i]));
        }

        isResetting = false; // Allow input again
    }

    private IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(buttonPushDistance);
        // Move left and right doors to open
        StartCoroutine(MoveDoorSmoothly(leftDoor, new Vector3(0, 0, -2)));
        StartCoroutine(MoveDoorSmoothly(rightDoor, new Vector3(0, 0, 2)));
        yield return null;
    }

    private IEnumerator MoveDoorSmoothly(GameObject door, Vector3 offset)
    {
        Vector3 startPosition = door.transform.position;
        Vector3 targetPosition = startPosition + offset;
        float duration = 1.0f;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            door.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        door.transform.position = targetPosition;
    }

    // Trigger logic
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inPuzzleRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inPuzzleRange = false;
        }
    }
}
