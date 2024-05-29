using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomInteractionManager : XRInteractionManager
{
    public void ForceDeselect(XRBaseInteractor interactor, XRBaseInteractable newInteractable)
    {
        while (interactor.interactablesSelected.Count > 0)
        {
            SelectExit(interactor, interactor.interactablesSelected[0]);
            SelectEnter((IXRSelectInteractor)interactor, newInteractable);
        }
    }
}
