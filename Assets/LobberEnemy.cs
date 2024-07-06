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

            /*Vector3 end = target.transform.position;
            Vector3 start = tip.transform.position;
            float xDiff = end.x - start.x;
            float zDiff = end.z - start.z;*/

            float dist = Vector3.Distance(target.transform.position, tip.transform.position) - tip.transform.position.y;
            missileSystemMain.startSpeed = Mathf.Sqrt(dist * missileSystemMain.gravityModifierMultiplier * 9.81f);

            missileSystem.Play();
        }
    }
}
