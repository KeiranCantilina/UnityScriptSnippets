using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InverseKinematics;

public class GenericRobotController : MonoBehaviour
{
    // Class Properties
    public bool AutoFindJoints;
    public int numberOfJoints;
    public ArticulationBody[] robotParts;
    public GameObject InverseKinematicsTarget;
    public Transform targetCartesian;
    
    // Robot joints
    private ArticulationBody[] joints;
    private ArticulationDrive[] drives;
    public Vector3[] axisDirections;


    // Misc vars
    private float[] newAngles;
    private float[] oldAngles;
    private bool moveRobot;

    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        moveRobot = false;
        // get robot
        // For auto-finding joints by name
        if (AutoFindJoints)
        {
            for (int i = 1; i - 1 < numberOfJoints; i++)
            {
                joints[i] = GameObject.Find("link_" + i.ToString()).GetComponent<ArticulationBody>();
                drives[i] = joints[i].xDrive;
            }
        }
        else
        {
            for (int i = 0; i < robotParts.Length; i++)
            {
                joints[i] = robotParts[i];
                drives[i] = joints[i].xDrive;
            }
        }


        // Axis directions THIS IS TEMPORARILY HARD CODED
        axisDirections[0] = new Vector3(0f, -1f, 0f); // This is right
        axisDirections[1] = new Vector3(1f, 0f, 0f); // This is right
        axisDirections[2] = new Vector3(1f, 0f, 0f); // This is right
        axisDirections[3] = new Vector3(0f, 0f, -1f); // This is right
        axisDirections[4] = new Vector3(1f, 0f, 0f); // This is right
        axisDirections[5] = new Vector3(0f, 0f, -1f); // This is right


    }

    // Update is called once per frame
    void Update()
    {
        if (moveRobot)
        {
            // get current robot joint positions
            for (int i=0; i<joints.Length; i++)
            {
                oldAngles[i] = joints[i].jointPosition[0];
            }

            // feed robot joints, axis angles, and dragger position to Ik calculator, get joints
            newAngles = InverseKinematics.GenericIKCalculator.RunIK(targetCartesian, InverseKinematicsTarget, joints, axisDirections, oldAngles);

            // Feed joints to articulationbody drives
            for (int i = 0; i < joints.Length; i++)
            {
                drives[i].target = newAngles[i];
            }
            // Rinse and repeat
        }
        moveRobot = false;
    }

    public void MoveRobot(Transform target)
    {
        targetCartesian = target;
        moveRobot = true;
    }
}
