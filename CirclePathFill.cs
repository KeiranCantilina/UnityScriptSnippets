using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class CirclePathFill : MonoBehaviour
{
    private Vector3 startWorldPoint;
    private Vector3 startScreenPoint;
    private Vector3 currentMouseScreenPosition;
    private Vector3 currentMouseWorldPosition;
    private Vector3[] screenWaypoints;
    private Vector3[] worldWaypoints;
    private List<Vector3> projectedWaypoints;
    private List<Vector3> pointNormals;
    private LineRenderer lineRenderer;
    private bool mouseDown;
    private Material mat;
    public GameObject targetObject;
    private Ray[] rays;
    private Quaternion[] normals_orientations;
    private Vector3[] circle;
    private Vector3 worldCircleMiddle;
    private Vector3 circleMiddle;

    // Coarseness mode parameters
    public enum Resolution_Settings { Fine, Normal, Coarse };
    public Resolution_Settings resolutionSetting;

    // Inputs
    private int number_spirals;
    private float radius;
    private int dataPointsPerCycle;
    private float static_z;

    // Output
    private Vector3[] spiralPoints;
    private Vector3[] worldSpiralPoints;

    // Temp
    private Vector3 startPoint;
    private Vector3 currentPoint;

    // Line renderer properties
    float offset_from_clipping_plane = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        // Create line renderer
        lineRenderer = this.gameObject.AddComponent<LineRenderer>();

        // Set line renderer properties
        Material mat = Resources.Load("Material/LineRendererMaterial", typeof(Material)) as Material;
        lineRenderer.material = mat;
        lineRenderer.startWidth = 0.001f;
        lineRenderer.endWidth = 0.001f;
        lineRenderer.numCapVertices = 3;
        lineRenderer.numCornerVertices = 3;

        // Enable line renderer
        lineRenderer.enabled = true;

        // FUTURE: Convert path width to meters for easier debugging
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse button is down, therefore this is a new click
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;
            OnMouseClickDown();
        }

        // Mouse button is up, therefore this is a click release
        else if (Input.GetMouseButtonUp(0))
        {
            mouseDown = false;
            OnMouseClickUp();
        }

        // Mouse button is down and it was down in the previous frame, this is a drag
        else if (mouseDown)
        {
            OnMouseClickDrag();
        }
    }

    // Grab starting point of circle, init line drawer
    void OnMouseClickDown()
    {
        startScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + offset_from_clipping_plane);
        startWorldPoint = Camera.main.ScreenToWorldPoint(startScreenPoint);
        static_z = startScreenPoint.z;
        UnityEngine.Debug.Log("Click!");
    }

    // every frame, grab current point of mouse. 
    // Draw circle and update circle start and end points
    void OnMouseClickDrag()
    {
        // Set line renderer properties
        lineRenderer.startWidth = 0.001f;
        lineRenderer.endWidth = 0.001f;

        // Get current mouse position
        currentMouseScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + offset_from_clipping_plane);
        currentMouseWorldPosition = Camera.main.ScreenToWorldPoint(currentMouseScreenPosition);

        // Calculate middle of circle and radius
        circleMiddle = Vector3.Lerp(startScreenPoint, currentMouseScreenPosition, 0.5f);
        worldCircleMiddle = Camera.main.ScreenToWorldPoint(circleMiddle);
        radius = Vector3.Distance(circleMiddle, currentMouseScreenPosition);

        // Number of segments for circle
        int circle_steps = (int)Math.Round(radius); // Renderer gets sad if there are too many line segments in a tiny area
        float step_width = 2*Mathf.PI/circle_steps;
        circle = new Vector3[circle_steps+1];
        Vector3[] worldCircle = new Vector3[circle_steps+1];

        // Calculate circle and move to world
        for (int i = 0; i < circle_steps+1; i++) 
        {
            circle[i].x = circleMiddle.x + (radius * Mathf.Cos((i+circle_steps/3)*step_width));
            circle[i].y = circleMiddle.y + (radius * Mathf.Sin((i+circle_steps/3) * step_width));
            circle[i].z = static_z;

            worldCircle[i] = Camera.main.ScreenToWorldPoint(circle[i]);
        }
        

        // Display circle with line renderer
        lineRenderer.positionCount = worldCircle.Length;
        lineRenderer.widthMultiplier = 2f;
        lineRenderer.SetPositions(worldCircle);

        //UnityEngine.Debug.Log("Drag!");
        UnityEngine.Debug.Log(radius);
    }

    void OnMouseClickUp()
    {
        // Set resolution
        if (resolutionSetting == Resolution_Settings.Fine)
        {
            number_spirals = 10;
        }
        else if (resolutionSetting == Resolution_Settings.Normal)
        {
            number_spirals = 5;
        }
        else if (resolutionSetting == Resolution_Settings.Coarse)
        {
            number_spirals = 2;
        }

        // Calculate spiral
        dataPointsPerCycle = 50;
        int totalNumberOfDataPoints = number_spirals * dataPointsPerCycle;
        spiralPoints = new Vector3[totalNumberOfDataPoints];

        for (int i = 0; i < totalNumberOfDataPoints; i++)
        {
            spiralPoints[i].x = circleMiddle.x + (i + 1) * (radius/(dataPointsPerCycle*number_spirals)) * (Mathf.Sin((i + 1) * (2 * Mathf.PI / dataPointsPerCycle)));
            spiralPoints[i].y = circleMiddle.y + (i + 1) * (radius/(dataPointsPerCycle*number_spirals)) * (Mathf.Cos((i + 1) * (2 * Mathf.PI / dataPointsPerCycle)));
            spiralPoints[i].z = static_z;
        }

        worldSpiralPoints = new Vector3[totalNumberOfDataPoints];
        for (int i = 0; i < totalNumberOfDataPoints; i++)
        {
            worldSpiralPoints[i] = Camera.main.ScreenToWorldPoint(spiralPoints[i]);
        }

        // DEBUG: Draw spiral with line renderer
        //lineRenderer.positionCount = worldSpiralPoints.Length;
        //lineRenderer.SetPositions(worldSpiralPoints);

        // Create rays, cast them out, and collect hit position and surface normals.
        // Using lists because projectedWaypoints needs to be dynamically sized in case of rays missing the target object.
        screenWaypoints = spiralPoints;
        projectedWaypoints = new List<Vector3>();
        pointNormals = new List<Vector3>();
        Collider collider = targetObject.GetComponentInChildren<Collider>();
        rays = new Ray[screenWaypoints.Length];
        for (int i = 0; i < screenWaypoints.Length; i++)
        {
            rays[i] = Camera.main.ScreenPointToRay(screenWaypoints[i]); // Rays shooting into the virtual world from the screen camera

            RaycastHit hit;
            if (collider.Raycast(rays[i], out hit, 1000.0f)) // Cast the ray and collect data if it hits within 1000 meters (otherwise move on)
            {
                projectedWaypoints.Add(hit.point);
                pointNormals.Add(hit.normal);
            }
            else
            {
                // Do nothing
            }
        }

        // Convert surface normals into orientations using method in Quaternion class (Quaternion.LookRotation)
        normals_orientations = new Quaternion[pointNormals.Count];
        for (int i = 0; i < pointNormals.Count; i++)
        {
            normals_orientations[i] = Quaternion.LookRotation(pointNormals[i]);
        }

        // If needed, convert Quaternions into euler angles

        // Print list of waypoints

        // Feed list of waypoints to line renderer to draw path
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.positionCount = projectedWaypoints.Count;
        lineRenderer.SetPositions(projectedWaypoints.ToArray());

    }
}
            
        
    

    
    


