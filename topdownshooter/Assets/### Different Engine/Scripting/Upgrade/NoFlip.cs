using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class NoFlip : MonoBehaviour
{
    [Header("World sprites that should not flip")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("UI images that should not flip")]
    [SerializeField] private Image[] uiImages;

    private void LateUpdate()
    {
        // sign of the owner (player/enemy) on X
        float ownerSign = Mathf.Sign(transform.localScale.x);
        if (ownerSign == 0f) ownerSign = 1f;

        
        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                var sr = spriteRenderers[i];
                if (sr == null) continue;

                Transform t = sr.transform;
                Vector3 s = t.localScale;
                float mag = Mathf.Abs(s.x);
                s.x = mag * ownerSign;
                t.localScale = s;
            }
        }

        if (uiImages != null)
        {
            for (int i = 0; i < uiImages.Length; i++)
            {
                var img = uiImages[i];
                if (img == null) continue;

                Transform t = img.transform;
                Vector3 s = t.localScale;
                float mag = Mathf.Abs(s.x);
                s.x = mag * ownerSign;
                t.localScale = s;
            }
        }
    }
}
