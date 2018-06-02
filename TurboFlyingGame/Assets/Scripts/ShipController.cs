using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour {

    public float maxSpeed = 10;
    public float acceleration = 1;
    public float angularAcceleration = 1;
    public float maxAngularSpeed = 10;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
    }

    void Update () {
		
        if(rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * Input.GetAxis("Vertical")*Time.deltaTime * acceleration);
        }

        if(!Cursor.visible && rb.angularVelocity.magnitude < maxAngularSpeed)
        {
            rb.AddTorque(transform.TransformVector( new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), -Input.GetAxis("Horizontal")) * angularAcceleration * Time.deltaTime));
        }

        if (Input.GetMouseButtonDown(2))
            Cursor.visible = !Cursor.visible;
	}
}
