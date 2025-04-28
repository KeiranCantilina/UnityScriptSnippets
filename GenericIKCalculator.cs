using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MathNet.Numerics.LinearAlgebra.Single;
using static System.Net.Mime.MediaTypeNames;

namespace InverseKinematics
{
    public class GenericIKCalculator : MonoBehaviour
    {
        // robot
        //private GameObject[] joint = new GameObject[6];
        public ArticulationBody[] joint = new ArticulationBody[6];
        public GameObject Target_Object;
        //private GameObject[] arm = new GameObject[6];
        //private float[] armL = new float[6];
        //private Vector3[] angle = new Vector3[6];
        private float[] angle = new float[6];
        private float[] prevAngle = new float[6];
        private Vector3[] dim = new Vector3[6];             // local dimensions of each joint
        private Vector3[] point = new Vector3[7];           // world position of joint end
        public Vector3[] axis = new Vector3[6];            // local direction of each axis
        private Quaternion[] rotation = new Quaternion[6];  // local rotation(quaternion) of joint relative to its parent
        private Quaternion[] wRotation = new Quaternion[6]; // world rotation(quaternion) of joint
        private Vector3 pos;                                // reference(target) position
        private Vector3 rot;                                // reference(target) pose
        private float lambda = 0.1f;
        private float[] minAngle = new float[6];            // limits of joint rotatation
        private float[] maxAngle = new float[6];
        private bool outOfLimit;


        // Start is called before the first frame update
        void Start()
        {
            // For auto-finding joints by name
            for (int i = 1; i - 1 < joint.Length; i++)
            {
                joint[i] = GameObject.Find("link_" + i.ToString()).GetComponent<ArticulationBody>();
            }


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
            axis[0] = new Vector3(0f, -1f, 0f); // This is right
            axis[1] = new Vector3(1f, 0f, 0f); // This is right
            axis[2] = new Vector3(1f, 0f, 0f); // This is right
            axis[3] = new Vector3(0f, 0f, -1f); // This is right
            axis[4] = new Vector3(1f, 0f, 0f); // This is right
            axis[5] = new Vector3(0f, 0f, -1f); // This is right

            // Initial Pose Angle TO DO: Make this set to what the current pose angle is
            angle[0] = prevAngle[0] = 0f;
            angle[1] = prevAngle[1] = 0f;
            angle[2] = prevAngle[2] = 0f;
            angle[3] = prevAngle[3] = 0f;
            angle[4] = prevAngle[4] = 0f;
            angle[5] = prevAngle[5] = 0f;

            // Get Joint Limits
            for (int i = 0; i < joint.Length; i++) // You can set different values for each joint.
            {
                minAngle[i] = joint[i].xDrive.lowerLimit;
                maxAngle[i] = joint[i].xDrive.upperLimit;
            }

        }

        // Update is called once per frame
        void Update()
        {
            
        }

        // Method to calculate Inverse Kinematics
        float[] CalcIK(Transform input)    
        {
            pos.x = input.position.x;
            pos.y = input.position.y;
            pos.z = input.position.z;
            rot.x = input.eulerAngles[0];
            rot.y = input.eulerAngles[1];
            rot.z = input.eulerAngles[2];

            int count = 0;
            outOfLimit = false;
            for (int i = 0; i < 100; i++)   // Iterate maz 99 times to converge on solution
            {
                count = i;
                // find position/pose of hand
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

        void ForwardKinematics()
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

        DenseMatrix CalcErr()
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

        DenseMatrix CalcJacobian()
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

        bool IsOutOfLimit()
        {
            return outOfLimit;
        }

        
    }
}
