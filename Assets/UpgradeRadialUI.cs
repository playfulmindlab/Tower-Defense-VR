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
            string propName = propManager.gameObject.name;
            if (char.IsDigit(propName[propName.Length - 1]))
                propName = propName.Remove(propName.Length - 4);

            towerNameText.text = propName.Replace(" Prop", "").Replace("(Clone)", "");
            if (propManager.upgradedTowerRef != null)
                upgradeCostText.text = "$" + propManager.upgradedTowerRef.towerCost;
            else
                upgradeCostText.text = "MAX";
        }
    }

}
