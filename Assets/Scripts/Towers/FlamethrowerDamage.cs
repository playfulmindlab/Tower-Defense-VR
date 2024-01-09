using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerDamage : Damage
{
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] private Collider fireVolume;

    public override void DamageTick(Enemy target)
    {
        fireVolume.enabled = target != null;

        if (target)
        {
            if (!fireEffect.isPlaying) 
                fireEffect.Play();
            return;
        }

        fireEffect.Stop();
    }
}
