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
    private CharacterController characterController;
    private PlayerController pc;
    private Animator animator;
    private Rigidbody rb; // Reference to the Rigidbody

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        pc = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AutoJumpTrigger") && !isAutoJumping)
        {
            var stoneListPopulate = other.GetComponent<JumpLocationPopulator>(); 
            stonePositions = stoneListPopulate.stonePositions; // Populate the stonePositions array from the JumpLocationPopulator script

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
        // Trigger the Jump Up animation when starting the jump
        if (animator != null)
        {
            // animator.SetTrigger("JumpUp");
        }

        // Disable path movement and player input during the sequence
        pc.canMove = false;
        pc.isJumping = true;

        
        foreach (Transform target in stonePositions)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = target.position;
            float elapsedTime = 0f;

            

            // Remove the player from the path system during the jump
            pc.enabled = false;

            // Perform the jump movement
            while (elapsedTime < jumpDuration)
            {
                // Calculate the direction towards the target stone (only on the XZ plane)
                Vector3 directionToTarget = new Vector3(endPos.x - transform.position.x, 0, endPos.z - transform.position.z);

                // Calculate the target Y-axis rotation
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                // Smoothly rotate the playerModelObject only on the Y-axis
                pc.playerModelObject.transform.rotation = Quaternion.Slerp(pc.playerModelObject.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Calculate jump height using parabolic arc
                float t = elapsedTime / jumpDuration;
                float height = 4 * jumpHeight * t * (1 - t);
                transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the player reaches the target position
            transform.position = endPos;

            // Trigger the Land animation when the player reaches the stone
            if (animator != null)
            {
                // animator.SetTrigger("Land"); // We can set up 2 animations one for the inital jump and one for landing, this way we can make it more realistic ín game maybe 
            }

            // Wait between jumps
            float timer = 0f;
            while (timer < jumpTimer)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // Wait for the player to move forward again before continuing
            while (!pc.moveForward)
            {
                yield return null;
            }
        }

        // Re-enable path movement after the jump sequence is completed
        pc.enabled = true;
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
