using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeVolume : ObstacleDamageVolume
{
    protected override void Start()
    {
        volumeEffect = new Effect("Spikes", DamageValue, FirerateValue, attackType, effectTime);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            List<Effect> enemyEffects = other.GetComponent<Enemy>().activeEffects;
            if (enemyEffects.Contains(volumeEffect))
            {
                enemyEffects.Remove(volumeEffect);
            }
        }
    }

}
