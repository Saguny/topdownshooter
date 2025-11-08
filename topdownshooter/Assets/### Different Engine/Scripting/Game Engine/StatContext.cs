using UnityEngine;

public class StatContext : MonoBehaviour
{
    public float fireRateMul = 1f;
    public float bulletSpeedMul = 1f;
    public float pickupRadiusMul = 1f;
    public float bulletDamageMul = 1f;

    public int bulletCountAdd = 0;

    public void ResetStats()
    {
        fireRateMul = 1f;
        bulletSpeedMul = 1f;
        pickupRadiusMul = 1f;
        bulletDamageMul = 1f;
        bulletCountAdd = 0;
    }

    public void Apply(UpgradeData u)
    {
        switch (u.type)
        {
            case UpgradeType.FireRate:
                fireRateMul *= u.additive ? (1f + u.value) : u.value;
                break;
            case UpgradeType.BulletSpeed:
                bulletSpeedMul *= u.additive ? (1f + u.value) : u.value;
                break;
            case UpgradeType.BulletCount:
                bulletCountAdd += Mathf.RoundToInt(u.value);
                break;
            case UpgradeType.PickupRadius:
                pickupRadiusMul *= u.additive ? (1f + u.value) : u.value;
                break;
            case UpgradeType.BulletDamage:
                bulletDamageMul *= u.additive ? (1f + u.value) : u.value;
                break;
            case UpgradeType.AuraUnlock:
            case UpgradeType.AuraDamage:
            case UpgradeType.AuraRadius:
                break;
        }
    }
}