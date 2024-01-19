using UnityEngine;

public class BlizzardPoolVolume : ObstacleDamageVolume
{
    [SerializeField] float damageOverTime = 1f;
    [SerializeField] float delayOnEachDamage = 1f;
    [SerializeField] float lifeLength = 3f;

    protected override void Start()
    {
        Init(damageOverTime, delayOnEachDamage);
        volumeEffect = new Effect("BlizzardPool", DamageValue, FirerateValue, attackType, effectTime);
        Destroy(this.gameObject, lifeLength);
    }
}