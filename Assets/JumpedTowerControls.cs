using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.XR;
using Unity.XR.CoreUtils;

public class JumpedTowerControls : MonoBehaviour
{
    [SerializeField] TowerBehaviour towerBehaviour;

    [SerializeField] Camera towerCamera;
    AudioListener cameraListener;

    //[SerializeField] GameObject playerParentHolder;
    Transform towerHead;
    MissileDamage missileDamage;

    private float towerRotation = 0f;

    public void Awake()
    {
        towerHead = towerBehaviour.towerPivot;

        cameraListener = towerCamera.GetComponent<AudioListener>();
        if (cameraListener.enabled) cameraListener.enabled = false;

        missileDamage = GetComponent<MissileDamage>();
    }

    public void SetCamera(bool camActive)
    {
        towerCamera.transform.gameObject.SetActive(camActive);
        cameraListener.enabled = camActive;
    }

    public void SetJumpedTower()
    {
        //if (TowerDefenseManager.CurrPhase == Phase.Defend || TowerDefenseManager.CurrPhase == Phase.Defend_ChooseJump)
        {
            towerRotation = towerBehaviour.gameObject.transform.localEulerAngles.x;
            GameControlManager.instance.SwapToJumpedControls(this);
        }
    }

    public void ToggleAutoShoot()
    {
        towerBehaviour.canFire = !towerBehaviour.canFire;
    }

    public void RotateGun(Quaternion rot)
    {
        towerHead.rotation = rot;// * Quaternion.Euler(0, 270, 0);
    }

    public void RotateGun(Vector2 balanceBoardCoords, float dampVal = 5f)
    {
        Quaternion rot = Quaternion.Euler(new Vector3((balanceBoardCoords.y - towerRotation) * 2f, -balanceBoardCoords.x * 2, 0f));

        //towerHead.localEulerAngles = new Vector3(-balanceBoardCoords.y * 2f, -balanceBoardCoords.x * 2, 0f);
        towerHead.localRotation = Quaternion.Slerp(towerHead.localRotation, rot, Time.deltaTime * dampVal);
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
