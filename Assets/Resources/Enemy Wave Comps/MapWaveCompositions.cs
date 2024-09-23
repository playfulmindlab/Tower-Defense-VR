using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Wave Comp Data", menuName = "Create Wave Data")]
public class MapWaveCompositions : ScriptableObject
{
    public WaveComposition[] waves;

}

[System.Serializable]
public struct WaveComposition
{
    public EnemySummonData[] enemyOrder;
    public int loopEnemyOrder;
    public AfterWaveStep afterWaveStep;
}

[SerializeField]
public enum AfterWaveStep { Continue, Intermission, LevelUp };
