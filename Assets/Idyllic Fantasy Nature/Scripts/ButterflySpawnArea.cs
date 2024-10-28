using UnityEngine;

namespace IdyllicFantasyNature
{
    public class ButterflySpawnArea : MonoBehaviour
    {
        public Collider Collider { get; set; }

        [Tooltip("the minimum cooldown for the butterfly to respawn")]
        [Min(0)] public float MinCooldown;
        [Tooltip("the maximum cooldown for the butterfly to respawn")]
        [Min(1)] public float MaxCooldown;

        public ButterflySpawn ButterflySpawn;

        private void Start()
        {
            Collider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.transform.CompareTag("Player"))
            {
               
            }
        }
    }
}
