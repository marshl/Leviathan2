using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretBehavior : BaseWeaponManager
{
	public Transform joint;
	public Transform arm;
	public float rotationSpeed;

	public bool chrisTurret = false;
	public float pitchOffset = 0;

	public float minimumPitchAngle = 180;

	public WeaponBase weapon;
	public BaseHealth health;

	private float range; //TODO: The weapon situation is a disaster that has to be fixed LM 12/05/14

	public NetworkOwnerControl ownerControl;
	private bool ownerInitialised = false;

	private void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		NetworkOwnerManager.instance.RegisterUnknownObject( this );
	}

	protected override void Awake()
	{
		base.Awake();

		// If playing locally, this has to be parented to the capital ship here
		/*if ( Network.peerType == NetworkPeerType.Disconnected
		    && GamePlayerManager.instance.GetPlayerWithID( -1 ).capitalShip != null )
		{
			this.ParentToOwnerShip( GamePlayerManager.instance.GetPlayerWithID( -1 ) );
		}*/

	}
	protected void Start()
	{
		BULLET_TYPE type = this.weapon.weaponDesc.bulletType;
		BulletDescriptor desc = BulletDescriptorManager.instance.GetDescOfType( type );
		this.range = desc.maxDistance;
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

		if ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected )
		{
			if ( this.currentTarget == null
			 || (this.transform.position - this.currentTarget.health.transform.position).magnitude > this.range )
			{
				TargetManager.instance.GetTargetsFromPlayer( this, this.transform, this.range, -1, Common.OpposingTeam( this.health.team ) );
				//this.target = TargetManager.instance.GetBestTarget( this.arm, -1, this.range, Common.OpposingTeam( this.health.team ) );
				Debug.DrawRay( this.arm.transform.position, this.arm.transform.forward );
			}
			if ( this.currentTarget != null )
			{
				this.TurretAim();
				this.gameObject.GetComponent<WeaponBase>().SendFireMessage();
			}
		}
	}

	protected Vector3 TurretAim()
	{
		float speed = BulletDescriptorManager.instance.GetDescOfType( this.weapon.weaponDesc.bulletType ).moveSpeed;

		Vector3 leadPosition = Common.GetTargetLeadPosition( this.arm.position, this.currentTarget.health.transform, speed );

		if ( leadPosition == this.arm.position )
		{
			return Vector3.zero;
		}

		//Predict the target's location ahead based on the shot speed and current velocity
		Vector3 direction = (leadPosition - this.arm.position).normalized;

		//create the rotation we need to be in to look at the target
		Quaternion lookRotation = Quaternion.LookRotation( direction );

		//copy the quaternion into a new variable so we can mess with it safely
		Quaternion newRot = lookRotation;

		//Wrapping code
		float threshold = lookRotation.eulerAngles.x;

		if ( threshold > 180 )
		{
			threshold -= 360 ;
		}

		//Turret pitch clamp
		if ( threshold > minimumPitchAngle )
		{
			newRot = Quaternion.Euler(minimumPitchAngle,lookRotation.eulerAngles.y,lookRotation.eulerAngles.z);
		}

		if ( chrisTurret )
		{
			Quaternion yRot = Quaternion.Euler( new Vector3(0, newRot.eulerAngles.y, 0) );

			joint.rotation = Quaternion.Slerp( joint.rotation, yRot, Time.deltaTime * rotationSpeed );

			Quaternion zRot = Quaternion.Euler( new Vector3( newRot.eulerAngles.x, joint.rotation.eulerAngles.y, joint.rotation.eulerAngles.z ) );
			arm.rotation = Quaternion.Slerp( arm.rotation, zRot, Time.deltaTime * rotationSpeed );
		}
		else
		{
			transform.rotation = Quaternion.Slerp( transform.rotation, newRot, Time.deltaTime * rotationSpeed );
		}
		return leadPosition;
	}

	// Unity Callback: Do not change signature
	private void OnSerializeNetworkView( BitStream _stream, NetworkMessageInfo _info )
	{
		if ( _stream.isWriting )
		{
			Quaternion jointRot = this.joint.rotation;
			Quaternion armRot = this.arm.rotation;
			NetworkViewID viewID = this.currentTarget == null ? this.networkView.viewID : this.currentTarget.health.networkView.viewID;
			_stream.Serialize( ref jointRot );
			_stream.Serialize( ref armRot );
			_stream.Serialize( ref viewID );
		}
		else if ( _stream.isReading )
		{
			Quaternion jointRot = Quaternion.identity;
			Quaternion armRot = Quaternion.identity;
			NetworkViewID viewID = this.networkView.viewID;
			_stream.Serialize( ref jointRot );
			_stream.Serialize( ref armRot );
			_stream.Serialize( ref viewID );

			this.joint.rotation = jointRot;
			this.arm.rotation = armRot;
			if ( viewID != this.networkView.viewID )
			{
				this.currentTarget = new TargetManager.Target( TargetManager.instance.GetTargetWithID( viewID ), -1, -1 );
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
		
		if ( !this.networkView.isMine )
		{
			this.enabled = false;
		}
	}
}
