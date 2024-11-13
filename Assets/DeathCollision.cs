using UnityEngine;

public class DeathCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().deathEvent?.Invoke();
        }
    }
}
