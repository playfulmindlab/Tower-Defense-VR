using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using LSL4Unity.Samples.SimpleInlet;

public enum ControlsSetting { Main, Jumped}
public enum JumpedType { Normal = 0, ReticleStatic = 1, ReticleFollow = 2}

public class GameControlManager : MonoBehaviour
{
    public ControlsSetting controlSetting;
    public static GameControlManager instance;

    InputActionManager inputActionManager;
    InputActionAsset normalInputAsset;
    InputActionAsset jumpedInputAsset;

    [SerializeField] Canvas jumpedTransitionCanvas;
    [SerializeField] Canvas jumpedOverlayWarning;
    [SerializeField] Canvas attackedBaseWarning;

    JumpedTowerUI towerUI;

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
    [SerializeField] public float cameraDamping = 5f;
    [SerializeField] GameObject tunnelVolume;
    //[SerializeField] public InputActionProperty attackButton, unjumpButton;

    SimpleInletBalanceBoard bbInlet;
    Animator jumpTransAnim;

    bool isJumped = false;
    public bool IsJumped { get { return isJumped; } set { } }


    public JumpedType jumpType = JumpedType.Normal;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
       // overlayWarning = GetComponentInChildren<Canvas>();
        rayLine = rightController.transform.Find("Ray Interactor").gameObject.GetComponent<LineRenderer>();
        pointerLine = rightController.transform.Find("PhysicsPointer").GetComponent<LineRenderer>();
        bbInlet = GetComponent<SimpleInletBalanceBoard>();
        jumpTransAnim = jumpedTransitionCanvas.gameObject.GetComponent<Animator>();

        inputActionManager = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<InputActionManager>();
        normalInputAsset = inputActionManager.actionAssets[0];
        jumpedInputAsset = inputActionManager.actionAssets[1];
        inputActionManager.actionAssets = new List<InputActionAsset> { normalInputAsset };

        towerUI = jumpedOverlayWarning.GetComponent<JumpedTowerUI>();
    }

    public void SwitchJumpType(int newType)
    {
        jumpType = (JumpedType)newType;
    }

    void ChangeTargetType()
    {
        if (jumpedTowerControls != null)
        {
            switch (jumpType)
            {
                case JumpedType.Normal:
                    TowerDefenseManager.instance.currTargetType = TargetType.First;
                    jumpedTowerControls.ToggleFollowEnemy(false);
                    break;

                case JumpedType.ReticleStatic:
                    TowerDefenseManager.instance.currTargetType = TargetType.First;
                    jumpedTowerControls.ToggleFollowEnemy(false);
                    break;

                case JumpedType.ReticleFollow:
                    TowerDefenseManager.instance.currTargetType = TargetType.Closest;
                    jumpedTowerControls.ToggleFollowEnemy(true);
                    break;
            }
        }
    }

    private void Update()
    {
        //TODO: try to move this to job system
        if (jumpedTowerControls != null)
        {
            if (jumpType == JumpedType.Normal)
                jumpedTowerControls.RotateGun(bbInlet.CoordValues3, cameraDamping);
            else
                jumpedTowerControls.RotateMissiles();
        }

        /*if (attackButton.action.WasPerformedThisFrame())
        {
            firing = !firing;
            jumpedTowerControls.SetGunFire(firing);
        }

        if (unjumpButton.action.WasPerformedThisFrame())
        {
            SwapControls(ControlsSetting.Main);
        }*/
    }

    void TogglePlayerCamera(bool cameraSetting)
    {
        mainCamera.enabled = cameraSetting;
        mainCamera.GetComponent<AudioListener>().enabled = cameraSetting;

        if (cameraSetting)
            mainCamera.tag = "MainCamera";
        else
            mainCamera.tag = "Untagged";
    }

    public void SwapControls(ControlsSetting newControlSetting)
    {
        //firing = false;

        switch (newControlSetting)
        {
            case ControlsSetting.Main:
                jumpedTowerControls.ForceGun(false);
                jumpedTowerControls.ToggleAutoShoot();

                jumpedTowerControls.SetCamera(false);
                TogglePlayerCamera(true);

                rayLine.enabled = true;
                pointerLine.enabled = true;

                AudioManager.instance.PlaySFXArray("TowerUnjump", towerViewCanvas.transform.position);
                attackedBaseWarning.worldCamera = mainCamera;
                jumpedTransitionCanvas.worldCamera = mainCamera;
                jumpedOverlayWarning.enabled = false;

                jumpedTowerControls.ToggleFollowEnemy(true);
                jumpedTowerControls.ResetHeadRotation();
                towerUI.ResetReticle();


                isJumped = false;
                GameManager.instance.LogNewEvent("Tower Jump End", jumpedTowerControls.gameObject, jumpedTowerControls.gameObject.transform.position, isJumped);
                jumpedTowerControls = null;

                break;

            case ControlsSetting.Jumped:
                jumpedTowerControls.ToggleAutoShoot();
                jumpedTowerControls.SetJumpedTower();
                jumpedTowerControls.AssignNewTowerUI(towerUI);

                TogglePlayerCamera(false);
                jumpedTowerControls.SetCamera(true);

                rayLine.enabled = false;
                pointerLine.enabled = false;

                AudioManager.instance.PlaySFXArray("TowerJump", towerViewCanvas.transform.position);
                attackedBaseWarning.worldCamera = jumpedTowerControls.TowerCamera;
                jumpedOverlayWarning.worldCamera = jumpedTowerControls.TowerCamera;
                jumpedTransitionCanvas.worldCamera = jumpedTowerControls.TowerCamera;
                jumpedOverlayWarning.enabled = true;

                tunnelVolume.transform.position = jumpedTowerControls.gameObject.transform.position;
                isJumped = true;
                //ChangeTargetType();
                //towerUI.ResetReticle();
                GameManager.instance.LogNewEvent("Tower Jumped", jumpedTowerControls.gameObject, jumpedTowerControls.gameObject.transform.position, isJumped);
                //DataEvent newEvent2 = new DataEvent("Tower Jumped", jumpedTowerControls.gameObject, jumpedTowerControls.gameObject.transform.position, isJumped);
                //EventManager.instance.RecordNewEvent(newEvent2);

                break;
        }
    }

    public void SwapToJumpedControls(JumpedTowerControls jumpedTower)
    {
        jumpedTowerControls = jumpedTower;

        jumpTransAnim.SetTrigger("Jump");
    }

    public void SwapToUnjumpedControls()
    {
        jumpTransAnim.SetTrigger("Unjump");
    }

    public void SwapControls(string newControlString)
    {
        ControlsSetting newSetting = (ControlsSetting)System.Enum.Parse(typeof(ControlsSetting), newControlString);
        if (System.Enum.IsDefined(typeof(ControlsSetting), newSetting))
            SwapControls(newSetting);
    }
}
