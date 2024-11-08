using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FinalDoor : MonoBehaviour, IInteractable
{
    private Animator anim;
    [SerializeField] private GameObject lightOrb;
    
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Interact()
    {
        anim.SetTrigger("OpenDoors");
        this.enabled = false;
        GetComponent<Collider>().enabled = false;
    }
}

public interface IInteractable
{
    public void Interact();
}


