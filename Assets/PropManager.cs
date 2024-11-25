using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropManager : MonoBehaviour
{

    //[SerializeField] bool aliveOnStartup = false;
    public LineRenderer line;
    public GameObject towerSpawn;

    public MiniMapTowerPlacement miniMapScript;

    public GameObject upgradedProp;

    [SerializeField] Canvas radialMenuCanvas;
    protected UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable xrGrab;
    protected TowerBehaviour towerScript;
    protected JumpedTowerControls jumpedTowerScript;

    public TowerBehaviour upgradedTowerRef;

    LayerMask baseLayer; //= LayerMask.GetMask("Baseplate");
    LayerMask pathLayer;

    protected bool isPropDropped = false;
    bool hasStarted = false;

    Outline propOutline;
    public void TogglePropOutline(bool isActive) { propOutline.OutlineColor = Color.red; if (isActive) propOutline.ChangeOutlineWidth(3); else propOutline.ChangeOutlineWidth(0); }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (hasStarted == false)
        {
            baseLayer = LayerMask.NameToLayer("Baseplate");
            pathLayer = LayerMask.NameToLayer("Path");
            miniMapScript = GameObject.FindGameObjectWithTag("MinimapBaseplate").GetComponentInParent<MiniMapTowerPlacement>();
            xrGrab = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
            propOutline = GetComponentInChildren<Outline>();
            TogglePropOutline(false);

            if (upgradedProp != null && upgradedProp.GetComponent<PropManager>().towerSpawn != null)
            {
                upgradedTowerRef = upgradedProp.GetComponent<PropManager>().towerSpawn.GetComponent<TowerBehaviour>();
            }

            radialMenuCanvas.enabled = false;

            hasStarted = true;
        }
    }

    public void ForceStart() { Start(); }

    public void TowerDropped()
    {
        if (isPropDropped == false)
        {
            Vector3 dir = (line.GetPosition(1) - line.GetPosition(0)).normalized;

            RaycastHit mainRayHit; 
            RaycastHit[] rayHits = new RaycastHit[4];

            Physics.Raycast(line.GetPosition(0), dir, out mainRayHit, 40f);
            Physics.Raycast(line.GetPosition(0) + (Vector3.left * .005f), dir, out rayHits[0], 40f);
            Physics.Raycast(line.GetPosition(0) + (Vector3.back * .005f), dir, out rayHits[1], 40f);
            Physics.Raycast(line.GetPosition(0) + (Vector3.right * .005f), dir, out rayHits[2], 40f);
            Physics.Raycast(line.GetPosition(0) + (Vector3.forward * .005f), dir, out rayHits[3], 40f);

            if (rayHits[0].collider == null || rayHits[1].collider == null ||
                rayHits[2].collider == null || rayHits[3].collider == null)
            {
                Destroy(gameObject);
                return;
            }

            //Debug.Log("DROP OUT: " + hit.collider.gameObject.name + " @ " + hit.collider.gameObject.layer.ToString());
            //Check to see if it intersects Baseplate & ONLY baseplate
            if (gameObject.tag == "Tower" && CheckRaycastOnLayer(rayHits, baseLayer))
            {
                towerSpawn = miniMapScript.DropNewProp(this.gameObject, towerSpawn, mainRayHit.point);

                towerScript = towerSpawn.GetComponent<TowerBehaviour>();
                jumpedTowerScript = towerSpawn.GetComponent<JumpedTowerControls>();

                isPropDropped = true;
            }
            else if ((gameObject.tag == "Obstacle" || gameObject.tag == "AttackObstacle") &&
                        CheckRaycastOnLayer(rayHits, pathLayer))//hit.collider.gameObject.layer == pathLayer)
            {
                towerSpawn = miniMapScript.DropNewProp(this.gameObject, towerSpawn, mainRayHit.point);

                towerScript = towerSpawn.GetComponent<TowerBehaviour>();
                jumpedTowerScript = towerSpawn.GetComponent<JumpedTowerControls>();

                isPropDropped = true;
            }
            else
            {
                Destroy(gameObject);
                miniMapScript.ResetRadiusDecal();
            }
        }
    }

    bool CheckRaycastOnLayer(RaycastHit[] rayHits, LayerMask findLayer)
    {
        bool hitValid = true;

        foreach(RaycastHit hit in rayHits)
        {
            Debug.Log("RAYHIT: " + hit.point + " // " + hit.collider.gameObject.layer);
            if (hit.collider.gameObject.layer != findLayer)
            {
                hitValid = false;
                break;
            }
        }

        return hitValid;
    }

    public void LockPropPosition()
    {
        if (xrGrab == null) Start();

        xrGrab.trackPosition = false;
        xrGrab.trackRotation = false;
    }

    public void ToggleRadialMenu(bool toggleState)
    {
        radialMenuCanvas.enabled = toggleState;

        if (towerScript != null && propOutline != null && towerScript.outline != null)
        {
            towerScript.ToggleOutline(toggleState);
            TogglePropOutline(toggleState);
        }

        if (miniMapScript != null && toggleState == true)
        {
            miniMapScript.SwapActivatedPropMenu(this);
        }
    }

    public void SpawnUpgradedProp(TowerBehaviour upgradedTowerScript)
    {
        towerScript = upgradedTowerScript.GetComponent<TowerBehaviour>();
        jumpedTowerScript = upgradedTowerScript.GetComponent<JumpedTowerControls>();
        isPropDropped = true;
    }

    public void JumpTower()
    {
        GameControlManager.instance.SwapToJumpedControls(jumpedTowerScript);
    }

    public void UpgradePropAndTower()
    {
        towerScript.UpgradeTower(miniMapScript, this);
    }

    public void DeletePropAndTower()
    {
        miniMapScript.DeleteTower(this, towerScript);
    }

    public void CheckIfTowerDestroyed()
    {
        if (towerScript == null)
            DeletePropAndTower();
    }
}
