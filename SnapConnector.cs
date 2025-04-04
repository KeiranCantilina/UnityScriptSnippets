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
    private GameObject myParent;
    //public Collider[] SnapAnchorColliders;
    public Vector3 SnapOffset;
    public Vector3 RotationOffset;
    private Quaternion savedOrientation;
    private bool isBeingDragged = false;
    private DevLocker.PhysicsUtils.DragRigidbodyBetter DraggingScript;
    private bool triggered = false;
    public float snapping_speed = 1.0f;
    private string allowableSnappingTag;

    // Start is called before the first frame update
    void Start()
    {
        allowableSnappingTag = this.tag;
        myParent = this.transform.parent.gameObject.transform.parent.gameObject;
        DraggingScript = this.transform.parent.gameObject.GetComponent<DevLocker.PhysicsUtils.DragRigidbodyBetter>();
    }

    // Update is called once per frame
    void Update()
    {
        isBeingDragged = DraggingScript.getDraggingStatus();
        if (triggered == true && !isBeingDragged)
        {
            

            var step = snapping_speed * Time.deltaTime;
            myParent.transform.position = Vector3.MoveTowards(myParent.transform.position, SnappingParent.transform.position + SnappingParent.transform.TransformDirection(SnapOffset), step);
            myParent.transform.rotation = Quaternion.RotateTowards(myParent.transform.rotation, SnappingParent.transform.rotation, step*100);
            //this.transform.localEulerAngles = Vector3.RotateTowards(this.transform.localEulerAngles, SnappingParent.transform.eulerAngles), step * 100, step * 100);
            // this part should rotate (in world orientation) to the target's orientation plus an offset in target local coordinate system converted to world rotations.

            
            if(Vector3.Distance(myParent.transform.position, SnappingParent.transform.position) < 0.001f)
            {
                snapped = true;

            }
            
        }
        if(triggered == true && snapped == false && isBeingDragged)
        {
            //Make objects face eachother
            myParent.transform.LookAt(2*myParent.transform.position - SnappingParent.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check tags to see if we need to reject collider
        if (!isFemaleConnector && other.CompareTag(allowableSnappingTag))
        {
            UnityEngine.Debug.Log("Triggered!\n");
            triggered = true;
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
