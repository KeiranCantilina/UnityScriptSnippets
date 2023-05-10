using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineBetweenSensors : MonoBehaviour
{
    public GameObject[] TrackingPoints; // The tracking sensor objects; remember to assign in inspector!
    public LineRenderer lineRenderer;   // The line renderer component; remember to assign in inspector!
    
    // Start is called before the first frame update
    void Start(){

        // Set the position count of the linerenderer
        lineRenderer.positionCount = TrackingPoints.Length;

        // Get the transforms of the objects and set the positions of the LineRenderer using them
        for (int i = 0;i<lineRenderer.positionCount;i++){
            lineRenderer.SetPosition(i, TrackingPoints[i].transform.position);
        }

    }

    
    // Update is called once per frame
    void Update(){
       
    }
}
