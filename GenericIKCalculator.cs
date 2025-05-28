using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MathNet.Numerics.LinearAlgebra.Single;
using static System.Net.Mime.MediaTypeNames;

//using InverseKinematic;
namespace KeiranUtils
{
    public static class GenericIKCalculator
    {

        // robot
        //private GameObject[] joint = new GameObject[6];
        public static ArticulationBody[] joint;
        public static GameObject Target_Object;
        //private GameObject[] arm = new GameObject[6];
        //private float[] armL = new float[6];
        //private Vector3[] angle = new Vector3[6];
        private static float[] angle;
        private static float[] prevAngle;
        private static Vector3[] dim;             // local dimensions of each joint
        private static Vector3[] point;           // world position of joint end
        public static Vector3[] axis;            // local direction of each axis
        private static Quaternion[] rotation;  // local rotation(quaternion) of joint relative to its parent
        private static Quaternion[] wRotation; // world rotation(quaternion) of joint
        private static Vector3 pos;                                // reference(target) position
        private static Vector3 rot;                                // reference(target) pose
        private static float lambda = 0.1f;
        private static float[] minAngle;            // limits of joint rotatation
        private static float[] maxAngle;
        private static bool outOfLimit;


        // Start is called before the first frame update
        static public float[] RunIK(Transform input, GameObject targetBody, ArticulationBody[] robotJoints, Vector3[] axisDirections, float[] previousJointPositions)
        {
            // Process input
            pos.x = input.position.x;
            pos.y = input.position.y;
            pos.z = input.position.z;
            rot.x = input.eulerAngles[0];
            rot.y = input.eulerAngles[1];
            rot.z = input.eulerAngles[2];
            Target_Object = targetBody;

            // For auto-finding joints by name
            joint = robotJoints;

            // now we know number of dof from joint, so we can define some stuff.
            dim = new Vector3[joint.Length];
            point = new Vector3[joint.Length + 1];
            axis = new Vector3[joint.Length];
            rotation = new Quaternion[joint.Length];
            wRotation = new Quaternion[joint.Length];
            angle = new float[joint.Length];

            // Position of each robot segment relative to its parent
            for (int i = 0; i < dim.Length - 1; i++)
            {
                dim[i] = joint[i + 1].transform.localPosition;
            }

            // Position of IK target relative to the last robot link
            // TO DO: test this, if it breaks, use commented out part.
            //dim[dim.Length - 1] = Target_Object.transform.localPosition;
            dim[dim.Length - 1] = joint[joint.Length-1].transform.InverseTransformPoint(Target_Object.transform.position);

            // Axis directions
            /*axis[0] = new Vector3(0f, -1f, 0f); // This is right
            axis[1] = new Vector3(1f, 0f, 0f); // This is right
            axis[2] = new Vector3(1f, 0f, 0f); // This is right
            axis[3] = new Vector3(0f, 0f, -1f); // This is right
            axis[4] = new Vector3(1f, 0f, 0f); // This is right
            axis[5] = new Vector3(0f, 0f, -1f); // This is right*/
            // For auto-finding joints by name
            axis = axisDirections;
            

            // Initial Pose Angle TO DO: Make this set to what the current pose angle is
            /*angle[0] = prevAngle[0] = 0f;
            angle[1] = prevAngle[1] = 0f;
            angle[2] = prevAngle[2] = 0f;
            angle[3] = prevAngle[3] = 0f;
            angle[4] = prevAngle[4] = 0f;
            angle[5] = prevAngle[5] = 0f;*/

            prevAngle = previousJointPositions;


            // Get Joint Limits
            minAngle = new float[joint.Length];
            maxAngle = new float[joint.Length];
            for (int i = 0; i < joint.Length; i++) // You can set different values for each joint.
            {
                minAngle[i] = joint[i].xDrive.lowerLimit;
                maxAngle[i] = joint[i].xDrive.upperLimit;
            }

            return CalcIK();

        }


