using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Oculus.Interaction.GrabAPI;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EyeTrackingRay : MonoBehaviour
{
    [SerializeField] private float rayDistance = 1f;
    [SerializeField] private float rayWidth = 0.01f;
    [SerializeField] private LayerMask layersToInclude;

    [SerializeField] private Color rayColorDefaultState = Color.yellow;
    [SerializeField] private Color rayColorHoverState = Color.green;

    private LineRenderer lineRenderer;
    private OVREyeGaze ovrEyeGaze;
    private List<EyeInteractable> eyeInteractables = new List<EyeInteractable>();

    GameObject seenObject;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        ovrEyeGaze = GetComponent<OVREyeGaze>();
        SetupRay();
    }

    void SetupRay()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.startColor = rayColorDefaultState;
        lineRenderer.endColor = rayColorDefaultState;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, (Vector3.forward * rayDistance));
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Vector3 raycastDir = transform.TransformDirection(Vector3.forward) * rayDistance;

        if (Physics.Raycast(transform.position, raycastDir, out hit, Mathf.Infinity, layersToInclude))
        {
            Unselect();
            lineRenderer.startColor = rayColorHoverState;
            lineRenderer.endColor = rayColorHoverState;

            /*var eyeInteractable = hit.transform.GetComponent<EyeInteractable>();
            if (eyeInteractable != null)
            {
                eyeInteractables.Add(eyeInteractable);
                eyeInteractable.IsHovered = true;
            }*/

            GameObject eyeSee = hit.collider.gameObject;
            if (eyeSee != null)
            {
                //Debug.Log("See : " + eyeSee.name);
                if (ovrEyeGaze.Eye == OVREyeGaze.EyeId.Left)
                    EyeTrackingRecorder.instance.seenObjectL = eyeSee;
                else
                    EyeTrackingRecorder.instance.seenObjectR = eyeSee;
            }
        }
        else
        {
            lineRenderer.startColor = rayColorDefaultState;
            lineRenderer.endColor = rayColorDefaultState;
            Unselect(true);
        }
    }

    void Unselect(bool clear = false)
    {
        if (eyeInteractables.Count > 0)
        {
            foreach (var interactable in eyeInteractables)
            {
                interactable.IsHovered = false;
            }
            if (clear)
            {
                eyeInteractables.Clear();
            }
        }
    }
}
