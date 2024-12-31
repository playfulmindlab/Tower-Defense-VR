using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageMethod
{
    public void DamageTick(Enemy target);
    public void Init(float damage, float firerate);
    public void UpdateDamage(float newDamage);
}

public class Damage : MonoBehaviour, IDamageMethod
{
    //public bool canFire = true;
    [SerializeField] protected ElementType attackType;

    protected private float damage;
    protected private float firerate;
    protected private float delay;

    public ElementType GetAttackType { get { return attackType; } set { } }
    public float DamageValue { get { return damage; } set { } }
    public float FirerateValue { get { return firerate; } set { } }

    public virtual void Init(float damage, float firerate)
    {
        this.damage = damage;
        this.firerate = firerate;
        delay = 1f / firerate;
    }
    public virtual void DamageTick(Enemy target)
    {
        if (target)
        {
            if (delay > 0f)
            {
                delay -= Time.deltaTime;
                return;
            }

            TowerDefenseManager.EnqueueDamageData(new EnemyDamage(target, damage, target.GetResistanceModifier(attackType)));

            delay = 1f / firerate;
        }
    }

    public void UpdateDamage(float newDamage)
    {
        damage = newDamage;
    }

}
