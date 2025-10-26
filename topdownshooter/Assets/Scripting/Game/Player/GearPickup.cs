using UnityEngine;

public class GearPickup : MonoBehaviour
{
    public int amount = 1;                  // Anzahl Zahnräder, die dieses Objekt gibt
    public AudioClip pickupSound;           // Sound beim Aufsammeln
    public float pickupSoundVolume = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Player Inventory
            PlayerInventory playerInventory = collision.GetComponent<PlayerInventory>();
            if (playerInventory != null)
            {
                playerInventory.AddGears(amount);
            }

            // Sound abspielen
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupSoundVolume);
            }

            // Pickup zerstören
            Destroy(gameObject);
        }
    }
}
