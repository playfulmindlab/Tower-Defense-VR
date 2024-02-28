using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeTower : TowerBehaviour
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

    }

    public override void Tick()
    {
        if (healthBar != null && shieldBar != null)
        {
            healthBar.value = health;
            shieldBar.value = shield;
        }
    }
}
