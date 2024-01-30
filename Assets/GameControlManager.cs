using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

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

    bool firing = false;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Update()
    {
        //TODO: try to move this to job system
        if (jumpedTowerControls != null)
        {
            if (rotateJoystick.action != null)
            {
                jumpedTowerControls.RotateGun(rotateJoystick.action.ReadValue<Quaternion>());
            }

            if (attackButton.action.WasPerformedThisFrame())
            {
                Debug.Log("goin thru");
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
