using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    // public variables
    public bool puzzleInRange;
    public float pickUpRadius;
    public LayerMask interactLayer;
    public string keyTag, LightorbTag;
    public bool hasKey, hasLightOrb;
    public GameObject lightorbPosition;

    // Private variables
    private GameObject lightOrbObject;
    private GameObject puzzleObject;
    private Animator anim;
    
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void Interact()
    {
        if(puzzleInRange == true)
        {
            InteractWithObject();
            anim.SetTrigger("Interact");
        }

        PickupObject();

        if(hasLightOrb)
        {
            lightOrbObject.transform.SetParent(this.transform);
            lightOrbObject.transform.SetPositionAndRotation(lightorbPosition.transform.position, lightorbPosition.transform.rotation);
            lightOrbObject.GetComponent<LightOrbBehavior>().InitializeOrb();
            lightOrbObject.GetComponent<LightOrbBehavior>().hasLightOrb = true;
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
            }
            if(hit.collider.CompareTag(LightorbTag))
            {
                hasLightOrb = true;
                lightOrbObject = hit.transform.gameObject;
            }
        }
    }

    private void InteractWithObject()
    {
        puzzleObject.GetComponent<IInteractable>()?.Interact();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Puzzle"))
        {
            puzzleInRange = true;
            puzzleObject = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Puzzle"))
        {
            puzzleInRange = false;;
            puzzleObject = null;
        }
    }
}
