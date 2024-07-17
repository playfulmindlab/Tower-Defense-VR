using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BarrelSpawnableScript : MonoBehaviour
{
    [SerializeField] GameObject newSpawn;

    XRGrabInteractable grabInfo;
    XRBaseInteractor lastInteractor;
    XRInteractionManager xrManager;

    MiniMapTowerPlacement minimap;

    void Start()
    {
        grabInfo = GetComponent<XRGrabInteractable>();
        xrManager = GameObject.FindGameObjectWithTag("XRManager").GetComponent<XRInteractionManager>();
        minimap = GameObject.FindGameObjectWithTag("MinimapBaseplate").GetComponentInParent<MiniMapTowerPlacement>();
    }

    public void OnHoverExitFunc()
    {
        if (grabInfo.isSelected)
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

        xrManager.SelectEnter((IXRSelectInteractor)lastInteractor, objectGrab);

        minimap.AssignCurrentTower(newObject);
    }
}
