using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbingMissileDamage : MissileDamage
{
    public override void DamageTick(Enemy target)
    {
        if (target)
        {
            if (delay > 0f)
            {
                delay -= Time.deltaTime;
                return;
            }

            missileSystemMain.startRotationX = towerHead.forward.x;
            missileSystemMain.startRotationY = towerHead.forward.y;
            missileSystemMain.startRotationZ = towerHead.forward.z;

            float dist = Vector3.Distance(target.transform.position, towerHead.transform.position);
            projectileSpeed = Mathf.Sqrt(dist * missileSystemMain.gravityModifierMultiplier * 9.81f);
            missileSystemMain.startSpeed = projectileSpeed;

            missileSystem.Play();

            delay = 1f / firerate;
            return;
        }
    }
}
