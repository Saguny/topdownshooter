using UnityEngine;

public class FinalRushArena : MonoBehaviour
{
    [Header("Radius")]
    [SerializeField]
    private float radius = 15.15f;   // set this to match your visual circle in the inspector

    public float Radius => radius;

    public void Initialize(Vector3 center)
    {
        transform.position = center;
        // radius is taken from inspector; no auto-magic, you control it
    }
}
