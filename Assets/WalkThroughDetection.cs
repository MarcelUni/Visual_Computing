using UnityEngine;
using UnityEngine.Events;

public class WalkThroughDetection : MonoBehaviour
{
    public UnityEvent WentThroughEvent;

    private void OnTriggerEnter(Collider other)
    {
        WentThroughEvent = new UnityEvent();
        
        if(other.CompareTag("Player"))
        {
            WentThroughEvent?.Invoke();
        }
    }


}
