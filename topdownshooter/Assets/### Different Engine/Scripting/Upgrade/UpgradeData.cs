using UnityEngine;

[CreateAssetMenu(menuName = "Rogue/Upgrade Data", fileName = "Upgrade_")]
public class UpgradeData : ScriptableObject
{
    [Header("Identity")]
    public string title;
    public UpgradeType type;
    public Sprite icon;

    [Header("Effect")]
    public float value = 1.10f;
    public bool additive = false;

    [TextArea] public string description;

    [Header("Leveling")]
    [SerializeField, Min(0)] private int level = 0;
    [SerializeField, Min(1)] private int maxLevel = 5;

    public int Level => level;
    public int MaxLevel => maxLevel;
    public bool IsAtCap => level >= maxLevel;

    public void LevelUp()
    {
        if (level < maxLevel) level++;
    }

    public string GetBaseTitle()
    {
        return string.IsNullOrEmpty(title) ? "Upgrade" : title;
    }

    public string GetLevelProgress()
    {
        return $"Lv. {level + 1}";
    }

    public string GetDisplayTitle()
    {
        return GetBaseTitle();
    }

    public void ResetLevel()
    {
        level = 0;
    }
}
