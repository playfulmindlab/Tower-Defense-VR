using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerTower : TowerBehaviour
{
    [SerializeField] float rotateSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }
}
