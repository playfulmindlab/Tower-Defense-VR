using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropAim : MonoBehaviour
{
    [SerializeField] LayerMask detectLayer;
    [SerializeField] LayerMask ignoreLayers;
    LineRenderer line;
    int layerInt = 0;
    PropManager pManager;
    float range = .001f;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        layerInt = (int) Mathf.Log(detectLayer.value, 2);
        ignoreLayers = ~ignoreLayers;
        pManager = GetComponentInParent<PropManager>();
        range = pManager.PlacementRange;
    }
    

    // Update is called once per frame
    void Update()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position + new Vector3(0, -40, 0));

        Vector3 dir = (line.GetPosition(1) - line.GetPosition(0)).normalized;

        RaycastHit mainRayHit;
        RaycastHit[] rayHits = new RaycastHit[4];

        Physics.Raycast(line.GetPosition(0), dir, out mainRayHit, 40f);
        Physics.Raycast(line.GetPosition(0) + (Vector3.left * range), dir, out rayHits[0], 40f);
        Physics.Raycast(line.GetPosition(0) + (Vector3.back * range), dir, out rayHits[1], 40f);
        Physics.Raycast(line.GetPosition(0) + (Vector3.right * range), dir, out rayHits[2], 40f);
        Physics.Raycast(line.GetPosition(0) + (Vector3.forward * range), dir, out rayHits[3], 40f);

        //RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out mainRayHit, 40, ignoreLayers))
        {
            Debug.Log("DETECT LAYER: " + mainRayHit.transform.gameObject.layer + " VS " + layerInt);

            //if (hit.transform.gameObject.layer == layerInt)
            if ((rayHits[0].transform.gameObject.layer == layerInt && rayHits[1].transform.gameObject.layer == layerInt &&
                 rayHits[2].transform.gameObject.layer == layerInt && rayHits[3].transform.gameObject.layer == layerInt))
                SetLineColor(Color.green);
            else SetLineColor(Color.red);
        }
        else SetLineColor(Color.red);
    }

    void SetLineColor(Color newColor)
    {
        line.startColor = newColor;
        line.endColor = newColor;
    }
}
