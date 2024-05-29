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

        GameObject newTower = Instantiate(newMainMapTower, Vector3.zero, Quaternion.identity);
        newTower.transform.parent = mainMap.transform;
        newTower.transform.localPosition = newProp.transform.localPosition;

        //GameObject bigTowerToSpawn = newProp.GetComponent<PropInfo>
    }
}
