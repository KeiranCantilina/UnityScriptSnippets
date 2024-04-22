using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreateWayPoint : MonoBehaviour
{

    public int ClickCount;
    private List<GameObject> Children;
    private int numberChildren;
    public string setKey;
    public string eraseKey;
    public GameObject InstrumentObject;
    public LineRenderer LineRenderer;
    public TextMeshPro DistanceText;
    public bool Spheres;
    public float SphereSize;
    public int ScalingFactor;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize List of Children
        Children = new List<GameObject> { };
        ClickCount = 0;

        // Grab all child objects that have the "Waypoint" tag and shove into Children list
        foreach (Transform child in this.transform)
        {
            if (child.CompareTag("Waypoint"))
            {
                Children.Add(child.gameObject);
            }
        }

        numberChildren = Children.Count;
        LineRenderer.positionCount = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(setKey))
        {
            // If we're out of children, create new child
            if (ClickCount+1 > numberChildren)
            {
                var newChild = new GameObject($"Sphere ({numberChildren + 1})");

                // If we want it to have a sphere mesh
                if (Spheres)
                {
                    newChild = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    newChild.name = $"Sphere ({numberChildren + 1})";
                }

                newChild.SetActive(false);
                newChild.transform.SetParent(this.transform, false);
                newChild.tag = "Waypoint";

                // Scale Object down (we do this even if there's no mesh, just to keep things consistent
                newChild.transform.localScale = new Vector3(SphereSize, SphereSize, SphereSize);

                //Instantiate(newChild, this.transform);
                Children.Add(newChild);
                numberChildren = Children.Count;
            }

            // Set coord to current coord of instrument
            Children[ClickCount].transform.SetPositionAndRotation(InstrumentObject.transform.localPosition, InstrumentObject.transform.localRotation);

            // Activate current object to make it visible
            Children[ClickCount].SetActive(true);

            // Increment click count
            ClickCount++;

            // Add to Line Renderer
            LineRenderer.positionCount = ClickCount;
            LineRenderer.SetPosition(ClickCount-1, Children[ClickCount-1].transform.position); 
        }

        if (Input.GetKeyDown(eraseKey))
        {
            if (ClickCount > 0)
            {
                // Deactivate previous object so user can't see it and retract click count
                ClickCount = ClickCount - 1;
                Children[ClickCount].SetActive(false);

                // Remove from Line renderer
                LineRenderer.positionCount = ClickCount;

                // Delete extra objects
                if (ClickCount > 1)
                {
                    Destroy(Children[ClickCount]);
                    Children.Remove(Children[ClickCount]);
                    numberChildren = numberChildren - 1;
                }
                
            }
            else
            {
                Debug.Log("No waypoints to erase!");
            }
            
        }

        // Based on click count, calculate distances between waypoints
        if (ClickCount > 1)
        {
            float totaldistance = 0;
            for (int i = 1; i < ClickCount; i++){
                totaldistance = totaldistance + Mathf.Abs(Vector3.Distance(Children[i].transform.position, Children[i - 1].transform.position))*ScalingFactor;
            }

            // Display perimeter
            DistanceText.transform.position = Vector3.Lerp(Children[0].transform.position, Children[ClickCount-1].transform.position, 0.5f);
            DistanceText.text = totaldistance.ToString("#.00");
        }
        else
        {
            DistanceText.text = "";
        }
    }

    

}
