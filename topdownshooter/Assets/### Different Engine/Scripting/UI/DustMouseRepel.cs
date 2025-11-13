using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DustMouseRepel : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera;                 // assign your UI/Main Camera

    [Header("Repel Settings")]
    public float repelRadius = 1.25f;           // world units
    public float repelStrength = 0.75f;         // velocity added per second
    public AnimationCurve falloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Glow / Size Response")]
    [Range(0f, 0.5f)] public float alphaBoost = 0.15f;
    [Range(1f, 1.6f)] public float sizeMultiplier = 1.2f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        int max = ps.main.maxParticles;
        if (particles == null || particles.Length < max)
            particles = new ParticleSystem.Particle[max];

        int count = ps.GetParticles(particles);
        Vector3 mouseWorld = GetMouseWorldPosition();

        for (int i = 0; i < count; i++)
        {
            Vector3 toParticle = particles[i].position - mouseWorld;
            float dist = toParticle.magnitude;

            if (dist < repelRadius)
            {
                float t = Mathf.Clamp01(dist / repelRadius);
                float force = repelStrength * falloff.Evaluate(1f - t);

                // push away from cursor (softly)
                if (dist > 0.0001f)
                {
                    Vector3 dir = toParticle / dist;
                    particles[i].velocity += dir * force * Time.deltaTime;
                }

                // subtle glow + size bump when near
                Color32 c = particles[i].GetCurrentColor(ps);
                float a = Mathf.Clamp01(c.a / 255f + alphaBoost * (1f - t));
                c.a = (byte)Mathf.RoundToInt(a * 255f);
                particles[i].startColor = c;

                float baseSize = particles[i].GetCurrentSize(ps);
                float targetSize = baseSize * Mathf.Lerp(1f, sizeMultiplier, 1f - t);
                particles[i].startSize = Mathf.Lerp(particles[i].startSize, targetSize, 0.15f);
            }
        }

        ps.SetParticles(particles, count);
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 m = Input.mousePosition;

        // if using a Screen Space - Camera canvas, the camera has a sensible near clip plane
        // otherwise, pick a depth where your particles live (z distance from camera)
        float depth = Mathf.Abs(targetCamera.transform.position.z - transform.position.z);
        m.z = depth;

        return targetCamera.ScreenToWorldPoint(m);
    }
}
