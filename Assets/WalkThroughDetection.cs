using UnityEngine;
using UnityEngine.Events;

public class WalkThroughDetection : MonoBehaviour
{
    public UnityEvent WentThroughEvent;

    private void OnTriggerEnter(Collider other)
    {   
        if(other.CompareTag("Player"))
        {
            Debug.Log("fuck off");
            WentThroughEvent?.Invoke();
        }
    }


}
