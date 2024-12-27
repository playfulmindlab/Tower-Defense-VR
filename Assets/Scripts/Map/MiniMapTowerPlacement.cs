using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapTowerPlacement : MonoBehaviour
{
    [SerializeField] GameObject mainMap;
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] PropManager lastPropMenuActivated = null;

    [SerializeField] GameObject currProp;
    [SerializeField] GameObject radiusDecal;
    RadiusSizeEditor radius;

    [SerializeField] bool buildOnBuildPhaseOnly = true;

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();

        radius = radiusDecal.GetComponent<RadiusSizeEditor>();
    }

    private void Update()
    {
        if (currProp != null)
        {
            radiusDecal.transform.position = new Vector3(currProp.transform.position.x, transform.position.y, currProp.transform.position.z);
        }
    }

    public void AssignCurrentTower(GameObject newProp)
    {
        currProp = newProp;

        if (newProp != null)
        {
            radius.ChangeRadiusSize(newProp.GetComponent<PropManager>().towerSpawn.GetComponent<TowerBehaviour>());
        }
    }

    public void ResetRadiusDecal()
    {
        if (currProp != null) currProp = null;
        radiusDecal.transform.position = new Vector3(0, -1000, 0);
    }

    public void CancelTowerPlacement()
    {
        if (currProp != null)
        {
            Destroy(currProp);
            AssignCurrentTower(null);
        }
        ResetRadiusDecal();
    }

    public GameObject DropNewProp(GameObject newProp, GameObject newMainMapTower, Vector3 localDropPoint)
    {
        TowerBehaviour towerToDrop = newProp.GetComponent<PropManager>().towerSpawn.GetComponent<TowerBehaviour>();
        GameObject spawnedTower = null;

        if (playerStats.CurrentMoney >= towerToDrop.towerCost && 
            (buildOnBuildPhaseOnly || (TowerDefenseManager.CurrPhase != Phase.Build || 
            TowerDefenseManager.CurrPhase != Phase.Repair)))
        {
            newProp.transform.parent = this.transform;
            newProp.transform.rotation = Quaternion.Euler(Vector3.zero);
            newProp.transform.position = localDropPoint;
            newProp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            newProp.GetComponent<Rigidbody>().isKinematic = true;
            newProp.layer = LayerMask.NameToLayer("Props");
            foreach(Transform pChild in newProp.transform) { pChild.gameObject.layer = LayerMask.NameToLayer("Props"); }

            RotateTowerTowardsPath(newProp);

            GameObject newTower = Instantiate(newMainMapTower, Vector3.zero, newProp.transform.rotation);
            newTower.transform.parent = mainMap.transform;
            newTower.transform.localEulerAngles = newProp.transform.localEulerAngles;
            newTower.transform.localPosition = newProp.transform.localPosition;

            PlaceNewTower(newTower, newTower.GetComponent<BoxCollider>());

            newProp.GetComponent<PropManager>().LockPropPosition();
            newTower.GetComponent<TowerBehaviour>().AssignPropParent(newProp.GetComponent<PropManager>());
            spawnedTower = newTower;

            newTower.name = NewTowerIDGenerator(newTower.name);
            newProp.name = NewTowerIDGenerator(newProp.name);

            GameManager.instance.LogNewEvent("PPO Placed - " + newTower.name, newTower, newTower.transform.position, GameControlManager.instance.IsJumped);

            AssignCurrentTower(null);
            ResetRadiusDecal();
        }
        else
        {
            Debug.Log("ERROR: Can o0nly place towers in build phase!");
        }
        return spawnedTower;
    }

    void PlaceNewTower(GameObject tower, Collider towerCollider, bool playTowerPlacedSFX = true)
    {
        TowerBehaviour currentTowerBehaviour = tower.GetComponent<TowerBehaviour>();
        TowerDefenseManager.towersInGame.Add(currentTowerBehaviour);

        playerStats.SubtractMoney(currentTowerBehaviour.towerCost);

        towerCollider.isTrigger = false;
        string towerTag = tower.tag;
        int newLayerNum = 0;

        switch (towerTag)
        {
            case "Obstacle":
            case "AttackObstacle":
                newLayerNum = 9;
                break;
            case "Tower":
            default:
                newLayerNum = 6;
                break;
        }

        towerCollider.gameObject.layer = newLayerNum;
        foreach (Transform child in towerCollider.transform)
        {
            Debug.Log("LAYER CHANGE: " + newLayerNum + " @ " + child.gameObject);
            child.gameObject.layer = newLayerNum;
        }

        if (playTowerPlacedSFX == true)
            AudioManager.instance.PlaySFXArray("TowerPlaced", tower.transform.position);

        towerCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        Debug.Log("XR PLACEMENT SUCCESSFUL!");
    }

    string NewTowerIDGenerator(string originalName)
    {
        originalName = originalName.Replace("(Clone)", " ");

        string newName = originalName;
        if (char.IsDigit(newName[newName.Length - 1]))
            newName = newName.Remove(newName.Length - 4);
        int randID = Random.Range(0, 10000);
        newName += randID.ToString("D4");

        return newName;
    }

    void RotateTowerTowardsPath(GameObject newTower)
    {
        string[] layerMasks = { "Path", "LevelUpPath" };
        float closestPathDistance = Mathf.Infinity;

        RaycastHit hit;
        for (int d = 0; d < 360; d += 30)
        {
            if (Physics.Raycast(newTower.transform.position + (Vector3.up * 0.003f),
                new Vector3(Mathf.Sin(d * Mathf.Deg2Rad), 0, Mathf.Cos(d * Mathf.Deg2Rad)),
                out hit, Mathf.Infinity, LayerMask.GetMask(layerMasks)))
            {
                if (hit.distance < closestPathDistance)
                {
                    newTower.transform.rotation = Quaternion.Euler(0, d, 0);
                    closestPathDistance = hit.distance;
                }
            }
        }
    }

    public void UpgradeTower(GameObject oldTower, PropManager oldProp, GameObject upgradedTower)
    {
        if (buildOnBuildPhaseOnly || (TowerDefenseManager.CurrPhase == Phase.Build || TowerDefenseManager.CurrPhase == Phase.Repair))
        {
            int newTowerCost = upgradedTower.GetComponent<TowerBehaviour>().towerCost;

            Debug.Log("PlayerStats: " + playerStats.gameObject.name);
            Debug.Log("Money: " + playerStats.CurrentMoney + " vs. " + newTowerCost);

            if (playerStats.CurrentMoney >= newTowerCost)
            {
                GameObject newTower = Instantiate(upgradedTower, oldTower.transform.position, oldTower.transform.rotation);
                BoxCollider towerCollider = newTower.GetComponent<BoxCollider>();

                TowerDefenseManager.towersInGame.Remove(oldTower.GetComponent<TowerBehaviour>());

                PlaceNewTower(newTower, towerCollider, false);

                newTower.name = NewTowerIDGenerator(newTower.name);
                GameManager.instance.LogNewEvent("PPO Upgrade - " + oldTower.name + " > " + newTower.name, oldTower, oldTower.transform.position, GameControlManager.instance.IsJumped);

                Destroy(oldTower);

                AudioManager.instance.PlaySFXArray("TowerUpgrade", newTower.transform.position);

                //upgradeConfetti.transform.position = newTower.transform.position;
                //upgradeConfetti.Play();

                //------------------------------------------
                //------------------------------------------

                GameObject newProp = Instantiate(oldProp.upgradedProp, oldProp.transform.position, oldProp.transform.rotation);
                newProp.name = NewTowerIDGenerator(newProp.name);

                PropManager newPropManager = newProp.GetComponent<PropManager>();
                if (newPropManager != null)
                {
                    newPropManager.SpawnUpgradedProp(newTower.GetComponent<TowerBehaviour>());
                    newPropManager.ForceStart();
                    newPropManager.ToggleRadialMenu(true);
                }

                newProp.transform.parent = this.transform;

                //string newID = 
                //newTower.name = NewTowerIDGenerator(newTower.name);
                //GameManager.instance.LogNewEvent("PPO Upgrade - " + gameObject.name + " > " + upgradedTower.name, this.gameObject, transform.position, GameControlManager.instance.IsJumped);

                Destroy(oldProp.gameObject);

                newProp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
                newProp.GetComponent<Rigidbody>().isKinematic = true;
                newPropManager.LockPropPosition();
                newTower.GetComponent<TowerBehaviour>().AssignPropParent(newPropManager);

                towerCollider.isTrigger = false;
                string propTag = newProp.tag;
                switch (propTag)
                {
                    case "Obstacle":
                    case "AttackObstacle":
                        towerCollider.gameObject.layer = 9;
                        break;
                    case "Tower":
                    default:
                        towerCollider.gameObject.layer = 6;
                        break;
                }
                newProp.layer = LayerMask.NameToLayer("Props");
                foreach (Transform pChild in newProp.transform) { pChild.gameObject.layer = LayerMask.NameToLayer("Props"); }
            }
            else
            {
                Debug.Log("UPGRADE_ERROR: You do not have the appropriate funds to upgrade this tower!");
            }
        }
        else { Debug.LogError("ERROR: You are not currently in the Build or Repair phase!"); }
    }

    public void DeleteTower(PropManager propScript, TowerBehaviour towerScript)
    {
        Debug.Log("PS: " + playerStats + " // TS: " + towerScript);
        if (propScript.IsDying == false)
        {
            propScript.IsDying = true;
            if (currProp != null && currProp == propScript)
                ResetRadiusDecal();
            playerStats.AddMoney(towerScript.towerCost);
            TowerDefenseManager.EnqueueTowerToRemove(towerScript);
            Destroy(propScript.gameObject);
        }
    }

    public void SwapActivatedPropMenu(PropManager newPropManager)
    {
        if (lastPropMenuActivated != null)
        {
            lastPropMenuActivated.ToggleRadialMenu(false);
        }

        if (lastPropMenuActivated == newPropManager)
        {
            lastPropMenuActivated.ToggleRadialMenu(false);
            lastPropMenuActivated = null;
        }
        else lastPropMenuActivated = newPropManager;
    }

}