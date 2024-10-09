using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public bool puzzleInRange;
    private GameObject puzzleObject;
    private Animator anim;

    public float pickUpRadius;
    public LayerMask interactLayer;


    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void Interact()
    {
        if(puzzleInRange != true)
            return;

        anim.SetTrigger("Interact");
        puzzleObject.GetComponent<IInteractable>()?.Interact();
        Debug.Log("Solving puzzle wup wup");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Puzzle"))
        {
            puzzleInRange = true;
            puzzleObject = other.gameObject;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Puzzle"))
        {
            puzzleInRange = false;;
            puzzleObject = null;
        }
    }

    private void PickUpObject(GameObject pickUpObj)
    {
    // RaycastHit[] = Physics.SphereCastAll(transform.position, pickUpRadius, )
        

        
    }

}
