using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public int waveNum = 1;
    [SerializeField] private int startingMoney;
    [SerializeField] private int currentMoney;

    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemyCountText;

    public int CurrentMoney { get { return currentMoney; } }

    // Start is called before the first frame update
    void Start()
    {
        currentMoney = startingMoney;
        UpdateMoneyText();
    }

    public void AddMoney(int newFunds)
    {
        currentMoney += newFunds;
        UpdateMoneyText();
    }

    public void SubtractMoney(int removedFunds)
    {
        currentMoney -= removedFunds;
        UpdateMoneyText();
    }

    public void UpdateMoneyText()
    {
        moneyText.SetText($"${currentMoney}");
    }

    public void DisplayWaveCount(int count)
    {
        waveText.text = "Wave: " + count;
    }

    public void DisplayEnemyCount(int count)
    {
        enemyCountText.text = "Enemies: " + count;
    }
}
