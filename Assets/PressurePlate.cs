using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public List<GameObject> targetObjects;  // List of target objects with animations
    public float openAnimationDelay = 0.5f; // Delay in seconds before playing the open animation
    public string playerTag = "Player";     // Tag for the player object
    public string keyTag = "Box";           // Tag for objects that can also trigger the animations

    private Dictionary<GameObject, (string closeAnimationName, string openAnimationName)> animationClips
        = new Dictionary<GameObject, (string, string)>();
    private int triggerCount = 0; // Counter to track the number of triggers entered

    private void Start()
    {
        foreach (GameObject targetObject in targetObjects)
        {
            Animation anim = targetObject.GetComponent<Animation>();
            if (anim != null && anim.GetClipCount() >= 2)
            {
                var enumerator = anim.GetEnumerator();
                enumerator.MoveNext(); // Move to the first clip
                string closeAnimationName = ((AnimationState)enumerator.Current).name;

                enumerator.MoveNext(); // Move to the second clip
                string openAnimationName = ((AnimationState)enumerator.Current).name;

                // Store animation clip names in the dictionary
                animationClips[targetObject] = (closeAnimationName, openAnimationName);
            }
            else
            {
                Debug.LogWarning($"Target Object {targetObject.name} does not have at least two animation clips.");
            }
        }
    }

    private void MovePlateDown()
    {
        Vector3 loweredPosition = transform.position + new Vector3(0, -0.2f, 0);
        transform.position = loweredPosition;
    }

    private void MovePlateUp()
    {
        Vector3 originalPosition = transform.position + new Vector3(0, 0.2f, 0);
        transform.position = originalPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            triggerCount++;
            if (triggerCount == 1) // Only play animations on the first object that enters
            {
                foreach (var kvp in animationClips)
                {
                    GameObject targetObject = kvp.Key;
                    (string closeAnimationName, string _) = kvp.Value;

                    Animation anim = targetObject.GetComponent<Animation>();
                    if (anim != null && !string.IsNullOrEmpty(closeAnimationName))
                    {
                        anim.Play(closeAnimationName);
                    }
                }
                MovePlateDown();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(keyTag))
        {
                foreach (var kvp in animationClips)
                {
                    GameObject targetObject = kvp.Key;
                    (string closeAnimationName, string _) = kvp.Value;

                    Animation anim = targetObject.GetComponent<Animation>();
                    if (anim != null && !string.IsNullOrEmpty(closeAnimationName))
                    {
                        anim.Play(closeAnimationName);
                    }
                }
                MovePlateDown();
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) || other.CompareTag(keyTag))
        {
            triggerCount--;
            if (triggerCount == 0) // Only play animations when all objects have left
            {
                StartCoroutine(PlayOpenAnimationsWithDelay());
            }
        }
    }

    private IEnumerator PlayOpenAnimationsWithDelay()
    {
        yield return new WaitForSeconds(openAnimationDelay); // Wait for the specified delay

        foreach (var kvp in animationClips)
        {
            GameObject targetObject = kvp.Key;
            (_, string openAnimationName) = kvp.Value;

            Animation anim = targetObject.GetComponent<Animation>();
            if (anim != null && !string.IsNullOrEmpty(openAnimationName))
            {
                anim.Play(openAnimationName);
            }
        }
        MovePlateUp();
    }
}
