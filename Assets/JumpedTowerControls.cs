using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpedTowerControls : MonoBehaviour
{
    [SerializeField] TowerBehaviour towerBehaviour;
    [SerializeField] Camera towerCamera;
    Transform towerHead;
    MissileDamage missileDamage;

    public void Awake()
    {
        towerHead = towerBehaviour.towerPivot;
        missileDamage = GetComponent<MissileDamage>();
    }

    public void SetCamera(bool camActive)
    {
        towerCamera.transform.gameObject.SetActive(camActive);
    }

    public void SetJumpedTower()
    {
        GameControlManager.instance.SwapToJumpedControls(this);
    }

    public void ToggleAutoShoot()
    {
        towerBehaviour.canFire = !towerBehaviour.canFire;
    }

    public void RotateGun(Quaternion rot)
    {
        towerHead.rotation = rot;// * Quaternion.Euler(0, 270, 0);
    }

    public void SetGunFire(bool firing)
    {
        missileDamage.ActivateGun(firing);
    }
}
