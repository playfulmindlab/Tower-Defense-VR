using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int startingMoney;
    [SerializeField] private int currentMoney;

    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI enemyKillCountText;
    [SerializeField] private TextMeshProUGUI enemyKillCountText2;

    public int CurrentMoney { get { return currentMoney; } }

    private void Awake()
    {
        transform.position = new Vector3(0f, 8.3f, -35f);
    }

    // Start is called before the first frame update
    void Start()
    {
        currentMoney = startingMoney;

        if (moneyText != null)
            UpdateMoneyText();
    }

    public void AddMoney(int newFunds)
    {
        currentMoney += newFunds;
        UpdateMoneyText();
        SpawnablesEnabler.instance.UpdateUnaffordableTowers(currentMoney);
    }

    public void SubtractMoney(int removedFunds)
    {
        currentMoney -= removedFunds;
        UpdateMoneyText();
        SpawnablesEnabler.instance.UpdateUnaffordableTowers(currentMoney);
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
    public void UpdateKillCountText(int count)
    {
        enemyKillCountText.text = "Kills: " + count;
        enemyKillCountText2.text = "Kills: " + count;
    }

}
