using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotionItem : ItemScript
{
    [SerializeField] float healthAmount = 30;

    protected override void OnItemUse()
    {
        Collider[] towersInRadius = Physics.OverlapSphere(transform.position, radius, targetLayer);

        for (int i = 0; i < towersInRadius.Length; i++)
        {
            TowerBehaviour tower = towersInRadius[i].GetComponent<TowerBehaviour>();
            int healing = Mathf.RoundToInt(tower.maxHealth * (healthAmount / 100));
            tower.Heal(healing);
        }

        base.OnItemUse();
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}


