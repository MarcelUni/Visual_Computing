using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorPuzzle : MonoBehaviour, IInteractable
{
    private Animator anim;
    

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Interact()
    {
        anim.SetTrigger("OpenDoors");
        this.enabled = false;
    }

    void OnTriggerEnter()
    {
        
    }

}


public interface IInteractable
{
    public void Interact();
}
