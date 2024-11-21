using UnityEngine;
using System.Collections;

public class AutoJumpController : MonoBehaviour
{
    private Transform[] stonePositions; // Gets assigned from the JumpLocationPopulator script that is attached to the AutoJumpTrigger object
    public float jumpHeight = 2f;
    public float jumpDuration = 0.5f;
    public float jumpTimer = 0.5f;
    public float rotationSpeed = 5f; // Speed of rotation
    private bool isAutoJumping = false;
    private bool forwardJump = true; // True = forward, False = backwards

    private CharacterController characterController;
    private PlayerController pc;
    [SerializeField] private Animator animator;
    private Rigidbody rb; // Reference to the Rigidbody

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        pc = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AutoJumpTrigger") && !isAutoJumping)
        {
            var stoneListPopulate = other.GetComponent<JumpLocationPopulator>(); 
            stonePositions = stoneListPopulate.stonePositions; // Populate the stonePositions array from the JumpLocationPopulator script
            forwardJump = stoneListPopulate.forwardJump; // Get the forwardJump bool from the JumpLocationPopulator script

                                                                // Disable all AutoJumpTrigger colliders to prevent multiple jumps
            GameObject[] autoJumpTriggers = GameObject.FindGameObjectsWithTag("AutoJumpTrigger");
            foreach (GameObject trigger in autoJumpTriggers)
            {
                Collider triggerCollider = trigger.GetComponent<Collider>();
                if (triggerCollider != null)
                {
                    triggerCollider.enabled = false;
                }
            }
            StartCoroutine(AutoJumpTriggerDisabled());
            StartCoroutine(AutoJumpSequence());
        }
    }

    IEnumerator AutoJumpTriggerDisabled()
    {
        yield return new WaitForSeconds(5f);
        // Disable all AutoJumpTrigger colliders to prevent multiple jumps
        GameObject[] autoJumpTriggers = GameObject.FindGameObjectsWithTag("AutoJumpTrigger");
        foreach (GameObject trigger in autoJumpTriggers)
        {
            Collider triggerCollider = trigger.GetComponent<Collider>();
            if (triggerCollider != null)
            {
                triggerCollider.enabled = true;
            }
        }
    }

    IEnumerator AutoJumpSequence()
    {
        isAutoJumping = true;

        // Freeze Rigidbody constraints during the jump
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }

        pc.moveForward = false;
        // Disable player input by disabling the PlayerController script
        pc.enabled = false;

        foreach (Transform target in stonePositions)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = target.position;

            // Phase 1: Rotate toward the target
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(endPos.x - transform.position.x, 0, endPos.z - transform.position.z));
            float rotationTime = 0.5f;
            float elapsedRotationTime = 0f;

            while (elapsedRotationTime < rotationTime)
            {
                pc.playerModelObject.transform.rotation = Quaternion.Slerp(pc.playerModelObject.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                elapsedRotationTime += Time.deltaTime;
                yield return null;
            }

            // Phase 2: Wait for a delay before jumping
            float delayBeforeJump = 0.5f;

            // Trigger the Jump Up animation
            if (animator != null)
            {
                animator.SetTrigger("JumpUp");
            }

            // Wait for the JumpUp animation to complete
            float jumpUpDuration = .5f; // Set this to the length of the JumpUp animation
            yield return new WaitForSeconds(jumpUpDuration);

            // Phase 3: Perform the jump
            float elapsedTime = 0f;
            bool landTriggered = false; // Ensure Land animation is triggered only once
            while (elapsedTime < jumpDuration)
            {
            

                // Trigger the Land animation slightly before landing
                if (animator != null && elapsedTime >= jumpDuration * 0.50f && !landTriggered)
                {
                    animator.SetTrigger("Land");
                    landTriggered = true;
                }

                // Calculate jump height using parabolic arc
                float t = elapsedTime / jumpDuration;
                float height = 4 * jumpHeight * t * (1 - t);
                transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the player reaches the target position
            transform.position = endPos;

            // Wait briefly after landing
            float postLandingWait = 0.2f; // Adjust as needed for the landing animation duration
            yield return new WaitForSeconds(postLandingWait);
        }

        // Re-enable player input by enabling the PlayerController script
        pc.enabled = true;

        // Re-enable path movement after the jump sequence is completed
        pc.isJumping = false;

        // Update the player's position on the path
        pc.distanceTravelled = pc.pathCreators[pc.currentPathIndex].path.GetClosestDistanceAlongPath(transform.position);

        // Unfreeze Rigidbody constraints after the jump
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Allow movement again, but keep rotation frozen
        }

        pc.canMove = true;
        isAutoJumping = false;
    }

}
