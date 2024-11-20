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
       

        // Disable path movement and player input during the sequence
        pc.canMove = false;
        pc.isJumping = true;

        foreach (Transform target in stonePositions)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = target.position;

            // Phase 1: Rotate toward the target
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(endPos.x - transform.position.x, 0, endPos.z - transform.position.z));
            float rotationTime = 0.5f; // Hvor lang tid det tager at rotere mod stenen
            float elapsedRotationTime = 0f;

            while (elapsedRotationTime < rotationTime)
            {
                pc.playerModelObject.transform.rotation = Quaternion.Slerp(pc.playerModelObject.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                elapsedRotationTime += Time.deltaTime;
                yield return null;
            }

            // Phase 2: Wait for a delay before jumping
            float delayBeforeJump = 0.5f; // Adjust this for the delay duration
            // Trigger the Jump Up animation when starting the jump
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                // animator.SetTrigger("JumpUp"); Vi kan have en sådan animation
                // hvor Lumi tager lidt tilløb og hopper vi kan justere delayBeforeJump til timingen af animationen
            }
            yield return new WaitForSeconds(delayBeforeJump);

            // Phase 3: Perform the jump
            float elapsedTime = 0f;
            while (elapsedTime < jumpDuration)
            {
                
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
                // animator.SetTrigger("Land");
            }

            // Wait between jumps
            float timer = 0f;
            while (timer < jumpTimer)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (forwardJump == true)
            {
                // Wait for the player to move forward again before continuing
                while (!pc.moveForward)
                {
                    yield return null;
                }
            }
            else
            {
                // Wait for the player to move backwards again before continuing
                while (!pc.moveBackward)
                {
                    yield return null;
                }
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
