using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RadiusSizeEditor : MonoBehaviour
{
    public static RadiusSizeEditor instance;

    DecalProjector decalProj;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);

        decalProj = GetComponent<DecalProjector>();
    }

    public void ChangeRadiusSize(TowerBehaviour tower)
    {
        transform.position = tower.transform.position;
        decalProj.size = new Vector3(tower.range * 2, tower.range * 2, 4f);
    }
}
