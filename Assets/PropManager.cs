using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropManager : MonoBehaviour
{
    public LineRenderer line;
    public GameObject towerSpawn;

    public MiniMapTowerPlacement miniMapScript;

    public GameObject upgradedProp;

    TowerBehaviour towerScript;
    JumpedTowerControls jumpedTowerScript;

    LayerMask baseLayer; //= LayerMask.GetMask("Baseplate");

    // Start is called before the first frame update
    void Start()
    {
        baseLayer = LayerMask.GetMask("Baseplate");

        miniMapScript = GameObject.FindGameObjectWithTag("MinimapBaseplate").GetComponentInParent<MiniMapTowerPlacement>();
    }

    public void TowerDropped()
    {
        Vector3 dir = (line.GetPosition(1) - line.GetPosition(0)).normalized;

        RaycastHit hit;
        Physics.Raycast(line.GetPosition(0), dir, out hit, 40f, baseLayer);

        if (Physics.Raycast(line.GetPosition(0), dir, out hit, 40f, baseLayer)) 
            Debug.Log("Detect Layer: " + hit.collider.gameObject.layer);

        //Check to see if it intersects Baseplate & ONLY baseplate
        if (Physics.Raycast(line.GetPosition(0), dir, out hit, 40f, baseLayer))
        {
            print("READING");

            //Physics.Raycast(line.GetPosition(0), dir, out hit, 40f, baseLayer);
            if (hit.collider != null)
            {
                Vector3 hitPoint = hit.point;

                Debug.Log("Hit Object: " + hit.collider.gameObject);
                //Debug.Log("World 1: " + this.transform.position + " // Local 1: " + this.transform.localPosition);

                towerSpawn = miniMapScript.DropNewProp(this.gameObject, towerSpawn, hit.point);

                towerScript = towerSpawn.GetComponent<TowerBehaviour>();
                jumpedTowerScript = towerSpawn.GetComponent<JumpedTowerControls>();

                //Debug.Log("World 2: " + this.transform.position + " // Local 2: " + this.transform.localPosition);
            }
        }
        else
        {
            Destroy(gameObject);
            print("NOTHING");
        }
    }

    public void SpawnUpgradedProp(TowerBehaviour upgradedTowerScript)
    {
        towerScript = upgradedTowerScript.GetComponent<TowerBehaviour>();
        jumpedTowerScript = upgradedTowerScript.GetComponent<JumpedTowerControls>();
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