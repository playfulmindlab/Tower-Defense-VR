using UnityEngine;

public class LavaPoolVolume : ObstacleDamageVolume
{
    [SerializeField] float damageOverTime = 1f;
    [SerializeField] float delayOnEachDamage = 1f;
    [SerializeField] float lifeLength = 3f;

    protected override void Start()
    {
        Init(damageOverTime, delayOnEachDamage);
        volumeEffect = new Effect(gameObject.name + " - LavaPool", DamageValue, FirerateValue, attackType, effectTime);

        Destroy(this.gameObject, lifeLength);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            damagingEnemies.Dequeue();
        }
    }
}
