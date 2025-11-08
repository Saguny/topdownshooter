using UnityEngine;

public class EnemyDamageScaler : MonoBehaviour
{
    [SerializeField] private EnemyArchetype archetype;
    [SerializeField] private DifficultyCurve difficulty;
    [SerializeField] private EnemyContactDamage contact;

    public void Initialize(float runTimeSeconds)
    {
       if (!contact) contact = GetComponent<EnemyContactDamage>();
       if(!contact || !archetype || !difficulty) return;

        bool baseDamageIsDPS = true;

        float scaled = archetype.baseDamage * difficulty.DamageAt(runTimeSeconds);

        contact.damagePerTick = baseDamageIsDPS ? scaled  * contact.tickInterval : scaled; 

    }
}
