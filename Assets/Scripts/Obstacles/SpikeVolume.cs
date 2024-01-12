using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeVolume : Damage
{
    //[SerializeField] private FlamethrowerDamage baseClass;
    [SerializeField] private float effectTime = 5f;

    Effect spikeEffect;

    private void Start()
    {
        spikeEffect = new Effect("Spikes", DamageValue, FirerateValue, effectTime);
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy target = other.GetComponent<Enemy>();
            TowerDefenseManager.EnqueueDamageData(new EnemyDamage(target, damage, target.damageResistance));
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            //Effect spikeEffect = new Effect("Spikes", baseClass.DamageValue, baseClass.FirerateValue, effectTime);
            AppliedEffect effect = new AppliedEffect(EnemySpawner.enemyTransformPairs[other.transform], spikeEffect);
            TowerDefenseManager.EnqueueEffectToApply(effect);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            List<Effect> enemyEffects = other.GetComponent<Enemy>().activeEffects;
            if (enemyEffects.Contains(spikeEffect))
            {
                enemyEffects.Remove(spikeEffect);
            }
        }
    }
}
