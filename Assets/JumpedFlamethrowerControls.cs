using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpedFlamethrowerControls : JumpedTowerControls
{
    FlamethrowerDamage flamethrowerDamage;

    private void Start()
    {
        flamethrowerDamage = GetComponent<FlamethrowerDamage>();
    }

    public override void ToggleAutoShoot()
    {
        base.ToggleAutoShoot();
        flamethrowerDamage.ToggleIsJumped();
    }


    public override void SetGunFire(bool firing)
    {
        flamethrowerDamage.ActivateGun(firing);
    }
}
