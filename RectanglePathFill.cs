using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectanglePathFill : MonoBehaviour
{
    private Vector3 startWorldPoint;
    private Vector3 startScreenPoint;
    private Vector3 currentMouseScreenPosition;
    private Vector3 currentMouseWorldPosition;
    private Vector3 rectPoint2;
    private Vector3 rectPoint4;
    private Vector3[] worldCorners;
    private Vector3[] screenWaypoints;
    private Vector3[] worldWaypoints;
    private List<Vector3> projectedWaypoints;
    private List<Vector3> pointNormals;
    private LineRenderer lineRenderer;
    public bool CoarseMode;
    private bool mouseDown;
    private Material mat;
    public GameObject targetObject;
    private Ray[] rays;
    private Quaternion[] normals_orientations;

    // Coarse mode vs fine mode parameters
    int FineModeSlices = 8;
    int CoarseModeSlices = 5;

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
        else if(Input.GetMouseButtonUp(0))
        {
            mouseDown = false;
            OnMouseClickUp();
        }

        // Mouse button is down and it was down in the previous frame, this is a drag
        else if(mouseDown)
        {
            OnMouseClickDrag();
        }
    }

    // Grab starting point of rectangle, init line drawer
    void OnMouseClickDown()
    {
        startScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane+offset_from_clipping_plane);
        startWorldPoint = Camera.main.ScreenToWorldPoint(startScreenPoint);
        UnityEngine.Debug.Log("Click!");
    }

    // every frame, grab current point of mouse. 
    // Draw rectangle and update rectangle start and end points
    void OnMouseClickDrag()
    {
        // Set line renderer properties
        lineRenderer.startWidth = 0.001f;
        lineRenderer.endWidth = 0.001f;

        // Get current mouse position
        currentMouseScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + offset_from_clipping_plane);
        currentMouseWorldPosition = Camera.main.ScreenToWorldPoint(currentMouseScreenPosition);

        // Calculate other two points of rectangle and create rectangle
        rectPoint2 = new Vector3(startScreenPoint.x, currentMouseScreenPosition.y, Camera.main.nearClipPlane + offset_from_clipping_plane);
        rectPoint4 = new Vector3(currentMouseScreenPosition.x, startScreenPoint.y, Camera.main.nearClipPlane + offset_from_clipping_plane);

        // Draw rectangle with line renderer
        worldCorners = new Vector3[5];
        worldCorners[0] = startWorldPoint;
        worldCorners[1] = Camera.main.ScreenToWorldPoint(rectPoint2);
        worldCorners[2] = currentMouseWorldPosition;
        worldCorners[3] = Camera.main.ScreenToWorldPoint(rectPoint4);
        worldCorners[4] = startWorldPoint;
        lineRenderer.positionCount = worldCorners.Length;
        lineRenderer.SetPositions(worldCorners);
        //UnityEngine.Debug.Log("Drag!");
    }


    void OnMouseClickUp()
    {
        UnityEngine.Debug.Log("Up!");
        int slices = 0;
        if (CoarseMode)
        {
            slices = CoarseModeSlices;
        }
        else
        {
            slices = FineModeSlices;
        }

        // Figure out quadrant
        int x_sign = Math.Sign(currentMouseScreenPosition.x - startScreenPoint.x); ;
        int y_sign = Math.Sign(currentMouseScreenPosition.y - startScreenPoint.y); ;

        // Find rectangle width
        float width = Math.Abs(currentMouseScreenPosition.x - startScreenPoint.x);
        float height = Math.Abs(currentMouseScreenPosition.y - startScreenPoint.y);

        // Divide width into slices
        float sliceWidth = width / slices;

        // Calculate number of waypoints
        int numberWaypoints = (2 * slices) + 2;
        screenWaypoints = new Vector3[numberWaypoints];

        // Start populating waypoints

        // First waypoint is startScreenPoint
        screenWaypoints[0] = startScreenPoint;

        // All even waypoints are x displacements, and all odd waypoints are y displacements
        float previous_x = startScreenPoint.x;
        float previous_y = startScreenPoint.y;
        float current_x = 0;
        float current_y = 0;
        float static_z = startScreenPoint.z;

        for (int i = 1; i < numberWaypoints; i++) // i = 1 not 0 because first waypoint is startScreenPoint
        {
            if (i % 2 == 0)
            {
                // is even
                current_x = previous_x + x_sign * sliceWidth;
                current_y = previous_y;
            }
            else
            {
                // is odd
                current_x = previous_x;
                current_y = previous_y + y_sign * height;

                // Invert y sign cause on the next odd waypoint we zig zag back the other way
                y_sign = y_sign * -1;
            }

            // Shove into screenWaypoints array
            screenWaypoints[i] = new Vector3(current_x, current_y, static_z);

            // Update "previous"
            previous_y = current_y;
            previous_x = current_x;
        }

        // Create additional waypoints by linear interpolation
        List<Vector3> temporaryWaypointList = new List<Vector3> ();
        for(int i = 0; i < screenWaypoints.Length; i++) 
        {
            if (i < screenWaypoints.Length - 1) // Can't run the loop all the way to the last waypoint, cause there's nothing to interpolate with
            {
                temporaryWaypointList.Add(screenWaypoints[i]);
                temporaryWaypointList.Add(Vector3.Lerp(screenWaypoints[i], screenWaypoints[i + 1], 0.5f));
            }
            else
            {
                temporaryWaypointList.Add(screenWaypoints[i]); // Add last waypoint
            }
        }
        

        // Throw list back into screenWaypoints cause it's too much work to refactor the code I already wrote before adding this part
        screenWaypoints = new Vector3[temporaryWaypointList.Count];
        screenWaypoints = temporaryWaypointList.ToArray();
        numberWaypoints = screenWaypoints.Length;

        // Convert screen waypoints to world waypoints
        worldWaypoints = new Vector3[numberWaypoints];
        for (int i = 0; i < screenWaypoints.Length; i++)
        {
            worldWaypoints[i] = Camera.main.ScreenToWorldPoint(screenWaypoints[i]);
        }

        // Create rays, cast them out, and collect hit position and surface normals.
        // Using lists because projectedWaypoints needs to be dynamically sized in case of rays missing the target object.
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
        for (int i = 0;i < pointNormals.Count; i++)
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