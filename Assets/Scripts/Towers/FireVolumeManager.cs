using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireVolumeManager : MonoBehaviour
{
    [SerializeField] private FlamethrowerDamage baseClass;
    [SerializeField] private float effectTime = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Effect flameEffect = new Effect("Fire", baseClass.DamageValue, baseClass.FirerateValue, effectTime);
            AppliedEffect effect = new AppliedEffect(EnemySpawner.enemyTransformPairs[other.transform], flameEffect);
            TowerDefenseManager.EnqueueEffectToApply(effect);
        }
    }
}
