using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleMotionInertia : MonoBehaviour
{
    public float inertiaStrength = 1.0f; // cuánto se arrastra el humo
    private ParticleSystem ps;
    private Vector3 lastPos;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        lastPos = transform.position;
    }

    void LateUpdate()
    {
        Vector3 velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;

        int count = ps.particleCount;
        if (particles == null || particles.Length < count)
            particles = new ParticleSystem.Particle[count];

        ps.GetParticles(particles, count);

        for (int i = 0; i < count; i++)
        {
            // aplica fuerza opuesta al movimiento
            particles[i].velocity -= velocity * inertiaStrength * Time.deltaTime;
        }

        ps.SetParticles(particles, count);
    }
}
