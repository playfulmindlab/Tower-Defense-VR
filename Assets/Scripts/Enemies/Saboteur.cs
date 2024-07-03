using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saboteur : Enemy
{
    [SerializeField] float empRadius = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Attack(TowerBehaviour attackedObject)
    {
        Collider[] towerColliders = Physics.OverlapSphere(transform.position, empRadius, 1 << LayerMask.NameToLayer("Tower"));

        foreach (Collider t in towerColliders)
        {
            //Freeze the towers here
            t.GetComponent<TowerBehaviour>().TowerDie();
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
