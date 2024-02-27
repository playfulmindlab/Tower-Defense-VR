using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireVolumeManager : MonoBehaviour
{
    [SerializeField] private FlamethrowerDamage baseClass;
    [SerializeField] private float effectTime = 5f;

    [SerializeField] int streamNum = 0;

    string effectID = "";

    private void Start()
    {
        effectID = baseClass.gameObject.name.ToString() + "_" + streamNum.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Effect flameEffect = new Effect("Fire_" + effectID, baseClass.DamageValue, baseClass.FirerateValue, baseClass.GetAttackType, effectTime);
            AppliedEffect effect = new AppliedEffect(EnemySpawner.enemyTransformPairs[other.transform], flameEffect);
            TowerDefenseManager.EnqueueEffectToApply(effect);
        }
    }
}