        // Method to calculate Inverse Kinematics
        private static float[] CalcIK()    
        {
            int count = 0;
            outOfLimit = false;
            for (int i = 0; i < 100; i++)   // Iterate maz 99 times to converge on solution
            {
                count = i;
                // find position/pose of hand (Forward Kinematics)
                ForwardKinematics();

                // calculate position/pose error from reference
                var err = CalcErr();    // 6x1 matrix(vector)
                float err_norm = (float)err.L2Norm();
                if (err_norm < 1E-3)
                {
                    for (int ii = 0; ii < joint.Length; ii++)
                    {
                        if (angle[ii] < minAngle[ii] || angle[ii] > maxAngle[ii])
                        {
                            outOfLimit = true;
                            break;
                        }
                    }
                    break;
                }

                // create jacobian
                var J = CalcJacobian(); // 6x6 matrix

                // correct angle of joints
                var dAngle = lambda * J.PseudoInverse() * err; // 6x1 matrix
                for (int ii = 0; ii < joint.Length; ii++)
                {
                    angle[ii] += dAngle[ii, 0] * Mathf.Rad2Deg;
                }
            }

            if (count == 99 || outOfLimit)  // did not converge or angle out of limit
            {
                for (int i = 0; i < joint.Length; i++) // reset slider
                {
                    // Do nothing
                    angle[i] = prevAngle[i];
                }
                return angle;
            }
            else // Set output anagles
            {
                for (int i = 0; i < joint.Length; i++)
                {
                    prevAngle[i] = angle[i];
                }
                return angle;
            }
        }

        private static void ForwardKinematics()
        {
            point[0] = joint[0].transform.position;
            wRotation[0] = Quaternion.AngleAxis(angle[0], axis[0]);
            for (int i = 1; i < joint.Length; i++)
            {
                point[i] = wRotation[i - 1] * dim[i - 1] + point[i - 1];
                rotation[i] = Quaternion.AngleAxis(angle[i], axis[i]);
                wRotation[i] = wRotation[i - 1] * rotation[i];
            }
            point[joint.Length] = wRotation[joint.Length - 1] * dim[joint.Length - 1] + point[joint.Length - 1];
        }

        private static DenseMatrix CalcErr()
        {
            // position error
            Vector3 perr = pos - point[6];
            // pose error
            Quaternion rerr = Quaternion.Euler(rot) * Quaternion.Inverse(wRotation[5]);
            // make error vector
            Vector3 rerrVal = new Vector3(rerr.eulerAngles.x, rerr.eulerAngles.y, rerr.eulerAngles.z);
            if (rerrVal.x > 180f) rerrVal.x -= 360f;
            //if (rerrVal.x < 180f) rerrVal.x += 360f;
            if (rerrVal.y > 180f) rerrVal.y -= 360f;
            //if (rerrVal.y < 180f) rerrVal.y += 360f;
            if (rerrVal.z > 180f) rerrVal.z -= 360f;
            //if (rerrVal.z < 180f) rerrVal.z += 360f;
            var err = DenseMatrix.OfArray(new float[,]
            {
                { perr.x },
                { perr.y },
                { perr.z },
                { rerrVal.x * Mathf.Deg2Rad},
                { rerrVal.y * Mathf.Deg2Rad},
                { rerrVal.z * Mathf.Deg2Rad}
            });
            return err;
        }

        private static DenseMatrix CalcJacobian()
        {
            Vector3 w0 = wRotation[0] * axis[0];
            Vector3 w1 = wRotation[1] * axis[1];
            Vector3 w2 = wRotation[2] * axis[2];
            Vector3 w3 = wRotation[3] * axis[3];
            Vector3 w4 = wRotation[4] * axis[4];
            Vector3 w5 = wRotation[5] * axis[5];
            Vector3 p0 = Vector3.Cross(w0, point[6] - point[0]);
            Vector3 p1 = Vector3.Cross(w1, point[6] - point[1]);
            Vector3 p2 = Vector3.Cross(w2, point[6] - point[2]);
            Vector3 p3 = Vector3.Cross(w3, point[6] - point[3]);
            Vector3 p4 = Vector3.Cross(w4, point[6] - point[4]);
            Vector3 p5 = Vector3.Cross(w5, point[6] - point[5]);

            var J = DenseMatrix.OfArray(new float[,]
            {
                { p0.x, p1.x, p2.x, p3.x, p4.x, p5.x },
                { p0.y, p1.y, p2.y, p3.y, p4.y, p5.y },
                { p0.z, p1.z, p2.z, p3.z, p4.z, p5.z },
                { w0.x, w1.x, w2.x, w3.x, w4.x, w5.x  },
                { w0.y, w1.y, w2.y, w3.y, w4.y, w5.y  },
                { w0.z, w1.z, w2.z, w3.z, w4.z, w5.z  }
            });
            return J;
        }

        public static bool IsOutOfLimit()
        {
            return outOfLimit;
        }

        
    }
}
