using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    [SerializeField] private LayerMask placementCheckMask;
    [SerializeField] private LayerMask placementCollideMask;
    [SerializeField] private LayerMask obstaclePlacementCheckMask;
    [SerializeField] private LayerMask obstaclePlacementCollideMask;
    [SerializeField] private PlayerStats playerStats;

    [SerializeField]private GameObject currentPlacingTower;
    private Camera playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPlacingTower != null)
        {
            Ray camRay = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if ((Physics.Raycast(camRay, out hitInfo, 100f, placementCollideMask) && currentPlacingTower.CompareTag("Tower")) ||
                (Physics.Raycast(camRay, out hitInfo, 100f, obstaclePlacementCollideMask) && currentPlacingTower.CompareTag("Obstacle")))
            {
                currentPlacingTower.transform.position = hitInfo.point;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Destroy(currentPlacingTower);
                currentPlacingTower = null;
                return;
            }

            if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject != null)
            {
                if (currentPlacingTower.CompareTag("Tower") &&
                    !hitInfo.collider.gameObject.CompareTag("Path") &&
                    !hitInfo.collider.gameObject.CompareTag("Tower"))
                {
                    BoxCollider towerCollider = currentPlacingTower.gameObject.GetComponent<BoxCollider>();
                    towerCollider.isTrigger = true;

                    Vector3 boxCenter = currentPlacingTower.gameObject.transform.position + towerCollider.center;
                    Vector3 halfExtents = towerCollider.size / 2;
                    if (!Physics.CheckBox(boxCenter, halfExtents, Quaternion.identity, placementCheckMask, QueryTriggerInteraction.Ignore))
                    {
                        TowerBehaviour currentTowerBehaviour = currentPlacingTower.GetComponent<TowerBehaviour>();
                        TowerDefenseManager.towersInGame.Add(currentTowerBehaviour);

                        playerStats.SubtractMoney(currentTowerBehaviour.towerCost);

                        towerCollider.isTrigger = false;
                        currentPlacingTower = null;
                    }
                }
                else if (currentPlacingTower.CompareTag("Obstacle") &&
                        hitInfo.collider.gameObject.CompareTag("Path"))
                {
                    Debug.Log("Going Once);");
                    BoxCollider towerCollider = currentPlacingTower.gameObject.GetComponent<BoxCollider>();
                    towerCollider.isTrigger = true;

                    Vector3 boxCenter = currentPlacingTower.gameObject.transform.position + towerCollider.center;
                    Vector3 halfExtents = towerCollider.size / 2;

                    Collider[] tempArray = Physics.OverlapBox(boxCenter, halfExtents, Quaternion.identity, placementCheckMask, QueryTriggerInteraction.Ignore);
                    foreach(Collider col in tempArray)
                    {
                        Debug.Log(col.gameObject.name);
                    }
                    
                    if (!Physics.CheckBox(boxCenter, halfExtents, Quaternion.identity, placementCheckMask, QueryTriggerInteraction.Ignore))
                    {
                        TowerBehaviour currentTowerBehaviour = currentPlacingTower.GetComponent<TowerBehaviour>();
                        TowerDefenseManager.towersInGame.Add(currentTowerBehaviour);

                        playerStats.SubtractMoney(currentTowerBehaviour.towerCost);

                        towerCollider.isTrigger = false;
                        currentPlacingTower = null;
                    }
                }
            }
        }
    }

    public void SetTowerToPlace(GameObject newTower)
    {
        int newTowerCost = newTower.GetComponentInChildren<TowerBehaviour>().towerCost;

        if (playerStats.CurrentMoney >= newTowerCost)
        {
            currentPlacingTower = Instantiate(newTower, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.Log("NOT_ERROR: You do not have the appropriate funds to buy this tower!");
        }
    }
}
