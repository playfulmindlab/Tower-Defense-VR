using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapTowerPlacement : MonoBehaviour
{
    [SerializeField] GameObject mainMap;
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] PropManager lastPropMenuActivated = null;

    [SerializeField] GameObject currTower;
    [SerializeField] GameObject radiusDecal;

    [SerializeField] bool buildOnBuildPhaseOnly = true;

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    private void Update()
    {
        if (currTower != null)
        {
            radiusDecal.transform.position = new Vector3(currTower.transform.position.x, transform.position.y, currTower.transform.position.z);
        }
    }

    public void AssignCurrentTower(GameObject newProp)
    {
        currTower = newProp;

        if (newProp != null)
        {
            RadiusSizeEditor radius = radiusDecal.GetComponent<RadiusSizeEditor>();
            radius.ChangeRadiusSize(newProp.GetComponent<PropManager>().towerSpawn.GetComponent<TowerBehaviour>());
        }
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

            RotateTowerTowardsPath(newProp);

            GameObject newTower = Instantiate(newMainMapTower, Vector3.zero, newProp.transform.rotation);
            newTower.transform.parent = mainMap.transform;
            newTower.transform.localEulerAngles = newProp.transform.localEulerAngles;
            newTower.transform.localPosition = newProp.transform.localPosition;

            PlaceNewTower(newTower, newTower.GetComponent<BoxCollider>());

            newProp.GetComponent<PropManager>().LockPropPosition();
            newTower.GetComponent<TowerBehaviour>().AssignPropParent(newProp.GetComponent<PropManager>());
            spawnedTower = newTower;

            AssignCurrentTower(null);
            radiusDecal.transform.position = new Vector3(0, -1000, 0);
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

        //if (currentPlacingTower != null)
        //    RotateTowerTowardsPath(currentPlacingTower);

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
            child.gameObject.layer = newLayerNum;
        }

        if (playTowerPlacedSFX == true)
            AudioManager.instance.PlaySFXArray("TowerPlaced", tower.transform.position);

        towerCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        Debug.Log("XR PLACEMENT SUCCESSFUL!");
    }

    void RotateTowerTowardsPath(GameObject newTower)
    {
        //int layerMask = 1 << 11; //"Path" Layer
        string[] layerMasks = { "Path", "LevelUpPath" };
        float closestPathDistance = Mathf.Infinity;

        RaycastHit hit;
        for (int d = 0; d < 360; d += 90)
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
                Destroy(oldTower);

                AudioManager.instance.PlaySFXArray("TowerUpgrade", newTower.transform.position);

                //upgradeConfetti.transform.position = newTower.transform.position;
                //upgradeConfetti.Play();

                //------------------------------------------
                //------------------------------------------

                GameObject newProp = Instantiate(oldProp.upgradedProp, oldProp.transform.position, oldProp.transform.rotation);

                PropManager newPropManager = newProp.GetComponent<PropManager>();
                if (newPropManager != null)
                {
                    newPropManager.SpawnUpgradedProp(newTower.GetComponent<TowerBehaviour>());
                    newPropManager.ForceStart();
                    newPropManager.ToggleRadialMenu(true);
                }

                newProp.transform.parent = this.transform;

                Destroy(oldProp.gameObject);

                newProp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
                newProp.GetComponent<Rigidbody>().isKinematic = true;
                newPropManager.LockPropPosition();
                newTower.GetComponent<TowerBehaviour>().AssignPropParent(newPropManager);
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
        playerStats.AddMoney(towerScript.towerCost);
        TowerDefenseManager.EnqueueTowerToRemove(towerScript);
        Destroy(propScript.gameObject);
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

//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

/*public class TowerPlacementVR : MonoBehaviour
{
    [SerializeField] private LayerMask placementCheckMask;
    [SerializeField] private LayerMask placementCollideMask;
    [SerializeField] private LayerMask obstaclePlacementCheckMask;
    [SerializeField] private LayerMask obstaclePlacementCollideMask;
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private GameObject currentPlacingTower;
    [SerializeField] GameObject radiusDecalObject;
    //[SerializeField] RadiusSizeEditor radiusSizeEditor;
    private Camera playerCamera;

    [SerializeField] VRPointer placementPointer;
    [SerializeField] ParticleSystem upgradeConfetti;
    [SerializeField] public InputActionProperty towerSpawnerButton;
    [SerializeField] public InputActionProperty cancelTowerSpawnButton;
    [SerializeField] public InputActionProperty deleteTowerButton;

    void Start()
    {
        playerCamera = Camera.main;
    }

    public void CancelTowerPlacement()
    {
        Destroy(currentPlacingTower);
        currentPlacingTower = null;
    }

    void Update()
    {
        if (currentPlacingTower != null)
        {
            if (cancelTowerSpawnButton.action.WasPerformedThisFrame())
            {
                CancelTowerPlacement();
                return;
            }

            currentPlacingTower.transform.position = placementPointer.endPoint;
            radiusDecalObject.transform.position = placementPointer.endPoint;

            if (towerSpawnerButton.action.WasPerformedThisFrame() && placementPointer.collision != null)
            {
                if (currentPlacingTower.CompareTag("Tower") &&
                    !placementPointer.collision.CompareTag("Path") &&
                    !placementPointer.collision.CompareTag("Tower"))
                {
                    BoxCollider towerCollider = currentPlacingTower.gameObject.GetComponent<BoxCollider>();
                    towerCollider.isTrigger = true;

                    Vector3 boxCenter = currentPlacingTower.gameObject.transform.position + towerCollider.center;
                    Vector3 halfExtents = towerCollider.size / 2;

                    if (!Physics.CheckBox(boxCenter, halfExtents, Quaternion.identity, placementCheckMask, QueryTriggerInteraction.Ignore))
                    {
                        CreateNewTower(currentPlacingTower, towerCollider);
                        currentPlacingTower = null;

                        radiusDecalObject.transform.position = new Vector3(0f, -1000f, 0f);
                    }
                }
                else if ((currentPlacingTower.CompareTag("Obstacle") ||
                        currentPlacingTower.CompareTag("AttackObstacle")) &&
                        placementPointer.collision.CompareTag("Path"))
                {
                    BoxCollider towerCollider = currentPlacingTower.gameObject.GetComponent<BoxCollider>();
                    towerCollider.isTrigger = true;

                    Vector3 boxCenter = currentPlacingTower.gameObject.transform.position + towerCollider.center;
                    Vector3 halfExtents = towerCollider.size / 2;

                    if (!Physics.CheckBox(boxCenter, halfExtents, Quaternion.identity, obstaclePlacementCheckMask, QueryTriggerInteraction.Ignore))
                    {
                        TowerBehaviour currentTowerBehaviour = currentPlacingTower.GetComponent<TowerBehaviour>();
                        TowerDefenseManager.towersInGame.Add(currentTowerBehaviour);

                        playerStats.SubtractMoney(currentTowerBehaviour.towerCost);

                        //towerCollider.isTrigger = false;
                        towerCollider.gameObject.layer = 9;
                        foreach (Transform child in towerCollider.transform)
                        {
                            child.gameObject.layer = 9;
                        }
                        AudioManager.instance.PlaySFXArray("TowerPlaced", currentPlacingTower.transform.position);
                        towerCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                        currentPlacingTower = null;

                        radiusDecalObject.transform.position = new Vector3(0f, -1000f, 0f);
                    }
                }
            }
        }

        else
        {
            if (deleteTowerButton.action.WasPerformedThisFrame() && placementPointer.collision != null)
            {
                Debug.Log("ButtonPressed");
                TowerBehaviour deletingTower = placementPointer.collision.GetComponent<TowerBehaviour>();
                if (deletingTower != null && (deletingTower.gameObject.CompareTag("Tower") || deletingTower.gameObject.CompareTag("Obstacle") || deletingTower.gameObject.CompareTag("AttackObstacle")))
                {
                    Debug.Log("DeletingTower");
                    playerStats.AddMoney(deletingTower.towerCost);
                    TowerDefenseManager.EnqueueTowerToRemove(deletingTower);
                }

            }
        }

    }

    public void SetTowerToPlace(GameObject newTower)
    {
        if (TowerDefenseManager.CurrPhase == Phase.Build ||
            TowerDefenseManager.CurrPhase == Phase.Repair)
        {
            int newTowerCost = newTower.GetComponent<TowerBehaviour>().towerCost;

            if (playerStats.CurrentMoney >= newTowerCost)
            {
                if (currentPlacingTower != null)
                {
                    CancelTowerPlacement();
                }

                currentPlacingTower = Instantiate(newTower, Vector3.zero, Quaternion.identity);
                RadiusSizeEditor.instance.ChangeRadiusSize(newTower.GetComponent<TowerBehaviour>());
            }
            else
            {
                Debug.Log("NOT_ERROR: You do not have the appropriate funds to buy this tower!");
            }
        }
        else { Debug.LogError("ERROR: You are not currently in the Build or Repair phase!"); }
    }

    public void UpgradeTower(GameObject oldTower, GameObject upgradedTower)
    {
        if (TowerDefenseManager.CurrPhase == Phase.Build ||
            TowerDefenseManager.CurrPhase == Phase.Repair)
        {
            int newTowerCost = upgradedTower.GetComponent<TowerBehaviour>().towerCost;

            if (playerStats.CurrentMoney >= newTowerCost)
            {
                GameObject newTower = Instantiate(upgradedTower, oldTower.transform.position, oldTower.transform.rotation);
                BoxCollider towerCollider = newTower.GetComponent<BoxCollider>();

                TowerDefenseManager.towersInGame.Remove(oldTower.GetComponent<TowerBehaviour>());

                CreateNewTower(newTower, towerCollider, false);
                Destroy(oldTower);

                AudioManager.instance.PlaySFXArray("TowerUpgrade", newTower.transform.position);

                upgradeConfetti.transform.position = newTower.transform.position;
                upgradeConfetti.Play();
            }
            else
            {
                Debug.Log("UPGRADE_ERROR: You do not have the appropriate funds to upgrade this tower!");
            }
        }
        else { Debug.LogError("ERROR: You are not currently in the Build or Repair phase!"); }
    }
}
*/
