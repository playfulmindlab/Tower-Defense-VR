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

    bool isPropDropped = false;

    // Start is called before the first frame update
    void Start()
    {
        baseLayer = LayerMask.GetMask("Baseplate");
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
            //Physics.Raycast(line.GetPosition(0), dir, out hit, 40f, baseLayer);

            //if (Physics.Raycast(line.GetPosition(0), dir, out hit, 40f, baseLayer)) 
            //    Debug.Log("Detect Layer: " + hit.collider.gameObject.layer);

            //Check to see if it intersects Baseplate & ONLY baseplate
            if (Physics.Raycast(line.GetPosition(0), dir, out hit, 40f, baseLayer))
            {
                print("READING");

                //Physics.Raycast(line.GetPosition(0), dir, out hit, 40f, baseLayer);
                if (hit.collider != null)
                {
                    towerSpawn = miniMapScript.DropNewProp(this.gameObject, towerSpawn, hit.point);

                    towerScript = towerSpawn.GetComponent<TowerBehaviour>();
                    jumpedTowerScript = towerSpawn.GetComponent<JumpedTowerControls>();

                    isPropDropped = true;
                    //Debug.Log("World 2: " + this.transform.position + " // Local 2: " + this.transform.localPosition);
                }
            }
            else
            {
                Destroy(gameObject);
                print("NOTHING");
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
        Debug.Log("Went Thru Upgrade");

        //everything in this script just changes the BIG towers, not the prop towers!!!
        towerScript.UpgradeTower(miniMapScript, this);
    }

    public void DeletePropAndTower()
    {
        Debug.Log("DeletingTower");
        miniMapScript.DeleteTower(this, towerScript);

    }
}
