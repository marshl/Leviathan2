using UnityEngine;
using System.Collections;

public class FighterMovement : MonoBehaviour
{
	public FighterMaster masterScript;

	public float turnSpeed = 35.0f;
	public float rollSpeed = 2.0f;
	public float acceleration = 2.0f;
	public float maxSpeed = 10.0f;
	public float desiredSpeed = 0.0f;
	public float minSpeed = -2.0f;
	public float rollDamping = 0.3f;
	public float minimumBounce = 10.0f;
	public float bounceVelocityModifier = 300.0f;
	
	/// <summary>
	/// The distance from the centre of the screen at which the turn amout will be maximum
	/// Example: A value of 1/3 means that the the mouse outside an ellipse with extentes 1/3
	/// of the screen size will hav maximum turn rate,
	/// gradually leading down to zero in the centre of the screen
	/// </summary>
	public float turnExtents;
	
	private float screenRatio;
	
	private void Start()
	{
		screenRatio = (float)Screen.width / (float)Screen.height;
	}
	
	private void LateUpdate()
	{
		switch( this.masterScript.state )
		{
		case FighterMaster.FIGHTERSTATE.FLYING:
		{
			this.rigidbody.AddForce( this.transform.forward * this.desiredSpeed * Time.deltaTime );
			if ( !GameMessages.instance.typing )
			{
				this.CheckFlightControls();
			}
			this.ApplyDrag();
			break;
		}
		
		case FighterMaster.FIGHTERSTATE.UNDOCKING:
		{
			//this.rigidbody.AddForce( this.transform.forward * this.desiredSpeed * Time.deltaTime );
			if ( !GameMessages.instance.typing )
			{
				this.CheckFlightControls();
			}
			//this.ApplyDrag();
			break;
		}
		
		default:
		{
			break;
		}
		}
	}
	
	private void CheckFlightControls()
	{
		if ( Input.GetMouseButton(0) ) // Left click - Turn the ship
		{
			Vector2 transformedMousePos = new Vector3(Input.mousePosition.x / Screen.width * 2.0f - 1.0f,
			                                          Input.mousePosition.y / Screen.height * 2.0f - 1.0f);
			
			transformedMousePos.x = Mathf.Clamp( transformedMousePos.x, -1.0f, 1.0f );
			transformedMousePos.y = Mathf.Clamp( transformedMousePos.y, -1.0f, 1.0f );
			
			//Set the desired rotation based on mouse position.
			transformedMousePos.x = Mathf.Sign( transformedMousePos.x ) * Common.GaussianCurveClamped(
				transformedMousePos.x, -1.0f, 0.0f, this.turnExtents, 1.0f );
			transformedMousePos.y = Mathf.Sign( transformedMousePos.y ) * Common.GaussianCurveClamped(
				transformedMousePos.y, -1.0f, 0.0f, this.turnExtents * this.screenRatio, 1.0f );
			
			Vector3 torqueValue = new Vector3(
				-transformedMousePos.y * Time.deltaTime * turnSpeed,
				transformedMousePos.x * Time.deltaTime * turnSpeed,
				0.0f );
			
			this.rigidbody.AddRelativeTorque( torqueValue );
		}

		if ( Input.GetKey( KeyCode.W ) ) // Accelerate
		{
			this.desiredSpeed += this.acceleration * Time.deltaTime;
			if( this.desiredSpeed > this.maxSpeed)
			{
				this.desiredSpeed = this.maxSpeed;
			}
		}
		if ( Input.GetKey( KeyCode.S ) ) // Deccelerate
		{
			this.desiredSpeed -= this.acceleration * Time.deltaTime;
			if ( this.desiredSpeed < this.minSpeed )
			{
				this.desiredSpeed = this.minSpeed;
			}
		}
		
		if ( Input.GetKey( KeyCode.Q) ) // Rotate left
		{
			this.rigidbody.AddRelativeTorque (new Vector3(0,0,rollSpeed));
		}
		if ( Input.GetKey(KeyCode.E) ) // Rotate right
		{
			this.rigidbody.AddRelativeTorque (new Vector3(0,0,-rollSpeed));
		}
		
		if ( Input.GetKey( KeyCode.Backspace ) ) // Stop
		{
			this.desiredSpeed = 0.0f;
		}
		if ( Input.GetKey( KeyCode.Tab ) ) // Thrusters (not a special)
		{
			this.desiredSpeed += ( this.acceleration * 2 * Time.deltaTime );
		}
	}
	
	private void ApplyDrag()
	{
		//I pray this works. Obtain local angular velocity and reduce a single axis of it.
		Vector3 angularVeloc = transform.InverseTransformDirection(rigidbody.angularVelocity);
		angularVeloc.z *= rollDamping;
		this.rigidbody.AddRelativeTorque( angularVeloc - transform.InverseTransformDirection( rigidbody.angularVelocity ) );
	}
	
	private void OnCollisionEnter(Collision _collision)
	{
		Vector3 bounceForce = (_collision.contacts[0].normal *
		                       this.rigidbody.velocity.magnitude * bounceVelocityModifier)
			+ (minimumBounce * _collision.contacts[0].normal.normalized);
		
		if ( _collision.rigidbody != null )
		{
			bounceForce *= (_collision.rigidbody.velocity.magnitude * bounceVelocityModifier);
		}

		this.rigidbody.AddForce( bounceForce );
		this.rigidbody.AddTorque( Vector3.Cross(new Vector3(bounceForce.z, bounceForce.y, bounceForce.x), this.transform.rotation.eulerAngles));
	}
}
