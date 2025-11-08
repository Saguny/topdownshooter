using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public int gearCount = 0;
    public int gearsForUpgrade = 5;
    public float gearIncreaseMultiplier = 1.3f;

    public List<UpgradeData> allUpgrades;
    public UpgradeMenuUI upgradeMenuUI;

    [SerializeField] private ProgressBarGradient progressBarGradient;

    private List<UpgradeData> runtimeUpgrades = new List<UpgradeData>();
    private int currentLevel = 1;
    private Aura aura;
    public bool hasAura = false;
    private StatContext stats;

    private void OnEnable()
    {
        BuildRuntimeUpgrades();
        UpdateProgress();
    }

    private void OnDisable()
    {
        foreach (var u in allUpgrades) if (u != null) u.ResetLevel();
        runtimeUpgrades.Clear();
    }

    private void Awake()
    {
        aura = GetComponentInChildren<Aura>(true);
        stats = GetComponent<StatContext>();
        if (aura == null) aura = transform.Find("Upgrades/Aura")?.GetComponent<Aura>();
    }

    public void AddGears(int amount)
    {
        gearCount += amount;
        UpdateProgress();
        if (gearCount >= gearsForUpgrade)
        {
            gearCount -= gearsForUpgrade;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        gearsForUpgrade = Mathf.CeilToInt(gearsForUpgrade * gearIncreaseMultiplier);
        UpdateProgress();
        OpenUpgradeMenu();
    }

    private void OpenUpgradeMenu()
    {
        Time.timeScale = 0f;

        List<UpgradeData> pool = new List<UpgradeData>(runtimeUpgrades);
        pool.RemoveAll(u => u == null || u.IsAtCap);

        if (hasAura) pool.RemoveAll(u => u.type == UpgradeType.AuraUnlock);
        else pool.RemoveAll(u => u.type == UpgradeType.AuraDamage || u.type == UpgradeType.AuraRadius);

        List<UpgradeData> randomUpgrades = new List<UpgradeData>();
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            randomUpgrades.Add(pool[index]);
            pool.RemoveAt(index);
        }

        randomUpgrades.RemoveAll(u => u == null || u.IsAtCap);
        upgradeMenuUI.Open(randomUpgrades, ApplyUpgrade);
    }

    private void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            Time.timeScale = 1f;
            return;
        }

        if (stats != null) stats.Apply(upgrade);

        switch (upgrade.type)
        {
            case UpgradeType.AuraUnlock:
                if (aura != null && !aura.gameObject.activeSelf) aura.gameObject.SetActive(true);
                hasAura = true;
                break;
            case UpgradeType.AuraDamage:
                if (aura != null && aura.gameObject.activeSelf) aura.damage *= upgrade.value;
                break;
            case UpgradeType.AuraRadius:
                if (aura != null && aura.gameObject.activeSelf)
                {
                    aura.radius *= upgrade.value;      // gameplay radius
                    aura.IncreaseVisualScale(1.15f);   // visual: +15% transform scale
                }
                break;
            case UpgradeType.AOEAttack:
                {
                    var aoe = GetComponent<AOEAttack>();
                    if (aoe == null)
                        aoe = gameObject.AddComponent<AOEAttack>();

                    // Werte pro Level skalieren (Beispiel)
                    float radius = 3f + (upgrade.Level - 1) * 0.5f;
                    float damage = 20f + (upgrade.Level - 1) * 5f;
                    float interval = Mathf.Max(2f, 5f - (upgrade.Level - 1) * 0.3f);

                    aoe.Activate(radius, damage, interval);
                    break;
                }
        }

        upgrade.LevelUp();
        Time.timeScale = 1f;
    }

    public void ResetRun()
    {
        gearCount = 0;
        gearsForUpgrade = 15;
        currentLevel = 1;
        foreach (var u in runtimeUpgrades) if (u != null) u.ResetLevel();
        UpdateProgress();
        if (stats != null) stats.ResetStats();
        hasAura = false;
    }

    private void UpdateProgress()
    {
        float t = gearsForUpgrade > 0 ? Mathf.Clamp01((float)gearCount / gearsForUpgrade) : 0f;
        if (progressBarGradient) progressBarGradient.SetProgress01(t);
    }

    private void BuildRuntimeUpgrades()
    {
        runtimeUpgrades.Clear();
        if (allUpgrades == null) return;
        foreach (var src in allUpgrades)
        {
            if (src == null) continue;
            var inst = Instantiate(src);
            inst.ResetLevel();
            runtimeUpgrades.Add(inst);
        }
    }
}