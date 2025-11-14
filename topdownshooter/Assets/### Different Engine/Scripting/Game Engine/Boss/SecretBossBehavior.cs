using UnityEngine;

public class SecretBossBehavior : MonoBehaviour
{
    public float triggerRadius = 3f;
    public LayerMask playerLayer;

    // assign the UI object (the thing on the canvas) here
    [SerializeField] private GameObject uiObject;

    private SecretBossHallucinationUI ui;
    private bool triggered = false;

    private void Awake()
    {
        if (uiObject != null)
        {
            ui = uiObject.GetComponent<SecretBossHallucinationUI>();
        }
    }

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
