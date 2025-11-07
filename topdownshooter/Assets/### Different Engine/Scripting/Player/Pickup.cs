using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int gears = 1;

    [Header("Pickup Sound")]
    [SerializeField] private AudioClip pickupSound;         // 🎵 Sound beim Aufheben
    [Range(0f, 1f)][SerializeField] private float pickupVolume = 1f; // 🔊 Lautstärkeregler

    private bool pulling;
    private Transform target;

    public void PullTo(Transform t)
    {
        pulling = true;
        target = t;
    }

    private void Update()
    {
        if (!pulling || target == null) return;
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * 8f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent(out PlayerInventory inv))
            inv.AddGears(gears);

        // 🔊 Optional: Sound für Gears
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);

        Destroy(gameObject);
    }

}
