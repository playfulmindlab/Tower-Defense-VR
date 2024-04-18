using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeBombItem : ItemScript
{
    [SerializeField] float slowAmount = 0.5f;

    protected override void OnItemUse()
    {
        Collider[] enemiesInRadius = Physics.OverlapSphere(transform.position, radius, targetLayer);

        for (int i = 0; i < enemiesInRadius.Length; i++)
        {
            Enemy enemy = EnemySpawner.enemyTransformPairs[enemiesInRadius[i].transform];
            //Debug.Log("Orig Speed: " + enemy.Speed + " // " + slowAmount);
            Effect slowEffect = new Effect(gameObject.name + " - Slow", slowAmount, enemy.Speed, 3f);
            AppliedEffect effect = new AppliedEffect(enemy, slowEffect);
            AudioManager.instance.PlaySFXArray("StatusFrozen", enemy.transform.position);
            TowerDefenseManager.EnqueueEffectToApply(effect);

            base.OnItemUse();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
