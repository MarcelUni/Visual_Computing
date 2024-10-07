using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public bool puzzleInRange;

    private GameObject puzzleObject;


    public void Interact()
    {
        if(puzzleInRange != true)
            return;

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
}
