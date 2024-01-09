using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData 
{
    public string playerID;
    public int wave;

    public List<HitsStruct> hitsStructures;
    public List<UpgradesStruct> upgradeStructures;

    public void AddHitfo(HitsStruct newHit)
    {
        hitsStructures.Add(newHit);
    }

    public void AddUpgradeInfo(UpgradesStruct newUpgrade)
    {
        upgradeStructures.Add(newUpgrade);
    }

}


