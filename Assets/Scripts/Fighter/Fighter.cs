using UnityEngine;
using System.Collections;

public class Fighter : MonoBehaviour {
	
	enum FighterState
	{
		Docked,
		Undocked,
		Dead
	};
	
	FighterState state;
	public float turnSpeed = 0.5f;
	public float turnRate = 4;
	public float rollSpeed = 0.5f;
	public float currentSpeed = 0.0f;
	public float acceleration = 2.0f;
	public float maxSpeed = 10.0f;

	Quaternion desiredRotation;
	
	//public float inertia = 1;
	//public float inertialSpeed = 0;
	
	//public Vector3 inertialVector;
	//public Vector3 targetVector;
	

	// Use this for initialization
	void Start () {
		//inertialVector = this.transform.forward;
	}
	
	// Update is called once per frame
	void Update () {
		
		CheckFlightControls();
		print(Input.mousePosition);
	
	}
	
	void FixedUpdate()
	{
		this.rigidbody.velocity = (this.transform.forward * currentSpeed);// + (inertialVector * inertialSpeed);

	}

	void LateUpdate()
	{
		this.transform.rotation = Quaternion.Slerp (this.transform.rotation,desiredRotation,Time.deltaTime * turnRate);
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
			desiredRotation = this.transform.rotation * Quaternion.Euler (new Vector3(
				turnSpeed * (transformedMousePos.y / Screen.height/2 ) * -1,
				turnSpeed * (transformedMousePos.x / Screen.width/2 ),
				0));


		}
		
		if(Input.GetKey(KeyCode.W))
		{
			currentSpeed += (acceleration * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.S))
		{
			currentSpeed -= (acceleration * Time.deltaTime);
		}
	}
}
