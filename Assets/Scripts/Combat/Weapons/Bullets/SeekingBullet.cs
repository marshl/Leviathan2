using UnityEngine;
using System.Collections;

/// <summary>
/// An subclass of the base bullet class that is used to control bullets that can move around
/// </summary>
public class SeekingBullet : SmartBullet
{
	public BaseHealth target;

	[Tooltip( "The descriptor of this bullet, converted into the right subclass DO NOT SET" ) ]
	public SeekingBulletDesc seekingDesc;

	protected override void Awake()
	{
		base.Awake();
		this.seekingDesc = this.desc as SeekingBulletDesc;
	}

	protected override void Update()
	{
		base.Update();

		if ( this.state != BULLET_STATE.FADING
		  && this.health.currentHealth > 0.0f
		  && this.distanceTravelled >= this.seekingDesc.seekingDelayDistance
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
		this.GetComponent<Rigidbody>().velocity = this.transform.forward * this.seekingDesc.moveSpeed;
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


	//Brought over from the base and tweaked a bit to do with explosions.
	public override void CheckCollision( Collider _collider )
	{
		//TODO: Quick and nasty fix, may have to be repaired to manage long-term missile collisions LM 08/05/14
		if ( this.source != null
		    && this.source.GetComponent<Collider>() == _collider )
		{
			return;
		}
		
		
		if ( this.desc.areaOfEffect ) //We do not deal damage ourself if it has an explosion
		{
			if(!this.seekingDesc.explodes)
			{
				TargetManager.instance.AreaOfEffectDamage( 
				                                          this.transform.position,
				                                          this.desc.aoeRadius,
				                                          this.desc.damage,
				                                          false, //TODO: Friendly fire bool
				                                          this.source.health.Owner );
			}
			if(this.seekingDesc.explodes)
			{
				CreateExplosion();
			}
			
			BulletManager.instance.DestroyLocalBullet( this );
		}
		else
		{
			BaseHealth health = _collider.gameObject.GetComponent<BaseHealth>();
			if ( health != null )
			{
				this.OnTargetCollision( health, desc.passesThroughTargets );
			}
			else
			{
				this.OnEmptyCollision();
			}
		}
	}

	public override void OnLifetimeExpiration()
	{
		if(this.seekingDesc.explodes)
		{
			CreateExplosion();
		}

		BulletManager.instance.DestroyLocalBullet( this );
	}

	public void CreateExplosion()
	{
		if(Network.peerType != NetworkPeerType.Disconnected)
		{
			GameObject explosion = Network.Instantiate (this.seekingDesc.explosionObject, this.transform.position, this.transform.rotation,1) as GameObject;
			explosion.GetComponent<SlowExplosion>().source = this.source;
		}
		else
		{
			GameObject explosion = GameObject.Instantiate (this.seekingDesc.explosionObject, this.transform.position, this.transform.rotation) as GameObject;
			explosion.GetComponent<SlowExplosion>().source = this.source;
		}

	}
}

