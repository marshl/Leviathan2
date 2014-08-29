using UnityEngine;
using System.Collections;

public class FighterCamera : MonoBehaviour
{
	public FighterMaster fighter;

	public float cameraDrift;
	public float pullBackFactor;

	private Transform eyePoint;
	private Vector3 angularVeloc;
	
	private void Start ()
	{
		eyePoint = fighter.transform.FindChild ("CameraPoint");
			
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

			angularVeloc = transform.InverseTransformDirection (this.fighter.rigidbody.angularVelocity);

			angularVeloc *= cameraDrift * 0.01f;
			Vector3 tempVector = new Vector3();

			tempVector.x = angularVeloc.y + angularVeloc.z;
			tempVector.y = angularVeloc.x * -1;
			tempVector.z = angularVeloc.z * 0 ;

			angularVeloc = tempVector;

			this.transform.position = eyePoint.transform.position + ( this.transform.rotation * tempVector );
		}
	}
}
