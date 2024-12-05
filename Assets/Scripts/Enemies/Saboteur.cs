using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saboteur : Enemy
{
    [SerializeField] float empRadius = 5f;

    string effectID = "";

    [SerializeField] float stopTowersTime = 15f;

    [SerializeField] float stunRate = 2f;
    float stunDelay = 2f;

    private void Start()
    {
        effectID = gameObject.name.ToString();

        stunDelay = 1 / stunRate;
    }

    public override void Tick()
    {
        //Attack Obstacle
        //Debug.Log("SAB tick GOTHRU");

        //First, go through Stun Attack
        stunDelay -= Time.deltaTime;

        if (stunDelay <= 0)
        {
            StunAttack(null);
            stunDelay = 1 / stunRate;
        }

        base.Tick();
/*
        if (attackingTower != null)
        {
            attackDelay -= Time.deltaTime;
            if (attackDelay <= 0)
            {
                Attack(attackingTower);
                attackDelay = 1 / attackRate;
            }
        }
        else if (speed == 0 && speedAffected == false && attackingTower == null)
        {
            ChangeTowerTarget(null);
            //Speed = origSpeed;
        }

        */


    }  

    public void StunAttack(TowerBehaviour attackedObject)
    {
        Collider[] towerColliders = Physics.OverlapSphere(transform.position, empRadius, 1 << LayerMask.NameToLayer("Towers"));

        //Debug.Log("SAB ATK GOTHRU : " + transform.position + " // " + towerColliders.Length + "ON LAYER " + LayerMask.NameToLayer("Towers"));

        foreach (Collider t in towerColliders)
        {
            TowerBehaviour tower = t.GetComponent<TowerBehaviour>();
            //Freeze the towers here
            Debug.Log("GRABBED TOWER: " + t.gameObject.name);
            //t.GetComponent<TowerBehaviour>().TowerDie();

            Effect stunEffect = new Effect("Stun_" + effectID, stopTowersTime);

            AppliedTowerEffect effect = new AppliedTowerEffect(tower, stunEffect, "ShockStatus");
            TowerDefenseManager.EnqueueTowerEffectToApply(effect);
        }
        /*if (attackedObject != null)
        {
            if (anim != null)
            {
                anim.SetInteger("AttackIndex", Random.Range(0, 3));
                anim.SetTrigger("Attack");
            }

            //AudioManager.instance.PlaySFXRandom("EnemyAttack", transform.position, 3);
            attackedObject.Damage(attack);
        }*/
    }
}
