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

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = EnemySpawner.enemyTransformPairs[other.transform];

            AppliedEffect effect = new AppliedEffect(enemy, volumeEffect);
            TowerDefenseManager.EnqueueEffectToApply(effect);

            Effect slowEffect = new Effect(gameObject.name + " - Slow", 0.5f, enemy.Speed, 3f);
            effect = new AppliedEffect(enemy, slowEffect);
            TowerDefenseManager.EnqueueEffectToApply(effect);
            AudioManager.instance.PlaySFXArray("FrozenStatus", enemy.transform.position);
        }
    }
}