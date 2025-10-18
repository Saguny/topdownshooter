using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    float deltaTime = 0.0f;

    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        fpsText.text = $"{fps:0.} FPS";

        if (fps >= 60f)
        {
            fpsText.color = Color.green;
        }
        else if (fps >= 30f)
        {
            fpsText.color = Color.yellow;
        }
        else
        {
            fpsText.color = Color.red;
        }
    }
}
