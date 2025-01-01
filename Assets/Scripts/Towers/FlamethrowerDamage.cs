using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerDamage : MissileDamage
{
    //[SerializeField] private ParticleSystem fireEffect;
    [SerializeField] private BoxCollider fireVolume;
    bool isJumped = false;
    bool isFiring = false;

    TowerBehaviour towerBehaviour;

    public override void Init(float damage, float firerate)
    {
        audioSource = GetComponent<AudioSource>();
        missileSystemMain = missileSystem.main;

        this.damage = damage;
        this.firerate = firerate;

        towerBehaviour = GetComponent<TowerBehaviour>();
        fireVolume.transform.localPosition = (Vector3.left * (towerBehaviour.range / 2f)) / 10f;
        fireVolume.center = (Vector3.right * (towerBehaviour.range / 4f)) / 10f;
        fireVolume.size = new Vector3((towerBehaviour.range / 2f) / 10f, 0.1f, 0.1f);

        missileSystemMain.startSpeed = towerBehaviour.range / 2f;
        //delay = 1f / firerate;
    }

    public void ToggleIsJumped()
    {
        isJumped = !isJumped;

        if (isJumped == true)
            missileSystem.Clear();
        else
            fireVolume.enabled = true;
    }

    public override void ActivateGun(bool activeState)
    {
        if (activeState == true)
        {
            audioSource.loop = true;
            //missileSystemMain.loop = true;
            UpdateDamage(damage * jumpDamageMultiplier);
            fireVolume.enabled = true;
            isFiring = true;
            missileSystem.Play();
        }
        else
        {
            missileSystem.Stop();
            audioSource.loop = false;
            //missileSystemMain.loop = false;
            fireVolume.enabled = false;
            isFiring = false;
            UpdateDamage(baseDamageValue);
        }
    }

    public override void DamageTick(Enemy target)
    {
        //fireVolume.enabled = target != null;
        if ((target && !isJumped) || (isJumped && isFiring))
        {
            if (!missileSystem.isPlaying)
                missileSystem.Play();
            return;
        }

        //fireEffect.Stop();
    }
}
