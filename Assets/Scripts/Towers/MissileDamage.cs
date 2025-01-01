using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileDamage : Damage, IDamageMethod
{
    public LayerMask enemyLayer;
    [SerializeField] protected ParticleSystem missileSystem;
    [SerializeField] protected Transform towerHead;
    //[SerializeField] protected float projectileSpeed = 30f;
    [SerializeField] string firingSFXName = "TowerBasicShot";

    protected ParticleSystem.MainModule missileSystemMain;
    protected AudioSource audioSource;

    protected float baseDamageValue = 0;
    protected float jumpDamageMultiplier;

    bool isJumped = false;
    bool isFiring = false;

    public override void Init(float damage, float firerate)
    {
        audioSource = GetComponent<AudioSource>();
        missileSystemMain = missileSystem.main;
        missileSystemMain.duration = 1 / firerate;

        //missileSystemMain.startSpeed = projectileSpeed;
        base.Init(damage, firerate);
        baseDamageValue = damage;
    }

    public void SetJumpMultiplier(float newMult)
    {
        jumpDamageMultiplier = newMult;
    }

    public virtual void ActivateGun(bool activeState)
    {
        //missileSystem.Stop();
        Debug.Log("ACTIVE: " + activeState);
        if (activeState == true)
        {
            audioSource.loop = true;
            missileSystemMain.loop = true;
            missileSystemMain.maxParticles = 100;
            UpdateDamage(damage * jumpDamageMultiplier);
            missileSystem.Play();
        }
        else
        {
            if (missileSystem != null)
                missileSystem.Stop();
            audioSource.loop = false;
            missileSystemMain.loop = false;
            missileSystemMain.maxParticles = 10;        
            UpdateDamage(baseDamageValue);
        }
    }

    public override void DamageTick(Enemy target)
    {
        Debug.Log("T9: " + target + "");

        //if ((target && !isJumped) || (isJumped && isFiring))//&& canFire == true)
        if (target != null)
        {
            if (delay > 0f)
            {
                delay -= Time.deltaTime;
                return;
            }

            //missileSystemMain.startRotationX = towerHead.forward.x;
            //missileSystemMain.startRotationY = towerHead.forward.y;
            //missileSystemMain.startRotationZ = towerHead.forward.z;

            AudioManager.instance.PlaySFXArray(firingSFXName, transform.position);
            missileSystem.Play();

            delay = 1f / firerate;
            return;
        }
        else if (target == null && missileSystem.isPlaying)
        {
            missileSystem.Stop();
        }
    }
}
