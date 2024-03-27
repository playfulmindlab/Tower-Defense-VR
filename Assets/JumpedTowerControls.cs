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

    public void RotateGun(Vector2 balanceBoardCoords)
    {
        //Quaternion rot;
        //rot = Quaternion.Euler(new Vector3(balanceBoardCoords.x, balanceBoardCoords.y, 0f));

       // towerHead.localEulerAngles = new Vector3(balanceBoardCoords.x * 2f, balanceBoardCoords.y * 2, 0f);

        towerHead.localEulerAngles = new Vector3(-balanceBoardCoords.y * 2f, -balanceBoardCoords.x * 2, 0f);
    }

    public void RotateGun(float balanceX, float balanceY, float magnitude)
    {
        Quaternion rot;

        //magnitude += 1;
        rot = Quaternion.Euler(new Vector3(balanceX * 2f, balanceY * 2f, 0f));

        towerHead.rotation = rot;
    }

    public void SetGunFire(bool firing)
    {
        missileDamage.ActivateGun(firing);
    }
}
