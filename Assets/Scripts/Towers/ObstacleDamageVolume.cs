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

    protected void OnTriggerEnter(Collider other)
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

    /*
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            AppliedEffect effect = new AppliedEffect(EnemySpawner.enemyTransformPairs[other.transform], volumeEffect);
            TowerDefenseManager.EnqueueEffectToApply(effect);
        }
    }
    */
    /*protected void Update()
    {
        damagingEnemies = damagingEnemies.RemoveAll(x => x is null);
        foreach (Transform enemy in damagingEnemies)
        {
            AppliedEffect effect = new AppliedEffect(EnemySpawner.enemyTransformPairs[enemy], volumeEffect);
            TowerDefenseManager.EnqueueEffectToApply(effect);
        }
    }*/
}

