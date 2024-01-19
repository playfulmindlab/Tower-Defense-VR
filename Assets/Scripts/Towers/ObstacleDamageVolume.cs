using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDamageVolume : Damage
{
    [SerializeField] protected float effectTime = 5f;

    protected Effect volumeEffect;

    protected virtual void Start()
    {
        volumeEffect = new Effect("Spikes", DamageValue, FirerateValue, attackType, effectTime);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            AppliedEffect effect = new AppliedEffect(EnemySpawner.enemyTransformPairs[other.transform], volumeEffect);
            TowerDefenseManager.EnqueueEffectToApply(effect);
        }
    }
}

