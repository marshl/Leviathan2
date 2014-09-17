using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretBehavior : BaseWeaponManager
{
	public Transform joint;
	public Transform arm;
	public float rotationSpeed;

	public float minPitchAngle;
	public float maxPitchAngle;
	public float pitchSpeed;

	public WeaponBase weapon;

	public float fireLockAngle;
	
	public NetworkOwnerControl ownerControl;
	private bool ownerInitialised = false;

	protected override void Awake()
	{
		base.Awake();

		this.restrictions.types = (int)( TARGET_TYPE.FIGHTER );
		this.restrictions.ignoreBelowHorizon = true;
		this.restrictions.transform = this.arm;
	}

#if UNITY_EDITOR
	private void Start()
	{
		if ( Network.peerType == NetworkPeerType.Disconnected
		  && this.health != null )
		{
			this.restrictions.teams = (int)Common.OpposingTeam( this.health.team );
		}
	}
#endif

	private void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		NetworkOwnerManager.instance.RegisterUnknownObject( this );
	}

	protected virtual void Update()
	{
		if ( this.ownerInitialised == false 
		    && this.ownerControl.ownerID != -1 )
		{
			int playerID = this.ownerControl.ownerID;
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
				Vector3 vectorToFront = this.arm.transform.forward;
				float angleToLeadPos = Vector3.Dot( vectorToLeadPos, vectorToFront );

				if ( angleToLeadPos >= 1.0f - this.fireLockAngle )
				{
					this.weapon.SendFireMessage();
				}
			}
		}
	}

	protected Vector3 TurretAim( Transform _target )
	{
		float speed = BulletDescriptorManager.instance.GetDescOfType( this.weapon.weaponType ).moveSpeed;
		Vector3 leadPosition = Common.GetTargetLeadPosition( this.arm.position, _target, speed );

		if ( leadPosition == this.arm.position )
		{
			return Vector3.zero;
		}
	
		Vector3 direction = (leadPosition - this.arm.position).normalized;
	
		float yawAngle = Common.AngleAroundAxis( this.transform.forward, direction, this.transform.up );
		Quaternion targetYaw = Quaternion.AngleAxis( yawAngle, Vector3.up );
		this.joint.localRotation = Quaternion.Slerp( this.joint.localRotation, targetYaw, Time.deltaTime * this.rotationSpeed );

		float pitchAngle = Common.AngleAroundAxis( this.transform.up, direction, this.joint.right);
		pitchAngle = Mathf.Clamp( pitchAngle, this.minPitchAngle, this.maxPitchAngle ) - 90.0f;
		Quaternion targetPitch = Quaternion.AngleAxis( pitchAngle, Vector3.right );
		this.arm.localRotation = Quaternion.Slerp( this.arm.localRotation, targetPitch, Time.deltaTime * this.pitchSpeed );
	
		return leadPosition;
	}

	// Unity Callback: Do not change signature
	private void OnSerializeNetworkView( BitStream _stream, NetworkMessageInfo _info )
	{
		if ( _stream.isWriting )
		{
			Quaternion jointRot = this.joint.rotation;
			Quaternion armRot = this.arm.rotation;
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

			this.joint.rotation = jointRot;
			this.arm.rotation = armRot;
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
		this.ownerInitialised = false;

		int ownerID = this.ownerControl.ownerID;
		GamePlayer ownerPlayer = GamePlayerManager.instance.GetPlayerWithID( ownerID );
		
		if ( ownerPlayer.capitalShip == null )
		{
			DebugConsole.Warning( "Turret instantiated by non-commander player", this );
		}
		
		this.ParentToOwnerShip( ownerPlayer );
		this.health.team = ownerPlayer.team;

		this.restrictions.teams = (int)Common.OpposingTeam( this.health.team );

		if ( !this.networkView.isMine )
		{
			this.enabled = false;
		}
	}
}
