using UnityEngine;
using System.Collections;

public class NewFighter : MonoBehaviour {

	const float EULER = 2.71828f;

	public float turnSpeed = 35.0f;
	public float rollSpeed = 2.0f;
	public float acceleration = 2.0f;
	//public float currentSpeed = 0.0f;
	//public float desiredSpeed = 0.0f;
	//public float maxSpeed = 10.0f; //Potentially used as a hard limit

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LateUpdate()
	{
		CheckFlightControls();
		ApplyDrag();
	}

	void CheckFlightControls()
	{
		if(Input.GetMouseButton(0))
		{
			Vector3 transformedMousePos = new Vector3(Input.mousePosition.x - Screen.width / 2,
			                                          Input.mousePosition.y - Screen.height / 2,
			                                          0);

			Vector3 torqueValue = Vector3.zero;
			//Set the desired rotation based on mouse position.
			
			transformedMousePos.y /= Screen.height / 2;
			transformedMousePos.x /= Screen.width / 2;

			transformedMousePos.y *= Gaussian(transformedMousePos.y);
			transformedMousePos.x *= Gaussian(transformedMousePos.x);

			torqueValue.x = transformedMousePos.y * Time.deltaTime * turnSpeed;
			torqueValue.y = -transformedMousePos.x * Time.deltaTime * turnSpeed;

			this.rigidbody.AddRelativeTorque(torqueValue);

			print("Torque vector: " + torqueValue);
			
		}

		if(Input.GetKey (KeyCode.W))
		{
			this.rigidbody.AddForce (this.transform.forward * acceleration * Time.deltaTime);
		}
		if(Input.GetKey (KeyCode.S))
		{
			this.rigidbody.AddForce (-this.transform.forward * acceleration * Time.deltaTime);
		}

		if(Input.GetKey (KeyCode.Q))
		{
			this.rigidbody.AddRelativeTorque (new Vector3(0,0,rollSpeed));
		}
		if(Input.GetKey (KeyCode.E))
		{
			this.rigidbody.AddRelativeTorque (new Vector3(0,0,-rollSpeed));
		}
	}

	void ApplyDrag()
	{

		//I pray this works. Obtain local angular velocity and reduce a single axis of it.

		Vector3 angularVeloc = transform.InverseTransformDirection(rigidbody.angularVelocity);

		angularVeloc.z *= 0.6f;

		this.rigidbody.AddRelativeTorque (angularVeloc - transform.InverseTransformDirection(rigidbody.angularVelocity));
	}

	float Gaussian(float x)
	{
		float a, b, c, d;
		
		a = -1.0f; // peak height
		b = 0.0f; // center peak position
		c = 1.0f; // width of the bell
		d = +2.0f; // offset
		
		float gaussian = a * EULER -  (Mathf.Pow ( x - b,2) / ( 2 * Mathf.Pow (c,2))) + d;
		return gaussian;
	}
}
