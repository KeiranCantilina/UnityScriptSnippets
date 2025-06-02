using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRubyShared;
using System;


public class KeiranOrbitGesture : MonoBehaviour
{
    /// <summary>The object to orbit</summary>
    [Tooltip("The object to orbit")]
    public Transform target;


    /// <summary>The rotation speed in degrees per second</summary>
    [Tooltip("The rotation speed")]
    private float xSpeed = 25.0f;
    private float ySpeed = 12.0f;

    /// <summary>The scale speed</summary>
    [Tooltip("The scale speed")]
    private float scaleSpeed = 1.0f;

    /// <summary>
    /// Rotation gesture
    /// </summary>
    public PanGestureRecognizer RotateGesture { get; private set; }

    /// <summary>
    /// Zoom gesture
    /// </summary>
    public ScaleGestureRecognizer ScaleGesture { get; private set; }

    
    private float distance = 2.0f;
    float prevDistance;
    private Quaternion originalRotation;


    public float yMinLimit = -20;
    public float yMaxLimit = 80;
    private float distanceNearLimit = 0.1f;

    float x = 0.0f;
    float y = 0.0f;


    private void OnEnable()
    {
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (target == null)
        {
            Vector3 targetPosition = new Vector3(0,0,0);
            Vector3 targetOrientation = new Vector3(0,0,0);
            target.transform.position = targetPosition;
            target.transform.eulerAngles = targetOrientation;
        }

        prevDistance = distance;

        ScaleGesture = new ScaleGestureRecognizer();
        ScaleGesture.StateUpdated += ScaleGestureCallback;
        FingersScript.Instance.AddGesture(ScaleGesture);

        RotateGesture = new PanGestureRecognizer();
        RotateGesture.StateUpdated += RotateGestureCallback;
        RotateGesture.MinimumNumberOfTouchesToTrack = RotateGesture.MaximumNumberOfTouchesToTrack = 1;
        FingersScript.Instance.AddGesture(RotateGesture);
    }

    private void Update()
    {
        
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    private void OnDisable()
    {
        if (FingersScript.HasInstance)
        {
            FingersScript.Instance.RemoveGesture(ScaleGesture);
            FingersScript.Instance.RemoveGesture(RotateGesture);
        }
    }

    private void ScaleGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            if (distance < distanceNearLimit)
            {
                distance = distanceNearLimit;
            }

            distance -= ScaleGesture.ScaleMultiplierRange * 0.1f * scaleSpeed; // Might need to adjust this

            if (Math.Abs(prevDistance - distance) > 0.001f)
            {
                prevDistance = distance;
                var rot = Quaternion.Euler(y, x, 0);
                var po = rot * new Vector3(0.0f, 0.0f, -distance) + target.transform.position;
                UnityEngine.Debug.Log(rot.eulerAngles);
                transform.rotation = rot;
                transform.position = po;
            }
        }
    }

    private void RotateGestureCallback(GestureRecognizer r)
    {
        if (r.State == GestureRecognizerState.Executing)
        {
            Vector2 pos = new Vector2(r.DeltaX, r.DeltaY);

            var dpiScale = 1f;
            if (Screen.dpi < 1) dpiScale = 1;
            if (Screen.dpi < 200) dpiScale = 1;
            else dpiScale = Screen.dpi / 200f;

            if (pos.x < 380 * dpiScale && Screen.height - pos.y < 250 * dpiScale) return;

            x += pos.x * xSpeed * 0.02f;
            y -= pos.y * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);
            var rotation = Quaternion.Euler(y, x, 0);
            var position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.transform.position;
            transform.rotation = rotation;
            transform.position = position;
        }
    }





}
