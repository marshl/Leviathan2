using UnityEngine;
using System.Collections;

/// <summary>
/// An subclass of the base bullet class that is used to control bullets that can move around
/// </summary>
public class SeekingBullet : BulletBase
{
	public TargettableObject target;

	/// <summary>
	/// The descriptor of this bullet, converted into the right subclass (do not set)
	/// </summary>
	[HideInInspector]
	public SeekingBulletDesc seekingDesc;

	protected override void Awake()
	{
		base.Awake();
		this.seekingDesc = this.desc as SeekingBulletDesc;
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		//TODO: Uncomment
		if ( this.distanceTravelled >= this.seekingDesc.seekingDelayDistance
		  && this.target != null )
		{
			this.TurnToTarget();
			float angleToTarget = this.GetAngleToTarget();
			if ( angleToTarget >= this.seekingDesc.maxDetectionAngle )
			{
				//this.target = null;
			}
		}

		//this.transform.Rotate( this.transform.up, Time.deltaTime * this.seekingDesc.turnRate);
		this.rigidbody.velocity = this.transform.forward * this.seekingDesc.moveSpeed;
		base.Update();
	}

	/// <summary>
	/// Turns this bullet to face its target
	/// </summary>
	/// <returns><c>true</c>, if the target is directly direct <c>false</c> otherwise.</returns>
	protected virtual bool TurnToTarget()
	{
		Vector3 vectorToTarget = target.transform.position - this.transform.position;

		// Return true if there is no distance to the target
		if ( vectorToTarget.sqrMagnitude == 0.0f )
		{
			return true;
		}
		
		Vector3 rayToTarget = vectorToTarget.normalized;
		// Return true if this is facing the target directly
		if ( rayToTarget == this.transform.forward )
		{
			return true;
		}

		// If the target is directly behind this bullet, turn on an arbitrary axis
		if ( rayToTarget == -this.transform.forward )
		{
			this.transform.Rotate( this.transform.up, this.seekingDesc.turnRate * Time.deltaTime );
			return false;
		}

		float angle = Vector3.Angle( this.transform.forward, rayToTarget );
		Vector3 perp = Vector3.Cross( this.transform.forward, rayToTarget );
		float amountToTurn = this.seekingDesc.turnRate * Time.deltaTime;

		bool facingTarget = false;
		// If there is less than a frame to turn in, rotate on the angle directly to prevent stuttering
		if ( amountToTurn > angle )
		{
			facingTarget = true;
			amountToTurn = angle;
		}
		this.transform.Rotate( perp.normalized, amountToTurn, Space.World );
		return facingTarget;
	}

	private Vector3 GetTargetLeadPosition()
	{
		if ( this.target.rigidbody != null )
		{
			/*Vector3 targetPos = this.target.transform.position;
			Vector3 targetVel = this.target.rigidbody.velocity;
			float targetSpeed = targetVel.magnitude;
			float flightDuration = ( targetPos - this.transform.position ).magnitude
				/ (this.desc.moveSpeed - targetSpeed );

			return targetPos + targetVel * flightDuration;*/

			Vector3 targetPos = this.target.transform.position;
			Vector3 targetForward = this.target.transform.forward;

			Vector3 vectorFromTarget = this.transform.position - targetPos;
			Vector3 rayFromTarget = vectorFromTarget.normalized;
			float angleFromTarget = Vector3.Angle( rayFromTarget, targetForward );
			float distToTarget = vectorFromTarget.magnitude;
			float flightDuration = distToTarget * Mathf.Cos( angleFromTarget ) / 2.0f;
			Vector3 targetVel = this.target.rigidbody.velocity;

			return targetPos + targetVel * flightDuration;
		}
		else
		{
			return this.target.transform.position;
		}
	}

	/*protected virtual void OnSerializeNetworkView( BitStream _stream, NetworkMessageInfo _info )
	{
		if ( _stream.isWriting )
		{
			Vector3 pos = this.transform.localPosition;
			Quaternion rot = this.transform.localRotation;
			_stream.Serialize( ref pos );
			_stream.Serialize( ref rot );
		}
		else
		{
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			_stream.Serialize( ref pos );
			_stream.Serialize( ref rot );

			this.transform.localPosition = pos;
			this.transform.localRotation = rot;
		}
	}*/

	private float GetAngleToTarget()
	{
		return Vector3.Angle( this.transform.forward,
		                     (target.transform.position - this.transform.position).normalized );
	}
}

