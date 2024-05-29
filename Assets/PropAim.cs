using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropAim : MonoBehaviour
{
    LineRenderer line;
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position + new Vector3(0, -40, 0));
    }
}
