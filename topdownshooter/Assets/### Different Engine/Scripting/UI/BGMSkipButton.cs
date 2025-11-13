using UnityEngine;
using UnityEngine.UI;

public class BGMSkipButton : MonoBehaviour
{
    private void Awake()
    {
        // auto-wire at runtime
        GetComponent<Button>().onClick.AddListener(OnSkipClicked);
    }

    private void OnSkipClicked()
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.NextTrack();
    }
}
