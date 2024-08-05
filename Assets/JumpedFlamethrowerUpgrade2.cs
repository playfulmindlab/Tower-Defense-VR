using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpedFlamethrowerUpgrade2 : JumpedTowerControls
{
    [SerializeField] FlamethrowerDamage flamethrowerDamage1;
    [SerializeField] FlamethrowerDamage flamethrowerDamage2;

    private void Start()
    {
        //flamethrowerDamage = GetComponent<FlamethrowerDamage>();
    }

    public override void ToggleAutoShoot()
    {
        base.ToggleAutoShoot();
        flamethrowerDamage1.ToggleIsJumped();
        flamethrowerDamage2.ToggleIsJumped();
    }


    public override void SetGunFire()
    {
        jumpStatus = !jumpStatus;
        flamethrowerDamage1.ActivateGun(jumpStatus);
        flamethrowerDamage2.ActivateGun(jumpStatus);
    }

    public override void ForceGun(bool newState)
    {
        jumpStatus = newState;
        flamethrowerDamage1.ActivateGun(newState);
        flamethrowerDamage2.ActivateGun(newState);
    }
}
