using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropAim : MonoBehaviour
{
    [SerializeField] LayerMask detectLayer;
    LineRenderer line;
    int layerInt = 0;
    Vector3 downCheck;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        layerInt = detectLayer.value;
    }

    // Update is called once per frame
    void Update()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position + new Vector3(0, -40, 0));

        if (Physics.Raycast(transform.position, Vector3.down, 40, layerInt))
        {
            //print("There is something in front of the object!");
            SetLineColor(Color.green);
        }
        else SetLineColor(Color.red);
    }

    void SetLineColor(Color newColor)
    {
        line.startColor = newColor;
        line.endColor = newColor;
    }
}
