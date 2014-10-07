using UnityEngine;
using System.Collections;

/// <summary>
/// An subclass of the base bullet class that is used to control bullets that can move around
/// </summary>
public class SeekingBullet : BulletBase
{
	public BaseHealth health;

	public BaseHealth target;

#if UNITY_EDITOR
	public int debugID;
#endif
	
	/// <summary>
	/// The descriptor of this bullet, converted into the right subclass (do not set)
	/// </summary>
	[HideInInspector]
	public SeekingBulletDesc seekingDesc;
	
	protected virtual void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		BulletManager.instance.seekingBulletMap.Add( this.networkView.viewID, this );
		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this.seekingDesc = this.desc as SeekingBulletDesc;

#if UNITY_EDITOR
		this.debugID = BulletManager.instance.debugSeekingID;
		BulletManager.instance.debugSeekingBulletMap.Add( this.debugID, this );
		++BulletManager.instance.debugSeekingID;
#endif
	}

	protected override void Update()
	{
		if ( this.distanceTravelled >= this.seekingDesc.seekingDelayDistance
		  && this.target != null )
		{
			this.TurnToTarget();
			Vector3 vectorToTarget = (target.transform.position - this.transform.position).normalized;
			float angleToTarget = Vector3.Angle( this.transform.forward, vectorToTarget );
			if ( this.seekingDesc.canAngleOut
			  && angleToTarget >= this.seekingDesc.maxDetectionAngle )
			{
				this.target = null;
			}
		}

		this.rigidbody.velocity = this.transform.forward * this.seekingDesc.moveSpeed;
		base.Update();
	}

	/// <summary>
	/// Turns this bullet to face its target
	/// </summary>
	/// <returns><c>true</c>, if the target is directly direct <c>false</c> otherwise.</returns>
	protected virtual bool TurnToTarget()
	{
		Vector3 targetPos = Common.GetTargetLeadPosition( this.transform.position, this.target.transform, this.seekingDesc.moveSpeed );
		Vector3 vectorToTarget = targetPos - this.transform.position;
		  
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
}

