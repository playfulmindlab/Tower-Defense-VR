using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingTower : TowerBehaviour
{
    [SerializeField] float rotateSpeed;

    // Start is called before the first frame update
    protected override void Start()
    {
        firerate /= 5f;
        base.Start();
    }

    public override void Tick()
    {
        if (healthBar != null && shieldBar != null)
        {
            healthBar.value = health;
            shieldBar.value = shield;
        }

        if (towerPivot != null)
            towerPivot.transform.Rotate(Vector3.up * Time.deltaTime * rotateSpeed * 5f);

        if (delay > 0f)
        {
            delay -= Time.deltaTime;
            return;
        }

        Collider[] towerColliders = Physics.OverlapSphere(transform.position, range, targetLayer);
        Debug.Log("Check: " + towerColliders.Length);

        foreach (Collider col in towerColliders)
        {
            TowerBehaviour tower = col.GetComponent<TowerBehaviour>();
            if (tower != null && tower != this)
            {
                tower.Heal(damage);
            }
        }

        delay = 1 / firerate;

    }
}
