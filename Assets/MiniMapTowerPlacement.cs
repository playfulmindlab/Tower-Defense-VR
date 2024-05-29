using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapTowerPlacement : MonoBehaviour
{
    [SerializeField] GameObject mainMap;
    PropDropping newProp;

    public void DropNewProp(GameObject newProp, GameObject newMainMapTower, Vector3 localDropPoint)
    {
        Debug.Log("Go Thru TTPTBM // Point: " + localDropPoint);

        newProp.transform.parent = this.transform;
        newProp.transform.rotation = Quaternion.Euler(Vector3.zero);
        newProp.transform.position = localDropPoint;
        newProp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;

        RotateTowerTowardsPath(newProp);

        GameObject newTower = Instantiate(newMainMapTower, Vector3.zero, newProp.transform.rotation);
        newTower.transform.parent = mainMap.transform;
        newTower.transform.localPosition = newProp.transform.localPosition;



        //GameObject bigTowerToSpawn = newProp.GetComponent<PropInfo>
    }

    /*void PlaceNewTower(GameObject tower, Collider towerCollider, bool playTowerPlacedSFX = true)
    {
        TowerBehaviour currentTowerBehaviour = tower.GetComponent<TowerBehaviour>();
        TowerDefenseManager.towersInGame.Add(currentTowerBehaviour);

        if (currentPlacingTower != null)
            RotateTowerTowardsPath(currentPlacingTower);

        playerStats.SubtractMoney(currentTowerBehaviour.towerCost);

        towerCollider.isTrigger = false;
        towerCollider.gameObject.layer = 6;

        foreach (Transform child in towerCollider.transform)
        {
            child.gameObject.layer = 6;
        }

        if (playTowerPlacedSFX == true)
            AudioManager.instance.PlaySFXArray("TowerPlaced", tower.transform.position);

        towerCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }*/

    void RotateTowerTowardsPath(GameObject newTower)
    {
        int layerMask = 1 << 11; //"Path" Layer
        float closestPathDistance = Mathf.Infinity;

        RaycastHit hit;
        for (int d = 0; d < 360; d += 90)
        {
            if (Physics.Raycast(newTower.transform.position + (Vector3.up * 0.05f), new Vector3(Mathf.Sin(d * Mathf.Deg2Rad), 0, Mathf.Cos(d * Mathf.Deg2Rad)), out hit, Mathf.Infinity, layerMask))
            {
                if (hit.distance < closestPathDistance)
                {
                    newTower.transform.rotation = Quaternion.Euler(0, d, 0);
                    closestPathDistance = hit.distance;
                }
            }
        }
    }
}
