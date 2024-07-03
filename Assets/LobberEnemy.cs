using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobberEnemy : Enemy
{
    [SerializeField] GameObject tip;
    ParticleSystem missileSystem;
    ParticleSystem.MainModule missileSystemMain;

    // Start is called before the first frame update
    void Start()
    {
        missileSystem = tip.GetComponentInChildren<ParticleSystem>();
        missileSystemMain = missileSystem.main;
    }

    public override void Attack(TowerBehaviour target)
    {
        if (target)
        {
            //missileSystemMain.startRotationX = towerHead.forward.x;
            //missileSystemMain.startRotationY = towerHead.forward.y;
            //missileSystemMain.startRotationZ = towerHead.forward.z;

            float dist = Vector3.Distance(target.transform.position, tip.transform.position);
            missileSystemMain.startSpeed = Mathf.Sqrt(dist * missileSystemMain.gravityModifierMultiplier * 9.81f);

            missileSystem.Play();
        }
    }
}
