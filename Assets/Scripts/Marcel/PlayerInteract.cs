using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    // public variables
    public bool DoorInRange, puzzleInRange;
    public float pickUpRadius;
    public string keyTag, LightorbTag;
    public bool hasKey, hasLightOrb;
    public GameObject lightorbPosition;

    // Private variables
    private GameObject lightOrbObject;
    private GameObject finalDoorObject;
    private GameObject puzzleObject;
    private Animator anim;
    private PlayerController pc;
    
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        pc = GetComponent<PlayerController>();
    }

    public void Interact()
    {
        if(DoorInRange == true && hasLightOrb)
        {
            Debug.Log("dorr");
            InteractWithDoor();
            anim.SetTrigger("Interact");
        }

        if(puzzleInRange && hasKey)
        {
            Debug.Log("key");
            InteractWithPuzzle();
            anim.SetTrigger("Interact");
        }

        if(hasKey == false || hasLightOrb == false)
        {
            PickupObject();
            // Pick up animation
        }
    }

    private void PickupObject()
    {
        // Casting within a range to get objects we can pickup
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, pickUpRadius, transform.forward);

        foreach (RaycastHit hit in hits)
        {
            if(hit.collider.CompareTag(keyTag))
            {
                hasKey = true;

                // Find the Particle System by name and play it at the hit collider's position
                GameObject particleSystemObject = GameObject.Find("Particle System");
                if (particleSystemObject != null)
                {
                    ParticleSystem ps = particleSystemObject.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        ps.Play();
                    }
                }

                hit.collider.gameObject.SetActive(false);
               
            }
            if(hit.collider.CompareTag(LightorbTag))
            {
                // Doing stuff to make light orb work and follow player

                hasLightOrb = true;
                lightOrbObject = hit.transform.gameObject;
                lightOrbObject.tag = "Untagged";

                lightOrbObject.transform.SetParent(this.transform);
                lightOrbObject.transform.SetPositionAndRotation(lightorbPosition.transform.position, lightorbPosition.transform.rotation);
                lightOrbObject.GetComponent<LightOrbBehavior>().hasLightOrb = true;

                lightOrbObject.GetComponent<LightOrbBehavior>().InitializeOrb();
            }
        }
    }

    private void InteractWithDoor()
    {
        finalDoorObject.GetComponent<IInteractable>()?.Interact();
        pc.canMoveForward = true;

    }

    private void InteractWithPuzzle()
    {
        puzzleObject.GetComponent<Puzzle>().Interact();
        puzzleInRange = false;
        pc.canMoveForward = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Final Door"))
        {
            DoorInRange = true;
            finalDoorObject = other.gameObject;
        }
        if(other.CompareTag("Puzzle"))
        {
            puzzleInRange = true;
            puzzleObject = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Final Door"))
        {
            DoorInRange = false;;
            finalDoorObject = null;
        }
        if(other.CompareTag("Puzzle"))
        {
            puzzleInRange = false;
            puzzleObject = null;
        }
    }
}
