using UnityEngine;
using System.Collections;

[System.Obsolete]
public class DeprecatedFighter : MonoBehaviour {
	
	enum FighterState
	{
		Docked,
		Undocked,
		Dead
	};

	const float EULER = 2.71828f;

	FighterState state;
	public float turnSpeed = 0.5f;
	public float turnRate = 4;
	public float rollSpeed = 0.5f;
	public float currentSpeed = 0.0f;
	public float acceleration = 2.0f;
	public float maxSpeed = 10.0f;

	public bool useGaussianSmoothing = false;

	Quaternion desiredRotation;
	
	//public float inertia = 1;
	//public float inertialSpeed = 0;
	
	//public Vector3 inertialVector;
	//public Vector3 targetVector;
	

	// Use this for initialization
	void Start () {
		//inertialVector = this.transform.forward;
		desiredRotation = this.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () 
	{
		CheckFlightControls();
	}
	
	void FixedUpdate()
	{
		if(this.rigidbody.velocity != this.transform.forward * currentSpeed)
		this.rigidbody.velocity = (this.transform.forward * currentSpeed);// + (inertialVector * inertialSpeed);
		this.rigidbody.maxAngularVelocity = 0;
	}

	void LateUpdate()
	{
		this.rigidbody.rotation = Quaternion.Slerp (this.rigidbody.rotation,desiredRotation,Time.deltaTime * turnRate);
	}
	
	void CheckFlightControls()
	{
		/*if(Input.GetKey(KeyCode.LeftArrow))
		{
			desiredRotation = Quaternion.Euler (this.transform.rotation.eulerAngles + new Vector3(0,-turnSpeed,0));
			//this.transform.Rotate(new Vector3(0,-turnSpeed * Time.deltaTime,0));
		}
		if(Input.GetKey(KeyCode.RightArrow))
		{
			desiredRotation = Quaternion.Euler (this.transform.rotation.eulerAngles + new Vector3(0,turnSpeed,0));
		}
		
		if(Input.GetKey(KeyCode.UpArrow))
		{
			desiredRotation = Quaternion.Euler (this.transform.rotation.eulerAngles + new Vector3(-turnSpeed,0,0));
		}
		if(Input.GetKey(KeyCode.DownArrow))
		{
			desiredRotation = Quaternion.Euler (this.transform.rotation.eulerAngles + new Vector3(turnSpeed,0,0));
		}*/

		if(Input.GetMouseButton(0))
		{
			Vector3 transformedMousePos = new Vector3(Input.mousePosition.x - Screen.width / 2,
			                                          Input.mousePosition.y - Screen.height / 2,
			                                          0);
			//Set the desired rotation based on mouse position.

			transformedMousePos.y /= Screen.height / 2;
			transformedMousePos.x /= Screen.width / 2;

			if(useGaussianSmoothing)
			{
				transformedMousePos.y *= Gaussian(transformedMousePos.y);
				transformedMousePos.x *= Gaussian(transformedMousePos.x);
			}

			DebugConsole.Log( "Pos: " + transformedMousePos );

			if(useGaussianSmoothing)
			{
				desiredRotation = desiredRotation * Quaternion.Euler (new Vector3(
				turnSpeed * transformedMousePos.y,
				turnSpeed * transformedMousePos.x * -1 ,
				0));
			}
			else 
			{
				desiredRotation = desiredRotation * Quaternion.Euler (new Vector3(
					turnSpeed * transformedMousePos.y * -1,
					turnSpeed * transformedMousePos.x ,
					0));
			}
		}
		
		if(Input.GetKey(KeyCode.W))
		{
			currentSpeed += (acceleration * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.S))
		{
			currentSpeed -= (acceleration * Time.deltaTime);
		}

		if(Input.GetKey (KeyCode.Q))
		{
			desiredRotation = desiredRotation * Quaternion.Euler (new Vector3(0,0,rollSpeed));
		}
		if(Input.GetKey (KeyCode.E))
		{
			desiredRotation = desiredRotation * Quaternion.Euler (new Vector3(0,0,-rollSpeed));
		}

	}
	//Function to compute a gaussian curve to smooth turning a bit
	//Uses magic numbers because it's highly unlikely to be used with any other code in this game.
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
