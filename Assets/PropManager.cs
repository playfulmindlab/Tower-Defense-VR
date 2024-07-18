using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropManager : MonoBehaviour
{
    public LineRenderer line;
    public GameObject towerSpawn;

    public MiniMapTowerPlacement miniMapScript;

    public GameObject upgradedProp;

    [SerializeField] Canvas radialMenuCanvas;
    UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable xrGrab;
    TowerBehaviour towerScript;
    JumpedTowerControls jumpedTowerScript;

    LayerMask baseLayer; //= LayerMask.GetMask("Baseplate");
    LayerMask pathLayer;

    bool isPropDropped = false;

    // Start is called before the first frame update
    void Start()
    {
        baseLayer = LayerMask.NameToLayer("Baseplate");
        pathLayer = LayerMask.NameToLayer("Path");
        miniMapScript = GameObject.FindGameObjectWithTag("MinimapBaseplate").GetComponentInParent<MiniMapTowerPlacement>();
        xrGrab = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();

        radialMenuCanvas.enabled = false;
    }

    public void TowerDropped()
    {
        if (isPropDropped == false)
        {
            Vector3 dir = (line.GetPosition(1) - line.GetPosition(0)).normalized;

            RaycastHit hit;

            Physics.Raycast(line.GetPosition(0), dir, out hit, 40f);
            if (hit.collider == null)
            {
                Destroy(gameObject);
                return;
            }

            //Check to see if it intersects Baseplate & ONLY baseplate
            if (gameObject.tag == "Tower" && hit.collider.gameObject.layer == baseLayer)
            {
                //Debug.Log(this.gameObject);
                //Debug.Log(towerSpawn);
                //Debug.Log(hit.point);
                towerSpawn = miniMapScript.DropNewProp(this.gameObject, towerSpawn, hit.point);

                towerScript = towerSpawn.GetComponent<TowerBehaviour>();
                jumpedTowerScript = towerSpawn.GetComponent<JumpedTowerControls>();

                isPropDropped = true;
            }
            else if ((gameObject.tag == "Obstacle" || gameObject.tag == "AttackObstacle") &&
                        hit.collider.gameObject.layer == pathLayer)
            {
                towerSpawn = miniMapScript.DropNewProp(this.gameObject, towerSpawn, hit.point);

                towerScript = towerSpawn.GetComponent<TowerBehaviour>();
                jumpedTowerScript = towerSpawn.GetComponent<JumpedTowerControls>();

                isPropDropped = true;
            }
            else
            {
                Destroy(gameObject);
            }
        }
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
        if (miniMapScript != null && toggleState == true)
            miniMapScript.SwapActivatedPropMenu(this);
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
        //jumpedTowerScript.SetJumpedTower();
    }

    public void UpgradePropAndTower()
    {
        //Debug.Log("Went Thru Upgrade");

        //everything in this script just changes the BIG towers, not the prop towers!!!
        towerScript.UpgradeTower(miniMapScript, this);
    }

    public void DeletePropAndTower()
    {
        //Debug.Log("DeletingTower");
        miniMapScript.DeleteTower(this, towerScript);

    }
}
