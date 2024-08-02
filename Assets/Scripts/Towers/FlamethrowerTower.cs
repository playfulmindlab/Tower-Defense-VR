using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerTower : TowerBehaviour
{
    [SerializeField] float rotateSpeed;

    public override void Tick()
    {
        if (healthBar != null && shieldBar != null)
        {
            healthBar.value = health;
            shieldBar.value = shield;
        }

        currentDamageMethodClass.DamageTick(target);

        if (canFire == true && towerPivot != null)
        {
            if (target != null)
            {
                Vector3 posDifference = target.transform.position - transform.position;
                posDifference.y = 0;

                float pingpong = Mathf.PingPong(Time.time * rotateSpeed * 5f, 90) - 45;
                towerPivot.transform.rotation = Quaternion.LookRotation(posDifference, Vector3.up) * Quaternion.Euler(0f, pingpong, 0f);
            }
        }
    }
}
