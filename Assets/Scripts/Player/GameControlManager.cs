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

    [Header("Main Game Controls")]
    [SerializeField] GameObject moveControls;
    [SerializeField] GameObject turnControls;
    [SerializeField] Canvas towerSpawnCanvas;

    [Header("Jumping Controls")]
    [SerializeField] Canvas towerViewCanvas;
    [SerializeField] JumpedTowerControls jumpedTowerControls;
    [SerializeField] public InputActionProperty rotateJoystick, attackButton;

    SimpleInletBalanceBoard bbInlet;

    bool firing = false;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);


        bbInlet = GetComponent<SimpleInletBalanceBoard>();
    }

    private void Update()
    {
        //TODO: try to move this to job system
        if (jumpedTowerControls != null)
        {

            //if (rotateJoystick.action != null)
            //if (bbInlet.IsReady)
            //{
                //jumpedTowerControls.RotateGun(rotateJoystick.action.ReadValue<Quaternion>());
                jumpedTowerControls.RotateGun(bbInlet.rotationValues);
            //}

            if (attackButton.action.WasPerformedThisFrame())
            {
                firing = !firing;
                jumpedTowerControls.SetGunFire(firing);
            }
        }
    }

    public void SwapControls(ControlsSetting newControlSetting)
    {
        switch (newControlSetting)
        {
            case ControlsSetting.Main:
                moveControls.SetActive(true);
                towerViewCanvas.gameObject.SetActive(false);
                jumpedTowerControls.ToggleAutoShoot();
                jumpedTowerControls.SetCamera(false);
                jumpedTowerControls = null;
                break;

            case ControlsSetting.Jumped:
                moveControls.SetActive(false);
                towerViewCanvas.gameObject.SetActive(true);
                jumpedTowerControls.ToggleAutoShoot();
                jumpedTowerControls.SetCamera(true);
                firing = false;
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
