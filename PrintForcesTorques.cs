using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintForcesTorques : MonoBehaviour
{
    private float time;
    private Vector3 impulse;
    private Vector3 force;
    private Vector3 torque;
    private ContactPoint[] collisionContacts;
    int numberOfContacts;

    // Start is called before the first frame update
    void Start()
    {
        time = Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        force = impulse / time;
        torque = new Vector3(0, 0, 0);

        if (force[0]==0 && force[1]==0 && force[2]==0)
        {

        }
        else
        {
            // iterate through collision contacts and add forces and calculate torques
            foreach (ContactPoint contact in collisionContacts)
            {
                Vector3 contactImpulse = contact.impulse;
                Vector3 contactLocation = contact.point;
                //Vector3 contactNormal = contact.normal;

                // MATH
                Vector3 contactForce = contactImpulse / time;
                Vector3 contactDirection = new Vector3();
                if (this.transform.GetComponent<Rigidbody>() == null)
                {
                    contactDirection = contactLocation - this.transform.position; // Vector of articulationbody coordinate system origin to the contact point
                }
                else
                {
                    contactDirection = contactLocation - this.transform.position; // Vector of rigidbody coordinate system to the contact point
                }
                
                Vector3 contactTorque = Vector3.Cross(contactDirection, contactForce);

                // now add up torques
                torque = torque + contactTorque;
            }

            
        }
        

        UnityEngine.Debug.Log("Force: " + force + "\nTorque: " + torque);

        time = Time.fixedDeltaTime;
    }

    void OnCollisionStay(Collision collisionInfo)
    {
        
        impulse = collisionInfo.impulse;
        numberOfContacts = collisionInfo.contactCount;
        //UnityEngine.Debug.Log("Collision!\n Number of contacts: "+ numberOfContacts + "\n" + "Impulse: " + impulse);
        collisionContacts = new ContactPoint[numberOfContacts];
        collisionInfo.GetContacts(collisionContacts);


    }

    void OnCollisionExit() 
    {
        impulse = new Vector3(0, 0, 0);
    }
}
