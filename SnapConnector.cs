// Assumes objects that will be snapped together have "forward" defined as the positive Z axis. If not, use the "RotationOffset" property to adjust.
// Written by Keiran Cantilina, 4-April-2025

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;

public class SnapConnector : MonoBehaviour
{
    public bool isFemaleConnector;
    private bool snapped = false;
    private GameObject SnappingParent;
    private GameObject CollidingWith;
    private GameObject myParent;
    //public Vector3 SnapOffset; // No longer needed because we automatically calculate snap offsets based on snap connector positions
    public Vector3 RotationOffset;
    private Quaternion savedOrientation;
    private bool isBeingDragged = false;
    private DevLocker.PhysicsUtils.DragRigidbodyBetter DraggingScript;
    private bool triggered = false;
    public float snapping_speed = 1.0f;
    private string allowableSnappingTag;
    public Vector3 ManualPositionOffset;

    // Start is called before the first frame update
    void Start()
    {
        // We're allowed to snap with anything else with this tag
        allowableSnappingTag = this.tag;

        // Get default parent transform and associated stuff (should be object this snap connector is attached to)
        myParent = this.transform.parent.gameObject.transform.parent.gameObject;
        DraggingScript = this.transform.parent.gameObject.GetComponent<DevLocker.PhysicsUtils.DragRigidbodyBetter>();
    }

    // Update is called once per frame
    void Update()
    {
        // update dragging status
        isBeingDragged = DraggingScript.getDraggingStatus();

        // If we're in snapping range and we're not being dragged:
        if (triggered == true && !isBeingDragged)
        {
            // Set snapping animation distance per frame
            var step = snapping_speed * Time.deltaTime;

            //Get snap offsets from snapping connector local positions, then transform to global coords
            Vector3 SnapOffsetFromOther = SnappingParent.transform.InverseTransformPoint(CollidingWith.transform.position);
            Vector3 SnapOffsetFromThis = myParent.transform.InverseTransformPoint(this.transform.position);

            // Snap!
            myParent.transform.position = Vector3.MoveTowards(myParent.transform.position, SnappingParent.transform.position + SnappingParent.transform.TransformDirection(-SnapOffsetFromThis + SnapOffsetFromOther + ManualPositionOffset), step); // Changed
            myParent.transform.rotation = Quaternion.RotateTowards(myParent.transform.rotation, SnappingParent.transform.rotation, step*100);

            // If we're close enough, consider us snapped
            if(Vector3.Distance(myParent.transform.position, SnappingParent.transform.position) < 0.001f)
            {
                snapped = true;

            }
        }

        // If we're not snapped but we're in snapping range, Look At 
        if(triggered == true && snapped == false && isBeingDragged)
        {
            //Make objects face eachother
            var step = snapping_speed * Time.deltaTime;
            //myParent.transform.LookAt(2*myParent.transform.position - SnappingParent.transform.position); // For instantaneous LookAt
            Quaternion LookatRotation = Quaternion.LookRotation(2 * myParent.transform.position - SnappingParent.transform.position); // For animated LookAt
            myParent.transform.rotation = Quaternion.RotateTowards(myParent.transform.rotation,LookatRotation, step*25);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check tags to see if we need to reject collider
        if (!isFemaleConnector && other.CompareTag(allowableSnappingTag))
        {
            UnityEngine.Debug.Log("Triggered!\n");
            triggered = true;
            CollidingWith = other.gameObject;
            SnappingParent = other.gameObject.transform.parent.gameObject.transform.parent.gameObject;

            // SnappingParent becomes actual parent here
            myParent.transform.parent = SnappingParent.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check tags to see if we need to reject collider
        if (!isFemaleConnector && other.CompareTag(allowableSnappingTag))
        {
            //this.transform.rotation = savedOrientation;
            UnityEngine.Debug.Log("Collider Exit\n");
            myParent.transform.parent = GameObject.Find("Workcell World").transform;
            triggered = false;
            snapped = false;
        }
    }
}
