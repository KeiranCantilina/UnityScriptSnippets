using UnityEngine;
using System.Collections;


//[RequireComponent(typeof(MeshCollider))]

public class SimpleIKDragger : MonoBehaviour
{

    private Vector3 screenPoint;
    private Vector3 offset;
    private Vector3 curPosition;
    public GenericRobotController robotController;
    private Transform target;


    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        target.position = curPosition;
        target.eulerAngles = new Vector3(0,0,0);
        robotController.MoveRobot(target);
    }


    public Vector3 GetDragPosition()
    {
        return curPosition;
    }

}
