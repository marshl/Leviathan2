using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretBehavior : BaseWeaponManager
{
	public Transform swivel;
	public Transform pivot;
	public float rotationSpeed;

	public float minPitchAngle;
	public float maxPitchAngle;
	public float pitchSpeed;

	public WeaponBase weapon;

	public float fireLockAngle;
	
	public NetworkOwnerControl ownerControl;
	private bool ownerInitialised = false;

	public Transform[] gunArms;
	public float gunArmTiltLimit;

	public Transform orientationPoint;

	protected override void Awake()
	{
		base.Awake();

		this.restrictions.types = (int)( TARGET_TYPE.FIGHTER );
		this.restrictions.ignoreBelowHorizon = true;
		this.restrictions.transform = this.pivot;
	}

	private void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		NetworkOwnerManager.instance.RegisterUnknownObject( this );
	}

	protected virtual void Update()
	{
		if ( this.ownerInitialised == false 
		  && this.ownerControl.ownerID != null )
		{
			this.restrictions.maxDistance = this.weapon.weaponDesc.bulletDesc.maxDistance;

			int playerID = this.ownerControl.ownerID.Value;
			GamePlayer player = GamePlayerManager.instance.GetPlayerWithID( playerID );
			if ( player != null && player.capitalShip != null )
			{
				this.OwnerInitialise();
			}
		}

		if ( (this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected)
		    && this.health.currentHealth > 0.0f )
		{
			float range = this.weapon.weaponDesc.bulletDesc.maxDistance;
			if ( this.currentTarget == null
			  || this.currentTarget.currentHealth <= 0.0f
			  || this.currentTarget.enabled == false
			  || (this.transform.position - this.currentTarget.transform.position).magnitude > range
			  || Vector3.Dot( this.currentTarget.transform.position - this.transform.position, this.transform.up) < 0.0f ) // Below the target horizon
			{
				this.currentTarget = TargetManager.instance.GetCentreTarget( this );
			}

			if ( this.currentTarget != null )
			{
				Vector3 leadPos = this.TurretAim( this.currentTarget.transform );
				Vector3 vectorToLeadPos = (leadPos - this.transform.position).normalized;
				Vector3 vectorToFront = this.pivot.transform.forward;
				float angleToLeadPos = Vector3.Dot( vectorToLeadPos, vectorToFront );

				if ( angleToLeadPos >= 1.0f - this.fireLockAngle )
				{
					this.weapon.SendFireMessage();
				}
			}
		}

		if ( this.health.currentHealth <= 0.0f )
		{
			if ( this.health.lastHitBy != null )
			{
				ScoreManager.instance.AddScore( SCORE_TYPE.TURRET_KILL, this.health.lastHitBy, true );
			}

			TargetManager.instance.RemoveTarget( this.health );

			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				GameObject.Destroy( this.gameObject );
			}
			else
			{
				Network.Destroy( this.gameObject );
			}
		}
	}

	protected Vector3 TurretAim( Transform _target )
	{
		float speed = BulletDescriptorManager.instance.GetDescOfType( this.weapon.weaponType ).moveSpeed;
		Vector3 leadPosition = Common.GetTargetLeadPosition( this.pivot.position, _target, speed );

		if ( leadPosition == this.pivot.position )
		{
			return Vector3.zero;
		}
	
		Vector3 direction = ( leadPosition - this.orientationPoint.position ).normalized;
	
		// Yaw
		float yawAngle = Common.AngleAroundAxis( this.transform.forward, direction, this.transform.up );
		float maxYaw = this.rotationSpeed * Time.deltaTime;
		float currentYaw = Common.AngleAroundAxis( this.transform.forward, this.swivel.transform.forward, this.transform.up );

		if ( yawAngle < 0 && currentYaw > 0 )
		{
			yawAngle += 360.0f;
		}
		else if ( yawAngle > 0 && currentYaw < 0 )
		{
			yawAngle -= 360.0f;
		}

		float yawDiff = yawAngle - currentYaw;
		float yawChange = Mathf.Sign( yawDiff ) * Mathf.Min( maxYaw, Mathf.Abs( yawDiff ) );
		//Debug.Log( "YawAngle:" + yawAngle + " MaxYaw:" + maxYaw + " CurrentYaw:" + currentYaw + " YawDiff:" + yawDiff + " NewYaw:" + yawChange );

		if ( Mathf.Abs( yawDiff ) >= 180.0f )
		{
			yawChange *= -1.0f;
		}
		Quaternion targetYaw = Quaternion.AngleAxis( yawChange + currentYaw, Vector3.up );
		this.swivel.localRotation = targetYaw;

		// Pitch
		float pitchAngle = Common.AngleAroundAxis( this.transform.up, direction, this.swivel.right);
		// Take off 90 degrees because the angle is measured from the up vector, but the pivot has angle 0 when pointed out the front
		pitchAngle = Mathf.Clamp( pitchAngle, this.minPitchAngle, this.maxPitchAngle ) - 90.0f;
		Quaternion targetPitch = Quaternion.AngleAxis( pitchAngle, Vector3.right );
		this.pivot.localRotation = Quaternion.Slerp( this.pivot.localRotation, targetPitch, Time.deltaTime * this.pitchSpeed );

		// Gun tilt
		foreach ( Transform gunArm in this.gunArms )
		{
			float tiltAngle = Common.AngleAroundAxis( this.pivot.forward, (leadPosition - gunArm.position).normalized, this.pivot.up );

			tiltAngle = Mathf.Clamp ( tiltAngle, -this.gunArmTiltLimit, this.gunArmTiltLimit );
			Quaternion targetTilt = Quaternion.AngleAxis( tiltAngle, gunArm.up );
			gunArm.localRotation = targetTilt;
		}

		return leadPosition;
	}

	// Unity Callback: Do not change signature
	private void OnSerializeNetworkView( BitStream _stream, NetworkMessageInfo _info )
	{
		if ( _stream.isWriting )
		{
			Quaternion jointRot = this.swivel.rotation;
			Quaternion armRot = this.pivot.rotation;
			NetworkViewID viewID = this.currentTarget == null ? NetworkViewID.unassigned : this.currentTarget.networkView.viewID;
			_stream.Serialize( ref jointRot );
			_stream.Serialize( ref armRot );
			_stream.Serialize( ref viewID );
		}
		else if ( _stream.isReading )
		{
			Quaternion jointRot = Quaternion.identity;
			Quaternion armRot = Quaternion.identity;
			NetworkViewID viewID = NetworkViewID.unassigned;
			_stream.Serialize( ref jointRot );
			_stream.Serialize( ref armRot );
			_stream.Serialize( ref viewID );

			this.swivel.rotation = jointRot;
			this.pivot.rotation = armRot;
			if ( viewID != NetworkViewID.unassigned )
			{
				this.currentTarget = TargetManager.instance.GetTargetWithID( viewID );
			}
		}
	}

	private void ParentToOwnerShip( GamePlayer _player )
	{
		this.transform.parent = _player.capitalShip.depthControl;
	}

	private void OwnerInitialise()
	{
		this.ownerInitialised = true;

		int ownerID = this.ownerControl.ownerID.Value;
		this.health.Owner = GamePlayerManager.instance.GetPlayerWithID( ownerID );

		if ( this.health.Owner.capitalShip == null )
		{
			DebugConsole.Warning( "Turret instantiated by non-commander player", this );
		}
		
		this.ParentToOwnerShip( this.health.Owner );

		this.restrictions.teams = (int)Common.OpposingTeam( this.health.Owner.team );

		if ( Network.peerType != NetworkPeerType.Disconnected && !this.networkView.isMine )
		{
			this.enabled = false;
		}
	}
}
