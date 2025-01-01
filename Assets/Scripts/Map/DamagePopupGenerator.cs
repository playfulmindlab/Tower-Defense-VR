using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopupGenerator : MonoBehaviour
{
    public static DamagePopupGenerator instance;

    [SerializeField] GameObject popupPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void SpawnNewPopup(Enemy damagedEnemy, float damageDealt)
    {
        GameObject newPopup = Instantiate(popupPrefab, damagedEnemy.transform.position, Quaternion.identity);
        newPopup.transform.SetParent(damagedEnemy.transform);

        TextMeshProUGUI newPopupText = newPopup.GetComponentInChildren<TextMeshProUGUI>();
        int damInt = Mathf.RoundToInt(damageDealt);
        newPopupText.text = "-" + damInt.ToString();

        Destroy(newPopup, 2.5f);
    }
}
