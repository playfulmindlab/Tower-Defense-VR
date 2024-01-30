using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileDamage : Damage, IDamageMethod
{
    public LayerMask enemyLayer;
    [SerializeField] protected ParticleSystem missileSystem;
    [SerializeField] protected Transform towerHead;
    [SerializeField] protected float projectileSpeed = 30f;

    protected ParticleSystem.MainModule missileSystemMain;

    public override void Init(float damage, float firerate)
    {
        missileSystemMain = missileSystem.main;
        missileSystemMain.startSpeed = projectileSpeed;
        base.Init(damage, firerate);
    }

    public void ActivateGun(bool activeState)
    {
        if (activeState == true)
        {
            missileSystemMain.loop = true;
            missileSystemMain.maxParticles = 1000;
            var emission = missileSystem.emission;
            emission.rateOverTimeMultiplier = missileSystemMain.startSpeedMultiplier / 5f;
            missileSystem.Play();
        }
        else
        {
            missileSystem.Stop();
            missileSystemMain.loop = false;
            missileSystemMain.maxParticles = 10;
            var emission = missileSystem.emission;
            emission.rateOverTimeMultiplier = 0f;
        }
    }

    public override void DamageTick(Enemy target)
    {
        if (target && canFire == true)
        {
            if (delay > 0f)
            {
                delay -= Time.deltaTime;
                return;
            }

            missileSystemMain.startRotationX = towerHead.forward.x;
            missileSystemMain.startRotationY = towerHead.forward.y;
            missileSystemMain.startRotationZ = towerHead.forward.z;

            missileSystem.Play();

            delay = 1f / firerate;
            return;
        }
    }
}
