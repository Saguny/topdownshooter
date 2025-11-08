using UnityEngine;
using UnityEngine.UI;

public class ProgressBarGradient : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color leftColor = new Color(0.6f, 0.9f, 1f, 0.6f);
    [SerializeField] private Color rightColor = new Color(0.8f, 0.6f, 1f, 0.6f);
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.3f);
    [SerializeField] private int gradientWidth = 256;

    private Sprite gradientSprite;

    private void Awake()
    {
        if (backgroundImage) backgroundImage.color = backgroundColor;

        if (fillImage)
        {
            gradientSprite = CreateGradientSprite(leftColor, rightColor, gradientWidth);
            fillImage.sprite = gradientSprite;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0f;
            fillImage.color = Color.white;
        }

        if (slider)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.wholeNumbers = false;
        }
    }

    public void SetProgress01(float t)
    {
        t = Mathf.Clamp01(t);
        if (slider) slider.value = t;
        if (fillImage) fillImage.fillAmount = t;
    }

    private Sprite CreateGradientSprite(Color left, Color right, int width)
    {
        if (width < 2) width = 2;
        Texture2D tex = new Texture2D(width, 1, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1);
            Color c = Color.Lerp(left, right, t);
            tex.SetPixel(x, 0, c);
        }

        tex.Apply(false, false);
        return Sprite.Create(tex, new Rect(0, 0, width, 1), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }
}
