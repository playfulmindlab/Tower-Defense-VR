using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDamageVolume : Damage
{
    [SerializeField] protected float effectTime = 5f;

    protected Effect volumeEffect;
    protected Queue<Transform> damagingEnemies = new Queue<Transform>();

    protected virtual void Start()
    {
        volumeEffect = new Effect(gameObject.name + " - Spikes", DamageValue, FirerateValue, attackType, effectTime);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            AppliedEffect effect = new AppliedEffect(EnemySpawner.enemyTransformPairs[other.transform], volumeEffect);
            TowerDefenseManager.EnqueueEffectToApply(effect);
        }
    }

    private void OnDestroy()
    {
        damagingEnemies.Clear();
    }
}

