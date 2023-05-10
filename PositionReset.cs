using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionReset : MonoBehaviour
{

    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;


    // Start is called before the first frame update
    void Start()
    {
        startPos = this.transform.position;
        startRot = this.transform.rotation;
        startScale = this.transform.localScale;
    }

    // Reset Position
    public void resetPosition()
    {
        this.transform.position = startPos;
        this.transform.rotation = startRot;
        this.transform.localScale = startScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
