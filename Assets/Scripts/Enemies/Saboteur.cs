using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saboteur : Enemy
{
    [SerializeField] float empRadius = 5f;
    [SerializeField] Transform ringRangeTransform;

    string effectID = "";

    [SerializeField] float stunTowersTime = 15f;

    [SerializeField] float stunRate = 2f;
    float stunDelay = 2f;

    private void Start()
    {
        effectID = gameObject.name.ToString();

        ringRangeTransform.localScale = new Vector3(empRadius, ringRangeTransform.localScale.y, empRadius);

        stunDelay = 1 / stunRate;
    }

    public override void Tick()
    {
        //Debug.Log("SAB tick GOTHRU");

        //First, go through Stun Attack
        stunDelay -= Time.deltaTime;

        if (stunDelay <= 0)
        {
            StunAttack(null);
            stunDelay = 1 / stunRate;
        }

        base.Tick();
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

            Effect stunEffect = new Effect("Stun_" + effectID, stunTowersTime);

            AppliedTowerEffect effect = new AppliedTowerEffect(tower, stunEffect, "ShockStatus");
            TowerDefenseManager.EnqueueTowerEffectToApply(effect);
        }
    }
}
