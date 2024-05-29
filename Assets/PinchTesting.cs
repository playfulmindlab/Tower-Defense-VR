using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PinchTesting : MonoBehaviour
{
    [SerializeField] List<Material> objectMaterials;
    [SerializeField] GameObject newSpawn;

    XRGrabInteractable grabInfo;
    XRBaseInteractor lastInteractor;
    [SerializeField] XRInteractionManager xrManager;

    public bool isPinched = false;

    void Start()
    {
        grabInfo = GetComponent<XRGrabInteractable>();

        foreach (Renderer childRender in GetComponentsInChildren<Renderer>())
        {
            objectMaterials.AddRange(childRender.materials);
        }
    }

    public void ChangeToWhite()
    {
        foreach (Material mat in objectMaterials)
        {
            mat.color = Color.white;
        }
        isPinched = false;
    }

    public void ChangeToYellow()
    {
        foreach (Material mat in objectMaterials)
        {
            mat.color = Color.yellow;
        }
        isPinched = false;
    }

    public void ChangeToRed()
    {
        foreach (Material mat in objectMaterials)
        {
            mat.color = Color.red;
        } 
    }

    public void ChangeToBlue()
    {
        foreach (Material mat in objectMaterials)
        {
            mat.color = Color.cyan;
        }
        isPinched = true;
    }

    public void ChangeToGreen()
    {
        foreach (Material mat in objectMaterials)
        {
            mat.color = Color.green;
        }
    }

    void AttachSpawnable()
    {
        GameObject newObject = Instantiate(newSpawn, transform.position, transform.rotation);
        XRGrabInteractable objectGrab = newObject.GetComponent<XRGrabInteractable>();

        xrManager.SelectEnter(lastInteractor, objectGrab);

    }

    public void ChangeOnWorking()
    {
        if (isPinched == true)
        {
            ChangeToGreen();
            isPinched = false;

            if (grabInfo.isSelected)
            {
                print("thisIsSelected");

                lastInteractor = (XRBaseInteractor)grabInfo.interactorsSelecting[0];
                xrManager.CancelInteractorSelection(grabInfo.interactorsSelecting[0]);
                AttachSpawnable();
            }
        }
        else
        {
            ChangeToWhite();
            if (!grabInfo.isSelected)
            {
                print("NOT thisIsSelected");
            }
        }
    }
}
