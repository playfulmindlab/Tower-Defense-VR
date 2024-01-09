using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
    public LayerMask enemiesLayer;

    public Enemy target;
    public Transform towerPivot;

    public int towerCost = 100;
    public float damage;
    public float firerate;
    public float range;

    private IDamageMethod currentDamageMethodClass;
    private float delay;

    // Start is called before the first frame update
    void Start()
    {
        currentDamageMethodClass = GetComponent<IDamageMethod>();

        if (currentDamageMethodClass == null)
        {
            Debug.LogError("Tower " + this.gameObject.name + " has no damage class attached!");
        }
        else
        {
            currentDamageMethodClass.Init(damage, firerate);
        }

        delay = 1 / firerate;
    }

    // Update is called once per frame
    public void Tick()
    {
        currentDamageMethodClass.DamageTick(target);

        if (target != null)
        {
            Vector3 posDifference = target.transform.position - transform.position;
            posDifference.y = 0;
            if (towerPivot != null)
                towerPivot.transform.rotation = Quaternion.LookRotation(posDifference, Vector3.up);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, range);
        if (target != null && towerPivot != null)
        {
            Gizmos.DrawLine(towerPivot.position, target.transform.position);
        }
    }
}
