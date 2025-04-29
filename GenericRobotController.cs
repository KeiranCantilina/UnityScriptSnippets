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
            joints = new ArticulationBody[numberOfJoints];
            for (int i = 1; i - 1 < numberOfJoints; i++)
            {
                joints[i] = GameObject.Find("link_" + i.ToString()).GetComponent<ArticulationBody>();
            }
        }
        else
        {
            joints = robotParts;
        }


        // Axis directions THIS IS TEMPORARILY HARD CODED
        axisDirections = new Vector3[joints.Length];
        axisDirections[0] = new Vector3(0f, -1f, 0f); // This is right
        axisDirections[1] = new Vector3(1f, 0f, 0f); // This is right
        axisDirections[2] = new Vector3(1f, 0f, 0f); // This is right
        axisDirections[3] = new Vector3(0f, 0f, -1f); // This is right
        axisDirections[4] = new Vector3(1f, 0f, 0f); // This is right
        axisDirections[5] = new Vector3(0f, 0f, -1f); // This is right

        // get current robot joint positions
        oldAngles = new float[joints.Length];
        for (int i = 0; i < joints.Length; i++)
        {
            oldAngles[i] = joints[i].jointPosition[0];
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (moveRobot)
        {
            // feed robot joints, axis angles, and dragger position to Ik calculator, get joints
            newAngles = InverseKinematics.GenericIKCalculator.RunIK(targetCartesian, InverseKinematicsTarget, joints, axisDirections, oldAngles);

            //DEBUG
            //UnityEngine.Debug.Log(newAngles[1]);
            
            // Feed joints to articulationbody drives
            for (int i = 0; i < joints.Length; i++)
            {
                var drive = joints[i].xDrive;
                drive.target = newAngles[i];
                joints[i].xDrive = drive;
            }
            
            // Rinse and repeat
            oldAngles = newAngles;
        }
        moveRobot = false;
    }

    public void MoveRobot(Vector3 targetPos, Vector3 targetRot)
    {
        targetCartesian.position = targetPos;
        targetCartesian.eulerAngles = targetRot;
        moveRobot = true;
    }
}
