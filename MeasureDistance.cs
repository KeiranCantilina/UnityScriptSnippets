using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MeasureDistance : MonoBehaviour
{
    public GameObject InstrumentObjectLeft;
    public GameObject InstrumentObjectRight;
    public TextMeshPro OutputTextMesh;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Measure distance
        float distance = Vector3.Distance(InstrumentObjectLeft.transform.position, InstrumentObjectRight.transform.position);
        
        // Display distance
        OutputTextMesh.text = distance.ToString("#.00");
        
        // Have this object constantly between the two instruments
        OutputTextMesh.transform.position = Vector3.Lerp(InstrumentObjectLeft.transform.position, InstrumentObjectRight.transform.position, 0.5f);
    }
}
