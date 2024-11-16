using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbingMissileDamage : MissileDamage
{
    TowerBehaviour towerScript;
    JumpedLobberTowerControls jumpedLobberTowerControls;

    public override void Init(float damage, float firerate)
    {
        towerScript = GetComponent<TowerBehaviour>();
        audioSource = GetComponent<AudioSource>();
        missileSystemMain = missileSystem.main;
        jumpedLobberTowerControls = GetComponent<JumpedLobberTowerControls>();
        base.Init(damage, firerate);
    }
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
            missileSystemMain.startSpeed = Mathf.Sqrt(dist * missileSystemMain.gravityModifierMultiplier * 9.81f);

            missileSystem.Play();

            delay = 1f / firerate;
            return;
        }
    }

    public override void ActivateGun(bool activeState)
    {
        if (activeState == true)
        {
            audioSource.loop = true;
            missileSystemMain.loop = true;
            missileSystemMain.startSpeedMultiplier = jumpedLobberTowerControls.ProjectileSpeed;
            missileSystemMain.maxParticles = 10;
            missileSystemMain.duration = 1 / towerScript.firerate;
            //var emission = missileSystem.emission;
            //emission.rateOverTimeMultiplier = missileSystemMain.startSpeedMultiplier;// / 5f;
            missileSystem.Play();
        }
        else
        {
            missileSystem.Stop();
            audioSource.loop = false;
            missileSystemMain.loop = false;
            missileSystemMain.maxParticles = 10;
            missileSystemMain.duration = 5;
            //var emission = missileSystem.emission;
            //emission.rateOverTimeMultiplier = 0f;
        }
    }
}
