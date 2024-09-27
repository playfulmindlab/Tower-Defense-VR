using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreplacedPropManager : PropManager
{
    [SerializeField] TowerBehaviour existingTowerScript;
    [SerializeField] JumpedTowerControls existingJumpedTowerScript;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        towerScript = existingTowerScript;
        jumpedTowerScript = existingJumpedTowerScript;
        isPropDropped = true;

        gameObject.layer = LayerMask.NameToLayer("Props");
        LockPropPosition();

        Collider towerCollider = existingTowerScript.GetComponent<Collider>();
        towerCollider.gameObject.layer = 6;
        foreach (Transform child in towerCollider.transform)
        {
            child.gameObject.layer = 6;
        }

        towerCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
}
