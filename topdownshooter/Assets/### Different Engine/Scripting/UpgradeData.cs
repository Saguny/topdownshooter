using UnityEngine;

[CreateAssetMenu(menuName = "Rogue/Upgrade Data", fileName = "Upgrade_")]
public class UpgradeData : ScriptableObject
{
    public string title;
    public UpgradeType type;

    // multiplicative when additive == false (1.10 = +10%)
    // additive when true (e.g., +1 bullet)
    public float value = 1.10f;
    public bool additive = false;

    [TextArea] public string description;
}
