using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeVolume : Damage
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy target = other.GetComponent<Enemy>();
            TowerDefenseManager.EnqueueDamageData(new EnemyDamage(target, damage, target.damageResistance));
        }
    }
}
