using UnityEngine;
using System.Collections;

public class FighterCamera : MonoBehaviour {

	public float cameraDrift = 1.0f;
	public float pullBackFactor = 5.0f;
	public GameObject fighter;
	private Transform eyePoint;
	private Vector3 velocity;
	private Vector3 angularVeloc;

	// Use this for initialization
	void Start () {


			
		eyePoint = fighter.transform.FindChild ("CameraPoint");
			

		this.transform.rotation = eyePoint.transform.rotation;
		this.transform.position = eyePoint.transform.position;

	}
	
	// Update is called once per frame
	void Update () {



	}

	void LateUpdate()
	{
		//Vector3 positionVector = 
		this.transform.position = eyePoint.transform.position;
		this.transform.rotation = fighter.transform.rotation;


		angularVeloc = transform.InverseTransformDirection (this.fighter.rigidbody.angularVelocity);

		angularVeloc *= cameraDrift * 0.01f;
		Vector3 tempVector = new Vector3();

		tempVector.x = angularVeloc.y + angularVeloc.z;
		tempVector.y = angularVeloc.x * -1;
		tempVector.z = angularVeloc.z * 0 ;

		angularVeloc = tempVector;

		this.transform.position = eyePoint.transform.position + (this.transform.rotation *tempVector );


		//Quaternion.Lerp (this.transform.rotation,fighter.transform.rotation,Time.deltaTime * cameraDrift);
		//Vector3.Lerp (this.transform.position, eyePoint.transform.position,pullBackFactor);
			//eyePoint.transform.position + (new Vector3(0,0,pullBackFactor) * Mathf.Abs (fighter.rigidbody.velocity.magnitude) * -1 );
			//eyePoint.transform.position;
	}
}
