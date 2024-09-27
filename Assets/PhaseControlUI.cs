using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhaseControlUI : MonoBehaviour
{
    [SerializeField] Button defendButton;

    public void ToggleDefendButton(bool toggleState)
    {
        defendButton.interactable = toggleState;
    }
}
