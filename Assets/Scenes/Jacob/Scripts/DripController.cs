using UnityEngine;

public class DripController : MonoBehaviour
{
    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;

    public float lingerTime = 2f; // Time before the particle falls
    public float gravityForce = -9.8f; // Force applied after lingering
    public float dripThreshold = 0.5f; // Size threshold for dripping

    private float[] particleTimers; // Tracks how long each particle has existed

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        particleTimers = new float[particleSystem.main.maxParticles];
    }

    void Update()
    {
        int aliveParticles = particleSystem.GetParticles(particles);

        for (int i = 0; i < aliveParticles; i++)
        {
            // Increment timer for this particle
            particleTimers[i] += Time.deltaTime;

            // Gradually increase the particle's size while lingering
            if (particleTimers[i] < lingerTime)
            {
                particles[i].startSize = Mathf.Lerp(0.05f, dripThreshold, particleTimers[i] / lingerTime);
                particles[i].velocity = Vector3.zero; // Stay in place
            }
            else
            {
                // Apply gravity to simulate dripping after lingering
                particles[i].velocity += new Vector3(0, gravityForce * Time.deltaTime, 0);
            }
        }

        // Apply changes back to the particle system
        particleSystem.SetParticles(particles, aliveParticles);

        // Reset timers for dead particles
        ResetDeadParticleTimers(aliveParticles);
    }

    private void ResetDeadParticleTimers(int aliveParticles)
    {
        for (int i = aliveParticles; i < particleTimers.Length; i++)
        {
            particleTimers[i] = 0f; // Reset timer for particles not currently active
        }
    }
}
