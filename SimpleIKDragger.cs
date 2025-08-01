using UnityEngine;
using System.Collections;



namespace KeiranUtils
{
    [AddComponentMenu("Keiran Utils/SimpleIKDragger")]
    public class SimpleIKDragger : MonoBehaviour
    {

        private Vector3 screenPoint;
        private Vector3 offset;
        private Vector3 curPosition;
        public GenericRobotController robotController;
        private Vector3 targetPos;
        private Vector3 targetRot;
        public Vector3 Orientation;


        void OnMouseDown()
        {
            screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

            //UnityEngine.Debug.Log("CLick!");
        }

        void OnMouseDrag()
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

            curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            targetPos = curPosition;
            targetRot = Orientation;
            robotController.MoveRobot(targetPos, targetRot, this.gameObject);
        }


        public Vector3 GetDragPosition()
        {
            return curPosition;
        }

    }
}