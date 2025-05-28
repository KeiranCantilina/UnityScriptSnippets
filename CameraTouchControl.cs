//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRubyShared;

namespace KeiranUtils
{
    /// <summary>
    /// Allows moving a camera in 3D space using pan, tilt, rotate and zoom mechanics
    /// </summary>
    [AddComponentMenu("Keiran Utils/CameraTouchControl")]
    public class CameraTouchControl : MonoBehaviour
    {
        /// <summary>The transform to move, defaults to the transform on this script</summary>
        [Tooltip("The transform to move, defaults to the transform on this script")]
        public Transform Target;

        /// <summary>Controls pan (left/right for strafe, up/down for forward/back) speed in number of world units per screen units panned</summary>
        [Range(-10.0f, 10.0f)]
        [Tooltip("Controls pan (left/right for strafe, up/down for forward/back) speed in number of world units per screen units panned")]
        public float PanSpeed = -5.0f;

        /// <summary>Controls zoom in/out speed</summary>
        [Range(-10.0f, 10.0f)]
        [Tooltip("Controls zoom in/out speed")]
        public float ZoomSpeed = 10.0f;

        /// <summary>Rotation speed</summary>
        [Tooltip("Rotation speed")]
        [Range(-100.0f, 100.0f)]
        public float RotationSpeed = 20.0f;

        /// <summary>Min and max rotation around x axis</summary>
        [Tooltip("Min and max rotation around x axis")]
        public Vector2 RotationXMinMax = new Vector2(-60.0f, 60.0f);

        /// <summary>Rotation dampening. Reduces rotation quickly once gesture ends. Set to 0 for complete dampening.</summary>
        [Tooltip("Rotation dampening. Reduces rotation quickly once gesture ends. Set to 0 for complete dampening.")]
        [Range(0.0f, 1.0f)]
        public float RotationDampening = 0.8f;

        /// <summary>How much to dampen movement, lower values dampen faster</summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("How much to dampen movement, lower values dampen faster")]
        public float Dampening = 0.95f;

        /// <summary>Whether to pan exclusively or allow pan with other gestures</summary>
        [Tooltip("Whether to pan exclusively")]
        public bool ExclusivePan;

        public float ScaleFactor = 100f;

        /// <summary>
        /// The pan gesture (left/right)
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }


        /// <summary>
        /// The scale gesture (zoom in/out)
        /// </summary>
        public ScaleGestureRecognizer ScaleGesture { get; private set; }

        /// <summary>
        /// Pan gesture that performs the rotation
        /// </summary>
        public PanGestureRecognizer RotateGesture { get; private set; }

        private float gestureDeltaXRotation;
        private float gestureDeltaYRotation;
        private Quaternion originalRotation;
        private Vector2 rotationVelocity;

        private Vector3 moveVelocity;
        //private float tiltVelocity;

        private Vector3 zoomVelocity;

        private void PanGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                Quaternion q = Target.rotation;
                q = Quaternion.Euler(0.0f, q.eulerAngles.y, 0.0f);
                moveVelocity += (q * Vector3.right * DeviceInfo.PixelsToUnits(gesture.DeltaX) * Time.deltaTime * PanSpeed * (100f / ScaleFactor));
                moveVelocity += (q * Vector3.up * DeviceInfo.PixelsToUnits(gesture.DeltaY) * Time.deltaTime * PanSpeed * (100f / ScaleFactor));
            }
        }



        private void ScaleGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                float zoomSpeed = ScaleGesture.ScaleMultiplierRange;
                zoomVelocity += (Target.forward * zoomSpeed * Time.deltaTime * ZoomSpeed * 1.0f / ScaleFactor);
            }
        }

        private void RotateGestureCallback(GestureRecognizer r)
        {
            if (Target == null)
            {
                return;
            }

            if (r.State == GestureRecognizerState.Executing)
            {
                rotationVelocity = Vector2.zero;
                ApplyRotation(DeviceInfo.PixelsToUnits(r.DeltaX) * RotationSpeed, DeviceInfo.PixelsToUnits(r.DeltaY) * RotationSpeed);
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                rotationVelocity = new Vector2(DeviceInfo.PixelsToUnits(r.VelocityX) * RotationSpeed * 0.01f, DeviceInfo.PixelsToUnits(r.VelocityY) * RotationSpeed * 0.01f);
            }
        }

        private void OnEnable()
        {
            if (Target == null)
            {
                Target = transform;
            }



            PanGesture = new PanGestureRecognizer();
            PanGesture.StateUpdated += PanGestureCallback;
            PanGesture.ThresholdUnits = 0.1f;
            PanGesture.MinimumNumberOfTouchesToTrack = PanGesture.MaximumNumberOfTouchesToTrack = 2;
            FingersScript.Instance.AddGesture(PanGesture);

            ScaleGesture = new ScaleGestureRecognizer();
            ScaleGesture.StateUpdated += ScaleGestureCallback;
            FingersScript.Instance.AddGesture(ScaleGesture);

            RotateGesture = new PanGestureRecognizer();
            RotateGesture.StateUpdated += RotateGestureCallback;
            RotateGesture.MinimumNumberOfTouchesToTrack = RotateGesture.MaximumNumberOfTouchesToTrack = 1;
            FingersScript.Instance.AddGesture(RotateGesture);
            Target = (Target == null ? transform : Target);
            originalRotation = (Target == null ? Quaternion.identity : Target.localRotation);

            if (!ExclusivePan)
            {
                PanGesture.AllowSimultaneousExecution(ScaleGesture);
            }

        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
                FingersScript.Instance.RemoveGesture(ScaleGesture);
                FingersScript.Instance.RemoveGesture(RotateGesture);

            }
        }

        private void Update()
        {
            Target = (Target == null ? transform : Target);




            Target.Translate(moveVelocity + zoomVelocity, Space.World);


            moveVelocity *= Dampening;

            zoomVelocity *= Dampening;


        }

        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }

            return Mathf.Clamp(angle, min, max);
        }

        private void ApplyRotation(float x, float y)
        {
            gestureDeltaXRotation = ClampAngle(gestureDeltaXRotation + x, -360.0f, 360.0f);
            gestureDeltaYRotation = ClampAngle(gestureDeltaYRotation + y, RotationXMinMax.x, RotationXMinMax.y);
            Quaternion xQuaternion = Quaternion.AngleAxis(gestureDeltaXRotation, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(gestureDeltaYRotation, Vector3.left);
            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }


    }
}