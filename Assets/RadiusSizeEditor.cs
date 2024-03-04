using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RadiusSizeEditor : MonoBehaviour
{
    [SerializeField] DecalProjector decalProj;

    // Start is called before the first frame update
    void Start()
    {
        decalProj = GetComponent<DecalProjector>();
    }

    public void ChangeRadiusSize(float radiusSize)
    {
        decalProj.size = new Vector3(radiusSize * 2, radiusSize * 2, 4f);
    }
}
