using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDamage : Damage, IDamageMethod
{
    [SerializeField] private Transform laserPivot;
    [SerializeField] private LineRenderer laserRenderer;

    public override void DamageTick(Enemy target)
    {
        if (target)
        {
            laserRenderer.enabled = true;
            laserRenderer.SetPosition(0, laserPivot.position);
            laserRenderer.SetPosition(1, target.transform.position);

            if (delay > 0f)
            {
                delay -= Time.deltaTime;
                return;
            }

            TowerDefenseManager.EnqueueDamageData(new EnemyDamage(target, damage, target.damageResistance));

            delay = 1f / firerate;
            return;
        }
        else
        {
            laserRenderer.enabled = false;
        }
    }
}

