using System.Collections;
using UnityEngine;

public class SecretBossBehavior : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private float triggerRadius = 3f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Ui hookup (optional in prefab)")]
    [SerializeField] private GameObject uiObject;

    [Header("Despawn")]
    [SerializeField] private float despawnDelay = 0.5f;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip triggerSfx;


    private SecretBossHallucinationUI ui;
    private bool triggered;

    // called from SpawnDirector right after Instantiate
    public void Init(GameObject uiObj)
    {
        uiObject = uiObj;
        cacheUiComponent();
    }

    private void Awake()
    {
        cacheUiComponent();
    }

    private void cacheUiComponent()
    {
        if (uiObject != null)
        {
            ui = uiObject.GetComponent<SecretBossHallucinationUI>();
        }
    }

    private void Update()
    {
        if (triggered) return;

        // check for player inside trigger radius
        Collider2D hit = Physics2D.OverlapCircle(transform.position, triggerRadius, playerLayer);
        if (hit != null)
        {
            onTriggered();
        }
    }

    private void onTriggered()
    {
        triggered = true;

        // show UI
        if (ui != null)
        {
            ui.PlayHallucination();
        }

        // play SFX
        if (audioSource != null && triggerSfx != null)
        {
            audioSource.PlayOneShot(triggerSfx);
        }

        // hide boss visuals immediately
        disableVisualsAndColliders();

        // destroy after delay
        StartCoroutine(despawnRoutine());
    }


    private void disableVisualsAndColliders()
    {
        // disable all renderers
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r != null) r.enabled = false;
        }

        // disable all 2d colliders
        var colliders2d = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders2d)
        {
            if (c != null) c.enabled = false;
        }
    }

    private IEnumerator despawnRoutine()
    {
        if (despawnDelay > 0f)
            yield return new WaitForSeconds(despawnDelay);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
