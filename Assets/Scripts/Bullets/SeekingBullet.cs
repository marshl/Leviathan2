using UnityEngine;
using System.Collections;

public abstract class SeekingBullet : BulletBase
{
	public SeekingBulletDesc seekingDesc;
	public TargettableObject target;

	public override void Start()
	{
		base.Start();
		this.seekingDesc = this.desc as SeekingBulletDesc;
	}

	protected override void Update()
	{
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
			Vector3 targetPos = this.target.transform.position;
			Vector3 targetVel = this.target.rigidbody.velocity;
			float targetSpeed = targetVel.magnitude;
			float flightDurection = ( targetPos - this.transform.position ).magnitude
				/ (this.desc.moveSpeed - targetSpeed );

			return targetPos + targetVel * flightDurection;
		}
		else
		{
			return this.target.transform.position;
		}
	}

	private float GetAngleToTarget()
	{
		return Vector3.Angle( this.transform.forward,
		                     (target.transform.position - this.transform.position).normalized );
	}
}
