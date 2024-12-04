using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BarrelSpawnableScript : MonoBehaviour
{
    [SerializeField] GameObject newSpawn;
    [SerializeField] Material disabledMat;

    XRGrabInteractable grabInfo;
    XRBaseInteractor lastInteractor;
    XRInteractionManager xrManager;

    MiniMapTowerPlacement minimap;
    MeshRenderer[] renderers;
    Material[] baseMats;

    [SerializeField] bool isInteractable = true;
    public bool isLocked = false;

    int towerCost = 0;
    public int TowerCost { get { return towerCost; } set { towerCost = value; } }

    void Awake()
    {
        grabInfo = GetComponent<XRGrabInteractable>();
        xrManager = GameObject.FindGameObjectWithTag("XRManager").GetComponent<XRInteractionManager>();
        minimap = GameObject.FindGameObjectWithTag("MinimapBaseplate").GetComponentInParent<MiniMapTowerPlacement>();
        renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

        List<Material> matHolder = new List<Material>();
        foreach (Renderer renderer in renderers)
        {
            matHolder.Add(renderer.materials[0]);
            //renderer.material = disabledMat;
        }

        baseMats = matHolder.ToArray();

    }

    public void EnableSpawnable()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = baseMats[i];
        }

        isInteractable = true;
    }

    public void DisableSpawnable()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = disabledMat;
        }

        isInteractable = false;
    }

    public void ToggleLockedStat(bool lockStat)
    {
        isLocked = lockStat;
    }


    public void OnSelectFunc()
    {
        if (isInteractable == true && grabInfo.isSelected)
        {
            lastInteractor = (XRBaseInteractor)grabInfo.interactorsSelecting[0];
            xrManager.CancelInteractorSelection(grabInfo.interactorsSelecting[0]);
            AttachSpawnable();
        }
    }

    void AttachSpawnable()
    {
        GameObject newObject = Instantiate(newSpawn, transform.position, transform.rotation);
        XRGrabInteractable objectGrab = newObject.GetComponent<XRGrabInteractable>();
        //newPropManager.ToggleRadialMenu(true)

        xrManager.SelectEnter((IXRSelectInteractor)lastInteractor, objectGrab);

        minimap.AssignCurrentTower(newObject);
    }
}
