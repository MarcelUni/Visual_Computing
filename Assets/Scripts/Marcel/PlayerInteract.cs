using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    // public variables
    public bool DoorInRange, puzzleInRange, trapInRange;
    public float pickUpRadius;
    public string keyTag, LightorbTag;
    public bool hasKey, hasLightOrb;
    public GameObject lightorbPosition;
    public float lightOrbLerpSpeed;

    // Private variables
    private GameObject lightOrbObject;
    private GameObject doorLightOrb;
    private GameObject finalDoorObject;
    private GameObject puzzleObject, trapObject;
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
            InteractWithDoor();
            anim.SetTrigger("Interact");
        }

        if(puzzleInRange && hasKey)
        {
            InteractWithPuzzle();
            anim.SetTrigger("Interact");
        }

        if(trapInRange && hasKey)
        {
            InteractWithTrap();
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
                anim.SetTrigger("Interact");
                // Find the Particle System by name and play it at the hit collider's position
                GameObject particleSystemObject = GameObject.Find("PickUpParticle");
                if (particleSystemObject != null)
                {
                    ParticleSystem ps = particleSystemObject.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        AudioManager.instance.PlaySFX("PickupSound");
                        ps.Play();
                    }
                }

                hit.collider.gameObject.SetActive(false);
               
            }
            if(hit.collider.CompareTag(LightorbTag))
            {
                anim.SetTrigger("Interact");
                // Doing stuff to make light orb work and follow player
                AudioManager.instance.PlaySFX("PickupSound");

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
        StartCoroutine(MoveLightOrb());
    }

    private IEnumerator MoveLightOrb()
    {
        Debug.Log(lightOrbObject.name);
        Debug.Log(doorLightOrb.name);

        while(Vector3.Distance(lightOrbObject.transform.position, doorLightOrb.transform.position) < .5f)
        {
            lightOrbObject.transform.position = Vector3.Lerp(transform.position, doorLightOrb.transform.position, lightOrbLerpSpeed);
            Debug.Log("ban");
        }

        yield return null;
    }

    private void InteractWithPuzzle()   
    {
        puzzleObject.GetComponent<Puzzle>().Interact();
        puzzleInRange = false;
        pc.canMoveForward = true;
    }

    private void InteractWithTrap()
    {
        trapObject.GetComponent<Trap>().Interact();
        trapInRange = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Final Door"))
        {
            DoorInRange = true;
            finalDoorObject = other.gameObject;
            doorLightOrb = finalDoorObject.GetComponentInChildren<LightOrbBehavior>()?.gameObject;
        }
        if(other.CompareTag("Puzzle"))
        {
            puzzleInRange = true;
            puzzleObject = other.gameObject;
            Debug.Log(puzzleObject.name);
        }
        if(other.CompareTag("Trap"))
        {
            trapInRange = true;
            trapObject = other.gameObject;
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
        if(other.CompareTag("Trap"))
        {
            trapInRange = false;
            trapObject = null;
        }
    }
}
