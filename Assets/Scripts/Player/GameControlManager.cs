using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using LSL4Unity.Samples.SimpleInlet;

public enum ControlsSetting { Main, Jumped}

public class GameControlManager : MonoBehaviour
{
    public ControlsSetting controlSetting;
    public static GameControlManager instance;

    InputActionManager inputActionManager;
    InputActionAsset normalInputAsset;
    InputActionAsset jumpedInputAsset;

    [Header("Main Game Controls")]
    [SerializeField] GameObject moveControls;
    [SerializeField] GameObject turnControls;
    [SerializeField] Camera mainCamera;
    [SerializeField] Canvas towerSpawnCanvas;

    [Header("Jumping Controls")]
    [SerializeField] Canvas towerViewCanvas;
    [SerializeField] JumpedTowerControls jumpedTowerControls;
    [SerializeField] float cameraDamping = 5f;
    [SerializeField] public InputActionProperty /*rotateJoystick,*/ attackButton;

    SimpleInletBalanceBoard bbInlet;

    bool firing = false;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        mainCamera = Camera.main;
        bbInlet = GetComponent<SimpleInletBalanceBoard>();

        inputActionManager = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<InputActionManager>();
        normalInputAsset = inputActionManager.actionAssets[0];
        jumpedInputAsset = inputActionManager.actionAssets[1];
        inputActionManager.actionAssets = new List<InputActionAsset> { normalInputAsset };

    }

    private void Update()
    {
        //TODO: try to move this to job system
        if (jumpedTowerControls != null)
        {
            jumpedTowerControls.RotateGun(bbInlet.rotationValues, cameraDamping);

            if (attackButton.action.WasPerformedThisFrame())
            {
                firing = !firing;
                jumpedTowerControls.SetGunFire(firing);
            }
        }
    }

    void TogglePlayerCamera(bool cameraSetting)
    {
        mainCamera.enabled = cameraSetting;
        mainCamera.GetComponent<AudioListener>().enabled = cameraSetting;
    }

    public void SwapControls(ControlsSetting newControlSetting)
    {
        firing = false;

        switch (newControlSetting)
        {
            case ControlsSetting.Main:
                //moveControls.SetActive(true);
                jumpedTowerControls.SetGunFire(false);
                //towerViewCanvas.gameObject.SetActive(false);
                jumpedTowerControls.ToggleAutoShoot();

                jumpedTowerControls.SetCamera(false);
                TogglePlayerCamera(true);
                //inputActionManager

                jumpedTowerControls = null;
                //mainCamera.is
                AudioManager.instance.PlaySFXArray("TowerUnjump", towerViewCanvas.transform.position);
                break;

            case ControlsSetting.Jumped:
                //moveControls.SetActive(false);
                //towerViewCanvas.gameObject.SetActive(true);
                jumpedTowerControls.ToggleAutoShoot();

                TogglePlayerCamera(false);
                jumpedTowerControls.SetCamera(true);

                AudioManager.instance.PlaySFXArray("TowerJump", towerViewCanvas.transform.position);
                break;
        }
    }

    public void SwapToJumpedControls(JumpedTowerControls jumpedTower)
    {
        jumpedTowerControls = jumpedTower;

        SwapControls(ControlsSetting.Jumped);
    }

    public void SwapControls(string newControlString)
    {
        ControlsSetting newSetting = (ControlsSetting)System.Enum.Parse(typeof(ControlsSetting), newControlString);
        if (System.Enum.IsDefined(typeof(ControlsSetting), newSetting))
            SwapControls(newSetting);
    }
}
