using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPuzzle : MonoBehaviour, IInteractable
{
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Interact()
    {
        anim.SetTrigger("OpenDoors");
        this.enabled = false;
    }
}

public interface IInteractable
{
    public void Interact();
}
