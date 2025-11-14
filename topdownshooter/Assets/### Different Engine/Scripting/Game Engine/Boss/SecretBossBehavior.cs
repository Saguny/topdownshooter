using UnityEngine;

public class SecretBossBehavior : MonoBehaviour
{
    public float triggerRadius = 3f;
    public LayerMask playerLayer;
    public SecretBossHallucinationUI ui;

    private bool triggered = false;

    private void Update()
    {
        if (triggered) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, triggerRadius, playerLayer);
        if (hits.Length > 0)
        {
            triggered = true;

            if (ui != null)
                ui.PlayHallucination();

            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
