using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetSpawnableTitleCanvas : MonoBehaviour
{
    [SerializeField] GameObject tower;
    TowerBehaviour towerStats;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI moneyText;


    void Start()
    {
        if (tower != null)
        {
            GameObject tempTower = Instantiate(tower, Vector3.zero, Quaternion.identity);
            towerStats = tempTower.GetComponent<TowerBehaviour>();

            nameText.text = towerStats.gameObject.name.Replace(" XR", "").Replace("(Clone)", "") + "s";
            moneyText.text = "$" + towerStats.towerCost;

            Destroy(tempTower);
        }
    }

}
