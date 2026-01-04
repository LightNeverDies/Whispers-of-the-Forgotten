using UnityEngine;

public class RainController : MonoBehaviour
{
    public GameObject exclusionZone;

    private ParticleSystem rainParticleSystem;
    private ParticleSystem.Particle[] particles;
    private Collider exclusionCollider;

    void Start()
    {
        rainParticleSystem = GetComponent<ParticleSystem>();
        exclusionCollider = exclusionZone.GetComponent<Collider>();

        if (rainParticleSystem == null)
        {
            return;
        }

        if (exclusionCollider == null)
        {
            return;
        }
    }

    void Update()
    {
        if (rainParticleSystem == null || exclusionCollider == null) return;

        if (particles == null || particles.Length < rainParticleSystem.main.maxParticles)
        {
            particles = new ParticleSystem.Particle[rainParticleSystem.main.maxParticles];
        }

        int numParticlesAlive = rainParticleSystem.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            Vector3 worldPosition = rainParticleSystem.transform.TransformPoint(particles[i].position);
            if (exclusionCollider.bounds.Contains(worldPosition))
            {
                particles[i].remainingLifetime = 0;
            }
        }

        rainParticleSystem.SetParticles(particles, numParticlesAlive);
    }
}
