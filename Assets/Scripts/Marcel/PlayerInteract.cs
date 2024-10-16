using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    // public variables
    public bool DoorInRange;
    public float pickUpRadius;
    public LayerMask interactLayer;
    public string keyTag, LightorbTag;
    public bool hasKey, hasLightOrb;
    public GameObject lightorbPosition;

    // Private variables
    private GameObject lightOrbObject;
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
            InteractWithObject();
            anim.SetTrigger("Interact");
        }

        if(hasKey == false || hasLightOrb == false)
            PickupObject();
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

    private void InteractWithObject()
    {
        puzzleObject.GetComponent<IInteractable>()?.Interact();
        puzzleObject.GetComponent<Collider>().enabled = false;
        pc.canMoveForward = true;

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Puzzle"))
        {
            DoorInRange = true;
            puzzleObject = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Puzzle"))
        {
            DoorInRange = false;;
            puzzleObject = null;
        }
    }
}
