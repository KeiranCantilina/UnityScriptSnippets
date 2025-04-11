using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;


public class TouchCameraController : MonoBehaviour
{
    public float lookSpeedH = 5f;
    public float lookSpeedV = 5f;
    public float zoomSpeed = 2f;
    public float dragSpeed = 5f;

    private float yaw;
    private float pitch;

    [SerializeField]
    private bool b_shouldMove = true;

    private void Awake()
    { EnhancedTouchSupport.Enable(); }

    private void Start()
    {
        // x - right    pitch
        // y - up       yaw
        // z - forward  roll
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        if (!enabled) return;

        if (Keyboard.current.spaceKey.wasReleasedThisFrame) { b_shouldMove = !b_shouldMove; }

        if (b_shouldMove)
        {
            if (Touch.activeTouches.Count > 0)
            {
                float touchToMouseScale = 0.25f;
                // look around with first touch
                //Touch t0 = Input.GetTouch(0);
                var t0 = Touch.activeTouches[0];
                yaw += lookSpeedH * touchToMouseScale * t0.delta.x;
                pitch -= lookSpeedV * touchToMouseScale * t0.delta.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0f);

                // and if have extra touch, also fly forward
                if (Touch.activeTouches.Count > 1)
                {
                    Touch t1 = Touch.activeTouches[1];
                    Vector3 offset = new Vector3(t1.delta.x, 0, t1.delta.y);
                    transform.Translate(offset * Time.deltaTime * touchToMouseScale, Space.Self);
                }
            }
            else
            {
                //Look around with Right Mouse
                if (Mouse.current.leftButton.isPressed)
                {
                    yaw += lookSpeedH * Time.deltaTime * Mouse.current.delta.x.ReadValue();
                    pitch -= lookSpeedV * Time.deltaTime * Mouse.current.delta.y.ReadValue();

                    transform.eulerAngles = new Vector3(pitch, yaw, 0f);

                    Vector3 offset = Vector3.zero;
                    float offsetDelta = Time.deltaTime * dragSpeed;
                    if (Keyboard.current.leftShiftKey.isPressed) offsetDelta *= 5.0f;
                    if (Keyboard.current.sKey.isPressed) offset.z -= offsetDelta;
                    if (Keyboard.current.wKey.isPressed) offset.z += offsetDelta;
                    if (Keyboard.current.aKey.isPressed) offset.x -= offsetDelta;
                    if (Keyboard.current.dKey.isPressed) offset.x += offsetDelta;
                    if (Keyboard.current.qKey.isPressed) offset.y -= offsetDelta;
                    if (Keyboard.current.eKey.isPressed) offset.y += offsetDelta;

                    transform.Translate(offset, Space.Self);
                }

                //drag camera around with Middle Mouse
                if (Mouse.current.middleButton.isPressed)
                {
                    transform.Translate(-Mouse.current.delta.x.ReadValue() * Time.deltaTime * dragSpeed,
                        -Mouse.current.delta.y.ReadValue() * Time.deltaTime * dragSpeed, 0);
                }

                //Zoom in and out with Mouse Wheel
                transform.Translate(0, 0, Mouse.current.delta.y.ReadValue() * Time.deltaTime * zoomSpeed, Space.Self);
            }
        }
    }

}