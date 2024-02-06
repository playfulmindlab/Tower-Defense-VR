using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBombItem : ItemScript
{
    [SerializeField] ElementType damageType;
    [SerializeField] float damageAmount = 30;

    protected override void OnItemUse()
    {
        Collider[] enemiesInRadius = Physics.OverlapSphere(transform.position, radius, targetLayer);

        for (int i = 0; i < enemiesInRadius.Length; i++)
        {
            Enemy enemy = enemiesInRadius[i].GetComponent<Enemy>();

            TowerDefenseManager.EnqueueDamageData(new EnemyDamage(enemy, damageAmount, enemy.GetResistanceModifier(damageType)));

            base.OnItemUse();
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
