using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropAim : MonoBehaviour
{
    [SerializeField] LayerMask detectLayer;
    [SerializeField] LayerMask ignoreLayers;
    LineRenderer line;
    int layerInt = 0;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        layerInt = (int) Mathf.Log(detectLayer.value, 2);
        ignoreLayers = ~ignoreLayers;
    }

    // Update is called once per frame
    void Update()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position + new Vector3(0, -40, 0));

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 40, ignoreLayers))
        {
            Debug.Log("DETECT LAYER: " + hit.transform.gameObject.layer + " VS " + layerInt);

            if (hit.transform.gameObject.layer == layerInt)
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
