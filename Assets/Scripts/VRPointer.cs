using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPointer : MonoBehaviour
{
    public float pointerLength = 3f;
    private LineRenderer lineRenderer = null;

    public Vector3 endPoint;
    public GameObject collision;

    public LayerMask towerLayer;

    int layerMaskInt = ~6;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        UpdateLength();
    }

    private void UpdateLength()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, endPoint = CalculateEnd());
    }

    private Vector3 CalculateEnd()
    {
        RaycastHit hit = CreateForwardRaycast();
        Vector3 endPosition = DefaultEnd(pointerLength);

        if (hit.collider) { endPosition = hit.point; collision = hit.collider.gameObject; }

        return endPosition;
    }

    private RaycastHit CreateForwardRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        Physics.Raycast(ray, out hit, pointerLength, layerMaskInt);
        return hit;
    }

    private Vector3 DefaultEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }
}
