using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Hands.Samples.GestureSample;

public class JumpedTowerControls : MonoBehaviour
{
    [SerializeField] TowerBehaviour towerBehaviour;

    [SerializeField] protected Camera towerCamera;
    public Camera TowerCamera { get { return towerCamera; } set { } }

    AudioListener cameraListener;

    protected Transform towerHead;
    public Transform towerTip;
    MissileDamage missileDamage;

    protected float towerRotation = 0f;
    StaticHandGesturesTowerJump[] gestureScripts;

    protected bool jumpStatus = false;
    public bool JumpStatus { get { return jumpStatus; } }

    public JumpedTowerUI towerUI;
    public RectTransform towerUITransform;

    [SerializeField] float jumpDamageMult = 2f;

    public void Awake()
    {
        towerHead = towerBehaviour.towerPivot;

        cameraListener = towerCamera.GetComponent<AudioListener>();
        if (cameraListener.enabled) cameraListener.enabled = false;

        gestureScripts = GetComponents<StaticHandGesturesTowerJump>();
        Debug.Log("GESTURES: " + gestureScripts.Length);

        foreach (StaticHandGesturesTowerJump s in gestureScripts)
        {
            s.enabled = false;
        }

        missileDamage = GetComponent<MissileDamage>();
        missileDamage.SetJumpMultiplier(jumpDamageMult);
    }

    public void AssignNewTowerUI(JumpedTowerUI newUI)
    {
        towerUI = newUI;
        towerUITransform = towerUI.GetComponent<RectTransform>();
    }

    public virtual void SetCamera(bool camActive)
    {
        towerCamera.transform.gameObject.SetActive(camActive);
        cameraListener.enabled = camActive;

        if (camActive) 
            towerCamera.transform.gameObject.tag = "MainCamera";
        else
            towerCamera.transform.gameObject.tag = "Untagged";
    }

    public void SetJumpedTower()
    {
        //if (TowerDefenseManager.CurrPhase == Phase.Defend || TowerDefenseManager.CurrPhase == Phase.Defend_ChooseJump)
        {
            foreach(StaticHandGesturesTowerJump s in gestureScripts)
            {
                s.enabled = true;
            }

            //ToggleFollowEnemy(false);
        }
    }

    public virtual void ToggleAutoShoot()
    {
        towerBehaviour.canFire = !towerBehaviour.canFire;
    }

    public virtual void ToggleFollowEnemy(bool newFollow)
    {
        towerBehaviour.followEnemy = newFollow;
    }

    public virtual void RotateGun(Vector2 balanceBoardCoords, float dampVal = 5f)
    {
        Quaternion rot = Quaternion.Euler(new Vector3((balanceBoardCoords.y - towerRotation) * 2f, balanceBoardCoords.x * 2, 0f));

        //towerHead.localEulerAngles = new Vector3(-balanceBoardCoords.y * 2f, -balanceBoardCoords.x * 2, 0f);
        towerHead.localRotation = Quaternion.Slerp(towerHead.localRotation, rot, Time.deltaTime * dampVal);
    }

    public virtual void RotateMissiles()
    {
        if (towerUI != null)
        {
            //towerUITransform.rotation = Quaternion.LookRotation((towerCamera.transform.position - towerUITransform.position).normalized);

            Debug.Log("Reticle Check");
            towerTip.LookAt(towerUI.ReticleTransform, Vector3.up);
        }
    }


    public virtual void EndTowerJump()
    {
        foreach (StaticHandGesturesTowerJump s in gestureScripts)
        {
            s.enabled = false;
        }
        //if (GameControlManager.instance.jumpType == JumpedType.Normal)
        //    ToggleFollowEnemy(true);

        GameControlManager.instance.SwapToUnjumpedControls();
    }

    public virtual void SetGunFire()
    {
        jumpStatus = !jumpStatus;

        missileDamage.ActivateGun(jumpStatus);

        if (jumpStatus == false && towerTip != null)
            towerTip.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));


    }

    public virtual void ResetHeadRotation()
    {
        towerHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    public virtual void ForceGun(bool newState)
    {
        jumpStatus = newState;
        missileDamage.ActivateGun(newState);
    }
}
