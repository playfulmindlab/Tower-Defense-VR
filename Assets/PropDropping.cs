using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropDropping : MonoBehaviour
{
    public LineRenderer line;
    public GameObject towerSpawn;

    public MiniMapTowerPlacement miniMapScript;

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
                Debug.Log("World 1: " + this.transform.position + " // Local 1: " + this.transform.localPosition);
                //Un-comment this out later
                //Make this a child of "Minimap", not "Baseplate"
                //this.transform.parent = hit.collider.transform.parent;
                //this.transform.rotation = Quaternion.Euler(Vector3.zero);
                //this.transform.position = hitPoint;

                miniMapScript.DropNewProp(this.gameObject, towerSpawn, hit.point);

                //this.transform.parent.GetComponent<TransferTowerPositionToBigMap>().DropNewProp(hitPoint, towerSpawn);

                Debug.Log("World 2: " + this.transform.position + " // Local 2: " + this.transform.localPosition);
                //Instantiate(towerSpawn, hit.point, Quaternion.identity);
            }

           // Vector3 dir = (line.GetPosition(1) - line.GetPosition(0)).normalized;
           // Physics.Raycast(line.GetPosition(0), dir, 40f, baseLayer);


        }
        else
        {
            //Vector3 dir = (line.GetPosition(1) - line.GetPosition(0)).normalized;
            //Physics.Raycast(line.GetPosition(0), dir, 40f, baseLayer)
            print("NOTHING");
        }
    }
}
