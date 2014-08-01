using UnityEngine;
using System.Collections;

public class FighterMovement : MonoBehaviour
{
	public FighterMaster masterScript;

	public float turnSpeed;
	public float rollSpeed;
	public float acceleration;
	public float maxSpeed;
	public float desiredSpeed;
	public float minSpeed;
	public float rollDamping;
	
	public float undockingSpeed;

	public float contactPushForce;
	public float contactTorque;
	
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
		if ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected )
		{
			switch ( this.masterScript.state )
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
				if ( this.masterScript.capitalShip != null )
				{
					float speed = this.masterScript.capitalShip.movement.currentMovementSpeed;
					Vector3 direction = this.masterScript.capitalShip.transform.forward;
					this.rigidbody.velocity = direction * speed + this.transform.forward * this.undockingSpeed;
				}
				break;
			}
			default:
			{
				break;
			}
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

	private void OnCollisionEnter( Collision _collision )
	{
		for ( int i = 0; i < _collision.contacts.Length; ++i )
		{
			Vector3 normal = _collision.contacts[i].normal;
			float collisionAngle = Vector3.Dot( normal, -this.transform.forward );
			
			this.rigidbody.velocity += normal * this.contactPushForce;
			Vector3 torqueAmount = Vector3.Cross( transform.forward, normal ) * this.contactTorque * collisionAngle;
			this.rigidbody.AddTorque( torqueAmount );
		}
		
		this.desiredSpeed = this.rigidbody.velocity.magnitude;
		this.desiredSpeed *= Vector3.Dot( this.transform.forward, this.rigidbody.velocity.normalized ) > 0.0f ? 1.0f : -1.0f;
	}

	public void OnRespawn()
	{
		this.rigidbody.velocity = Vector3.zero;
		this.rigidbody.angularVelocity = Vector3.zero;
		this.desiredSpeed = 0.0f;
	}
}
