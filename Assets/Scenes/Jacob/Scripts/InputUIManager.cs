using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputUIManager : MonoBehaviour
{
    [System.Serializable]
    public class InputUIElement
    {
        public string inputName; // The method name that needs to be called (e.g., "MoveForward", "MoveBackward")
        public Image uiImage;    // The corresponding UI Image
        public TMP_Text instructionText; // The instruction to display
    }

    public List<InputUIElement> inputUIElements; // List of UI elements for each method
    public float activationDuration = 2f; // Duration required to validate an input
    public float initialAlpha = 0.3f; // Starting alpha for all UI images

    private int currentIndex = 0; // Tracks the current active input UI element
    private float activeTime = 0f; // Tracks how long the current method has been called
    private Dictionary<string, InputUIElement> inputUIDictionary; // For quick lookups

    private void Start()
    {
        // Build a dictionary and initialize UI elements
        inputUIDictionary = new Dictionary<string, InputUIElement>();
        foreach (var element in inputUIElements)
        {
            inputUIDictionary[element.inputName] = element;
            SetImageAlpha(element.uiImage, initialAlpha); // Set partial visibility
            element.uiImage.gameObject.SetActive(false); // Hide all elements initially
            element.instructionText.enabled = false;    // Hide all instructions
        }

        // Activate the first element in the sequence
        ActivateElement(currentIndex);
    }

    public void NotifyInput(string inputName)
    {
        // If the input doesn't match the current UI element, do nothing
        if (inputName != inputUIElements[currentIndex].inputName)
            return;

        // Progress timer
        activeTime += Time.deltaTime;

        // Fade in the UI image
        var element = inputUIElements[currentIndex];
        float alpha = Mathf.Clamp01(initialAlpha + (activeTime / activationDuration) * (1f - initialAlpha));
        SetImageAlpha(element.uiImage, alpha);

        // If the input has been called long enough, proceed to the next UI element
        if (activeTime >= activationDuration)
        {
            // Disable current element and move to the next
            element.uiImage.gameObject.SetActive(false);
            element.instructionText.enabled = false;

            currentIndex++;
            activeTime = 0f;

            // Activate the next element if available
            if (currentIndex < inputUIElements.Count)
            {
                ActivateElement(currentIndex);
            }
            else 
            {
                GameManager.instance.ChangeScene();
            }
        }
    }

    private void ActivateElement(int index)
    {
        var element = inputUIElements[index];
        element.uiImage.gameObject.SetActive(true);
        element.instructionText.enabled = true;
        element.instructionText.text = $"Make {element.inputName} handsign"; // Set correct instruction
        SetImageAlpha(element.uiImage, initialAlpha); // Reset alpha
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null) return;
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
