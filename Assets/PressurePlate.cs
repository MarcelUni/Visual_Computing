using System.Collections;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Transform[] rocks;  // Assign rock transforms in Inspector
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player stepped on the plate
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            PlayRockAnimations(0);  // Play the first animation (close path)
            MovePlateDown();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player stepped off the plate
        if (other.CompareTag("Player") && isActivated)
        {
            isActivated = false;
            PlayRockAnimations(1);  // Play the second animation (open path)
            MovePlateUp();
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

    private void PlayRockAnimations(int animationIndex)
    {
        // Trigger the specified animation clip by index (0 for close, 1 for open) for each rock
        foreach (Transform rock in rocks)
        {
            Animation rockAnimation = rock.GetComponent<Animation>();
            if (rockAnimation != null)
            {
                int currentIndex = 0;
                foreach (AnimationState animState in rockAnimation)
                {
                    if (currentIndex == animationIndex)
                    {
                        rockAnimation.Play(animState.name);  // Play the specific animation by its name
                        break;
                    }
                    currentIndex++;
                }
            }
        }
    }
}
