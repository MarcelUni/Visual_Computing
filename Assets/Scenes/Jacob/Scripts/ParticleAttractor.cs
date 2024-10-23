using UnityEngine;

public class ParticleAttractor : MonoBehaviour
{
    [Tooltip("The Transform of the player.")]
    public Transform playerTransform;

    [Tooltip("Maximum speed at which particles move toward the player.")]
    public float maxAttractionSpeed = 5f;

    [Tooltip("Time in seconds before particles start moving toward the player after emission starts.")]
    public float attractionDelay = 2f;

    [Tooltip("Duration of the easing effect (how long it takes to reach max speed).")]
    public float easeDuration = 2f;

    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;
    private bool attractionStarted = false;
    private bool particleSystemStarted = false;
    private float timer = 0f;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();

        if (particleSystem == null)
        {
            Debug.LogError("ParticleAttractor script requires a Particle System component.");
            enabled = false;
            return;
        }

        // Initialize the particle array with the max number of particles.
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
    }

    void Update()
    {
        // Check if the particle system has started playing
        if (particleSystem.isPlaying && !particleSystemStarted)
        {
            // Set flag indicating the particle system has started playing
            particleSystemStarted = true;
        }

        // Once the particle system has started, start the timer for the attraction delay
        if (particleSystemStarted && !attractionStarted)
        {
            timer += Time.deltaTime;

            // When the timer exceeds the attraction delay, start particle attraction
            if (timer >= attractionDelay)
            {
                attractionStarted = true;
                timer = 0f; // Reset timer for the ease effect
            }
        }

        // If the attraction has started, move particles toward the player with easing
        if (attractionStarted)
        {
            int numParticlesAlive = particleSystem.GetParticles(particles);

            for (int i = 0; i < numParticlesAlive; i++)
            {
                // Calculate direction from particle to player
                Vector3 directionToPlayer = (playerTransform.position - particles[i].position).normalized;

                // Calculate the distance between the particle and the player
                float distanceToPlayer = Vector3.Distance(particles[i].position, playerTransform.position);

                // Applyy easing to the speed (ease in effect)
                float easeFactor = Mathf.Clamp01(timer / easeDuration); // 0 to 1 based on time elapsed
                float easedSpeed = maxAttractionSpeed * easeFactor * easeFactor; // Ease in (quadratic)

                // Set particle velocity toward the player with eased speed
                particles[i].velocity = directionToPlayer * easedSpeed;

                 // If the distance is less than a small threshold, consider the particle to have reached the player
                if (distanceToPlayer < 0.1f) // You can adjust the threshold value as needed
                {
                    particles[i].remainingLifetime = 0f;
                }
            }

            // Apply the modified particle data back to the particle system
            particleSystem.SetParticles(particles, numParticlesAlive);

            // Increase timer for the ease effect
            timer += Time.deltaTime;

            //  If no particles are left alive, stop the particle system
            if (numParticlesAlive == 0)
            {
                particleSystem.Stop();
                attractionStarted = false;
            }
        }
    }
}
