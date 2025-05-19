using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualPathDraw : MonoBehaviour
{
    private Vector3 startWorldPoint;
    private Vector3 startScreenPoint;
    private Vector3 currentMouseScreenPosition;
    private Vector3 currentMouseWorldPosition;
    private Vector3 rectPoint2;
    private Vector3 rectPoint4;
    private Vector3[] worldCorners;
    private List<Vector3> screenWaypoints;
    private List<Vector3> worldWaypoints;
    private List<Ray> cameraRays;
    private List<Vector3> projectedWaypoints;
    private List<Vector3> pointNormals;
    private LineRenderer lineRenderer;
    public bool CoarseMode;
    private bool mouseDown;
    private Material mat;
    public GameObject targetObject;
    private Ray[] rays;
    private Quaternion[] normals_orientations;

    // Line renderer properties
    private float offset_from_clipping_plane = 0.1f;

    // Linear interpolation points between waypoints
    private int lerp_n = 10;


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

    void OnMouseClickDown()
    {
        // collect point in screen space
        startScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + offset_from_clipping_plane);
        startWorldPoint = Camera.main.ScreenToWorldPoint(startScreenPoint);
        UnityEngine.Debug.Log("Click!");

        // add to list of points in screen space
        screenWaypoints.Add(startScreenPoint);
        worldWaypoints.Add(startWorldPoint);

        // Add to list of raycasting rays for later use
        cameraRays.Add(Camera.main.ScreenPointToRay(startWorldPoint));
    }

    void OnMouseClickUp()
    {
        // Lerp between previous point on object and new point in world space to generate n waypoints (unless this is the first point)
        Collider collider = targetObject.GetComponentInChildren<Collider>();
        RaycastHit hit;
        if (projectedWaypoints.Count > 0)
        {
            Vector3 lerp_point = new Vector3();
            Vector3 lerp_rayDirection = new Vector3();


            for (int i = 0; i < lerp_n; i++)
            {
                lerp_point = Vector3.Lerp(projectedWaypoints[projectedWaypoints.Count], worldWaypoints[worldWaypoints.Count], (i+1)/lerp_n); // Last lerp point is same as new point

                // Lerp between previous camera ray vector and new camera ray vector to generate vector
                lerp_rayDirection = Vector3.Lerp(cameraRays[cameraRays.Count - 1].direction, cameraRays[cameraRays.Count].direction, (i + 1) / lerp_n);

                // Create rays and raycast to find object points (variable number of points depending on if clicked point actually intersects object)
                Ray currentRay = new Ray(lerp_point, lerp_rayDirection);
                
                // If we actually hit the object, add the resultant object point to our list
                if (collider.Raycast(currentRay, out hit, 1000.0f))
                {
                    projectedWaypoints.Add(hit.point);
                    pointNormals.Add(hit.normal);
                }
                else
                {
                    // Do nothing
                }

            }
        }
        else 
        {
            // This is the first point. As long as it hits the object, we keep it.
            Ray currentRay = Camera.main.ScreenPointToRay(screenWaypoints[screenWaypoints.Count]);

            // If we actually hit the object, add the resultant object point to our list
            if (collider.Raycast(currentRay, out hit, 1000.0f))
            {
                projectedWaypoints.Add(hit.point);
                pointNormals.Add(hit.normal);
            }
            else
            {
                // Do nothing
            }
        }

        // Throw points in object space in line renderer
        // Feed list of waypoints to line renderer to draw path
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.positionCount = projectedWaypoints.Count;
        lineRenderer.SetPositions(projectedWaypoints.ToArray());
    }

    void OnMouseClickDrag()
    {
        // Do nothing
    }
}
