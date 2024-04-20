using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreateWayPoint : MonoBehaviour
{

    private int ClickCount;
    private List<GameObject> Children;
    private int numberChildren;
    public string setKey;
    public string eraseKey;
    public GameObject InstrumentObject;
    public LineRenderer LineRenderer;
    public TextMeshPro DistanceText;
    public bool Spheres;
    public float SphereSize;

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
            // Set coord to current coord of instrument
            Children[ClickCount].transform.SetPositionAndRotation(InstrumentObject.transform.localPosition, InstrumentObject.transform.localRotation);

            // Add Mesh object (if we want it)
            if (Spheres)
            {
                Children[ClickCount] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }

            // Scale Object down
            Children[ClickCount].transform.localScale = new Vector3(SphereSize, SphereSize, SphereSize);

            // Activate current object to make it visible
            Children[ClickCount].SetActive(true);

            // Increment click count
            ClickCount++;

            // Add to Line Renderer
            LineRenderer.positionCount = ClickCount;
            LineRenderer.SetPosition(ClickCount-1, Children[ClickCount-1].transform.position);

            // If we're out of children, create new child
            if (ClickCount > numberChildren)
            {
                var newChild = new GameObject($"Sphere ({numberChildren + 1})");
                newChild.SetActive(false);
                newChild.transform.SetParent(this.transform, false);
                newChild.tag = "Waypoint";
                //Instantiate(newChild, this.transform);
                Children.Add(newChild);
                numberChildren = Children.Count;
            }
        }

        if (Input.GetKeyDown(eraseKey))
        {
            if (ClickCount > 0)
            {
                // Deactivate previous object so user can't see it and retract click count
                ClickCount = ClickCount - 1;
                Children[ClickCount].SetActive(false);
            }
            
        }

        // Based on click count, calculate distances between waypoints
        if (ClickCount > 1)
        {
            float totaldistance = 0;
            for (int i = 1; i < ClickCount; i++){
                totaldistance = totaldistance + Mathf.Abs(Vector3.Distance(Children[i].transform.position, Children[i - 1].transform.position));
            }

            // Display perimeter
            DistanceText.transform.position = Vector3.Lerp(Children[0].transform.position, Children[ClickCount-1].transform.position, 0.5f);
            DistanceText.text = totaldistance.ToString("#.00");
        }
    }

    

}
