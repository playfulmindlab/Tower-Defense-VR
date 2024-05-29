using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnableObject : MonoBehaviour
{
    [SerializeField] GameObject spawnableObject;
    [SerializeField] GameObject spawnedObject;

    [SerializeField] Transform newSpawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        

    }

    public void RespawnGameObject()
    {
        if (spawnedObject != null)
        {
            spawnedObject = null;
        }

        GameObject newObject = Instantiate(spawnedObject, newSpawnPoint.position, newSpawnPoint.rotation);
        spawnedObject = newObject;
    }
}
