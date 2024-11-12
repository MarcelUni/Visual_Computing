using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Trap : MonoBehaviour, IInteractable
{
    public GameObject trapDismantledObject;
    public bool TrapDismantled;

    public void Interact()
    {
        trapDismantledObject.SetActive(true);
        
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        TrapDismantled = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if(TrapDismantled)
        {
            GetComponent<Collider>().enabled = false;
            return;
        }
        else if(other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().deathEvent?.Invoke();
        }
    }
}