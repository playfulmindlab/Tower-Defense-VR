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

    [SerializeField] Canvas jumpedOverlayWarning;
    [SerializeField] Canvas attackedBaseWarning;

    [Header("Main Game Controls")]
    //[SerializeField] GameObject moveControls;
    //[SerializeField] GameObject turnControls;
    [SerializeField] Camera mainCamera;
    [SerializeField] Canvas towerSpawnCanvas;
    [SerializeField] GameObject rightController;
    LineRenderer pointerLine;
    LineRenderer rayLine;

    [Header("Jumping Controls")]
    [SerializeField] Canvas towerViewCanvas;
    [SerializeField] JumpedTowerControls jumpedTowerControls;
    [SerializeField] float cameraDamping = 5f;
    [SerializeField] public InputActionProperty attackButton, unjumpButton;

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
       // overlayWarning = GetComponentInChildren<Canvas>();
        rayLine = rightController.transform.Find("Ray Interactor").gameObject.GetComponent<LineRenderer>();
        pointerLine = rightController.transform.Find("PhysicsPointer").GetComponent<LineRenderer>();
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

            if (unjumpButton.action.WasPerformedThisFrame())
            {
                SwapControls(ControlsSetting.Main);
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
                jumpedTowerControls.EndTowerJump();
                rayLine.enabled = true;
                pointerLine.enabled = true;

                jumpedTowerControls = null;
                AudioManager.instance.PlaySFXArray("TowerUnjump", towerViewCanvas.transform.position);
                attackedBaseWarning.worldCamera = mainCamera;
                jumpedOverlayWarning.enabled = false;
                break;

            case ControlsSetting.Jumped:
                //moveControls.SetActive(false);
                //towerViewCanvas.gameObject.SetActive(true);
                jumpedTowerControls.ToggleAutoShoot();

                TogglePlayerCamera(false);
                jumpedTowerControls.SetCamera(true);
                rayLine.enabled = false;
                pointerLine.enabled = false;

                AudioManager.instance.PlaySFXArray("TowerJump", towerViewCanvas.transform.position);
                attackedBaseWarning.worldCamera = jumpedTowerControls.TowerCamera;
                jumpedOverlayWarning.worldCamera = jumpedTowerControls.TowerCamera;
                jumpedOverlayWarning.enabled = true;
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
