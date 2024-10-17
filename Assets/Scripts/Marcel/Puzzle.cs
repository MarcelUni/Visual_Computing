using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour, IInteractable
{
    public GameObject puzzleSolvedObject;


    public void Interact()
    {
        puzzleSolvedObject.SetActive(true);
        GetComponent<Collider>().enabled = false;
    }
}
