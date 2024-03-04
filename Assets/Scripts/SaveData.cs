using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData 
{
    public string playerID;
    public float totalTimePlayed;
    public int numberOfSessions;
    //public int wave;

    public List<HitsStruct> hitsStructures;
    public List<UpgradesStruct> upgradeStructures;


    DateTime timeSessionStarted;

    public void SessionInit()
    {
        numberOfSessions++;
        timeSessionStarted = DateTime.Now;
    }

    public void SessionEnded()
    {
        TimeSpan totalSessionTime = DateTime.Now - timeSessionStarted;
        totalTimePlayed += (float)totalSessionTime.TotalHours;
    }

    public void AddHitfo(HitsStruct newHit)
    {
        hitsStructures.Add(newHit);
    }

    public void AddUpgradeInfo(UpgradesStruct newUpgrade)
    {
        upgradeStructures.Add(newUpgrade);
    }

}


