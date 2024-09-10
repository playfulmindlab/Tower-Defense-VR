using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeRadialUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI towerNameText;
    [SerializeField] TextMeshProUGUI upgradeCostText;

    [SerializeField] PropManager propManager;

    private void Start()
    {
        propManager = GetComponentInParent<PropManager>();

        if (propManager != null)
        {
            towerNameText.text = propManager.gameObject.name.Replace(" Prop", "").Replace("(Clone)", "");
            if (propManager.upgradedTowerRef != null)
                upgradeCostText.text = "$" + propManager.upgradedTowerRef.towerCost;
            else
                upgradeCostText.text = "MAX";
        }
    }

}
