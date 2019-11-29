using UnityEngine;
using System.Collections;

public class FighterCamera : MonoBehaviour
{
	public FighterMaster fighter;

	public float cameraDrift;
	public float pullBackFactor; //Fov pulled back by at maximum thrust
	//public float velocityDivisor = 250;

	private Transform eyePoint;
	private Vector3 angularVeloc;
	
	private void Start ()
	{
		eyePoint = fighter.transform.Find ("CameraPoint");
			
		this.transform.rotation = eyePoint.transform.rotation;
		this.transform.position = eyePoint.transform.position;
	}

	private void LateUpdate()
	{
		if ( this.fighter.state == FighterMaster.FIGHTERSTATE.DEAD
		  || this.fighter.state == FighterMaster.FIGHTERSTATE.OUT_OF_CONTROL )
		{
			this.transform.LookAt( this.fighter.transform.position );
		}
		else
		{
			this.transform.position = eyePoint.transform.position;
			this.transform.rotation = fighter.transform.rotation;

			angularVeloc = transform.InverseTransformDirection (this.fighter.GetComponent<Rigidbody>().angularVelocity);

			angularVeloc *= cameraDrift * 0.01f;
			Vector3 tempVector = new Vector3();

			tempVector.x = angularVeloc.y + angularVeloc.z;
			tempVector.y = angularVeloc.x * 1;
			//tempVector.z = angularVeloc.z * 0 ;
			tempVector.z =  0.0f ;

			angularVeloc = tempVector;

			this.transform.position = eyePoint.transform.position + ( this.transform.rotation * tempVector );
			if(this.fighter.state != FighterMaster.FIGHTERSTATE.UNDOCKING && !this.fighter.movement.boostIgnoresMax)
			{
				this.GetComponent<Camera>().fieldOfView = 60 + ((this.fighter.movement.desiredSpeed / this.fighter.movement.maxSpeed) * pullBackFactor);
			}
		}
	}
}
