using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("Gear Settings")]
    public int gearCount = 0;
    public int gearsForUpgrade = 5;
    public float gearIncreaseMultiplier = 1.5f;

    [Header("Upgrade Settings")]
    public List<UpgradeData> allUpgrades; // assign ScriptableObjects here
    public UpgradeMenuUI upgradeMenuUI;
    public Slider progressBar;

    private int currentLevel = 1;
    private AutoShooter autoShooter;
    private Aura aura; // optional, in case you add it later
    public bool hasAura = false;

    private void Awake()
    {
        autoShooter = GetComponent<AutoShooter>();
        aura = GetComponentInChildren<Aura>(true);

        if (aura == null)
        {
            aura = transform.Find("Upgrades/Aura")?.GetComponent<Aura>();   
        }

        if (progressBar != null)
            progressBar.maxValue = gearsForUpgrade;
    }

    public void AddGears(int amount)
    {
        gearCount += amount;

        if (progressBar != null)
            progressBar.value = gearCount;

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

        if (progressBar != null)
        {
            progressBar.maxValue = gearsForUpgrade;
            progressBar.value = gearCount;
        }

        OpenUpgradeMenu();
    }

    private void OpenUpgradeMenu()
    {
        Time.timeScale = 0f; // pause gameplay

        // pick 3 random upgrades
        List<UpgradeData> randomUpgrades = new List<UpgradeData>();
        List<UpgradeData> pool = new List<UpgradeData>(allUpgrades);

        if (hasAura)
        {
            pool.RemoveAll(u => u.type == UpgradeType.AuraUnlock);
        } else {
            pool.RemoveAll(u =>
            u.type == UpgradeType.AuraDamage ||
            u.type == UpgradeType.AuraRadius);
        }

        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            randomUpgrades.Add(pool[index]);
            pool.RemoveAt(index);
        }

        upgradeMenuUI.Open(randomUpgrades, ApplyUpgrade);
    }

    private void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.FireRate:
                autoShooter.fireRate *= upgrade.value; // smaller = faster
                break;

            case UpgradeType.BulletSpeed:
                {
                    Bullet bulletScript = autoShooter.bulletPrefab.GetComponent<Bullet>();
                    if (bulletScript != null)
                        bulletScript.Speed *= upgrade.value;
                    break;
                }


            case UpgradeType.BulletCount:
                autoShooter.bulletCount += Mathf.RoundToInt(upgrade.value);
                break;

            case UpgradeType.AuraUnlock:
                if (aura != null && !aura.gameObject.activeSelf)
                    aura.gameObject.SetActive(true);
                    hasAura = true;
                break;

            case UpgradeType.AuraDamage:
                if (aura != null && aura.gameObject.activeSelf)
                    aura.damage *= upgrade.value;
                break;

            case UpgradeType.AuraRadius:
                if (aura != null && aura.gameObject.activeSelf)
                    aura.radius *= upgrade.value;
                break;
        }

        Time.timeScale = 1f; // resume gameplay
    }

    public void ResetRun()
    {
        gearCount = 0;
        gearsForUpgrade = 5;
        currentLevel = 1;

        if (progressBar != null)
        {
            progressBar.maxValue = gearsForUpgrade;
            progressBar.value = 0;
        }
    }
}
