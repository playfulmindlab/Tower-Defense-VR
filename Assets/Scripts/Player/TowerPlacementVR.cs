using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacementVR : MonoBehaviour
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
            currentPlacingTower.transform.position = placementPointer.endPoint;
            radiusDecalObject.transform.position = placementPointer.endPoint;

            if (cancelTowerSpawnButton.action.WasPerformedThisFrame())
            {
                CancelTowerPlacement();
                return;
            }

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
                        towerCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                        currentPlacingTower = null;

                        radiusDecalObject.transform.position = new Vector3(0f, -1000f, 0f);
                    }
                }
            }
        }

        else
        {
            if (deleteTowerButton.action.WasPerformedThisFrame())
            {
                Debug.Log("ButtonPressed");
                if (placementPointer.collision.GetComponent<TowerBehaviour>() != null)
                {
                    Debug.Log("DeletingTower");
                    TowerDefenseManager.EnqueueTowerToRemove(placementPointer.collision.GetComponent<TowerBehaviour>());
                }

            }
        }

    }

    void CreateNewTower(GameObject tower, Collider towerCollider)
    {
        TowerBehaviour currentTowerBehaviour = tower.GetComponent<TowerBehaviour>();
        TowerDefenseManager.towersInGame.Add(currentTowerBehaviour);

        playerStats.SubtractMoney(currentTowerBehaviour.towerCost);

        towerCollider.isTrigger = false;
        towerCollider.gameObject.layer = 6;
        foreach (Transform child in towerCollider.transform)
        {
            child.gameObject.layer = 6;
        }
        towerCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; ;

    }

    public void SetTowerToPlace(GameObject newTower)
    {
        int newTowerCost = newTower.GetComponent<TowerBehaviour>().towerCost;

        if (playerStats.CurrentMoney >= newTowerCost)
        {
            if (currentPlacingTower != null)
            {
                CancelTowerPlacement();
            }

            currentPlacingTower = Instantiate(newTower, Vector3.zero, Quaternion.identity);
            radiusDecalObject.GetComponent<RadiusSizeEditor>().ChangeRadiusSize(newTower.GetComponent<TowerBehaviour>().range);
        }
        else
        {
            Debug.Log("NOT_ERROR: You do not have the appropriate funds to buy this tower!");
        }
    }

    public void UpgradeTower(GameObject oldTower, GameObject upgradedTower)
    {
        int newTowerCost = upgradedTower.GetComponent<TowerBehaviour>().towerCost;

        if (playerStats.CurrentMoney >= newTowerCost)
        {
            GameObject newTower = Instantiate(upgradedTower, oldTower.transform.position, Quaternion.identity);
            BoxCollider towerCollider = newTower.GetComponent<BoxCollider>();

            TowerDefenseManager.towersInGame.Remove(oldTower.GetComponent<TowerBehaviour>());

            CreateNewTower(newTower, towerCollider);
            Destroy(oldTower);

            upgradeConfetti.transform.position = newTower.transform.position;
            upgradeConfetti.Play();
        }
        else
        {
            Debug.Log("UPGRADE_ERROR: You do not have the appropriate funds to upgrade this tower!");
        }
    }
}