using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretBehavior : BaseWeaponManager
{
	public BaseHealth target;

	public Transform joint;
	public Transform arm;
	public float rotationSpeed;

	private Vector3 direction;
	private Quaternion lookRotation;
	private Quaternion newRot;

	public bool chrisTurret = false;
	public float pitchOffset = 0;

	public float minimumPitchAngle = 180;

	public WeaponBase weapon;
	public BaseHealth health;

	private float range; //TODO: The weapon situation is a disaster that has to be fixed LM 12/05/14

	private void OnNetworkInstantiate( NetworkMessageInfo _info )
	{ 
		Debug.Log( "TurretBehaviour: OnNetworkInstantiate", this );
		int ownerID = Common.NetworkID( this.networkView.owner );

		GamePlayer ownerPlayer = GamePlayerManager.instance.GetPlayerWithID( ownerID );

		if ( ownerPlayer.capitalShip == null )
		{
			Debug.LogWarning( "Turret instantiated by non-commander player", this );
		}

		this.ParentToOwnerShip( ownerPlayer );

		this.health.teamNumber = ownerPlayer.team;
	}

	protected override void Awake()
	{
		base.Awake();

		if ( Network.peerType == NetworkPeerType.Disconnected
		    && GamePlayerManager.instance.GetPlayerWithID( -1 ).capitalShip != null )
		{
			this.ParentToOwnerShip( GamePlayerManager.instance.GetPlayerWithID( -1 ) );
		}

	}
	protected void Start()
	{
		BULLET_TYPE type = this.weapon.weaponDesc.bulletType;
		BulletDescriptor desc = BulletDescriptorManager.instance.GetDescOfType( type );
		this.range = desc.maxDistance;
	} 

	private void Update()
	{
		if ( this.networkView.isMine )
		{
			if ( this.target == null
			 || (this.transform.position - this.target.transform.position).magnitude > this.range )
			{
				this.target = TargetManager.instance.GetBestTarget( this.arm, -1, this.range, Common.OpposingTeam( this.health.teamNumber ) );
			}
			if ( this.target != null )
			{
				this.TurretAim();
				this.gameObject.GetComponent<WeaponBase>().SendFireMessage();
			}
		}
	}

	private void TurretAim()
	{
		float speed = BulletDescriptorManager.instance.GetDescOfType( this.weapon.weaponDesc.bulletType ).moveSpeed;

		Vector3 leadPosition = Common.GetTargetLeadPosition( this.transform.position, this.target.transform, speed );

		if ( leadPosition == this.transform.position )
		{
			return;
		}

		//Predict the target's location ahead based on the shot speed and current velocity
		direction = (leadPosition - transform.position).normalized;

		//create the rotation we need to be in to look at the target
		lookRotation = Quaternion.LookRotation( direction );

		//copy the quaternion into a new variable so we can mess with it safely
		newRot = lookRotation;

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
	}

	// Unity Callback: Do not change signature
	private void OnSerializeNetworkView( BitStream _stream, NetworkMessageInfo _info )
	{
		if ( _stream.isWriting )
		{
			Quaternion jointRot = this.joint.rotation;
			Quaternion armRot = this.arm.rotation;
			NetworkViewID viewID = this.target == null ? this.networkView.viewID : this.target.networkView.viewID;
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
				this.target = TargetManager.instance.GetTargetWithID( viewID );
			}
			
		}
	}

	private void ParentToOwnerShip( GamePlayer _player )
	{ 
		this.transform.parent = _player.capitalShip.transform;
	}
}
