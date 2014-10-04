using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// The base bullet class that is attached to each bullet fired
/// For non-instance information, see the associated bullet descriptor
/// </summary>
public class BulletBase : MonoBehaviour 
{
	public enum BULLET_STATE
	{
		INACTIVE,
		ACTIVE_OWNED,
		ACTIVE_NOT_OWNED,
	};
	
	public BULLET_STATE state;
	public int index;
	public WEAPON_TYPE weaponType;
	public LocalBulletBucket parentBucket;
	public BulletDescriptor desc;

	public BaseWeaponManager source;
	public float distanceTravelled;
	public float damageScale = 1.0f;

	public Vector3 lastPosition;
	private bool specialCollision = false; //Have we tripped a special collision detection check?

	protected virtual void Awake()
	{
		this.desc = BulletDescriptorManager.instance.GetDescOfType( this.weaponType );
	}

	protected virtual void Start()
	{
		// Nothing should be required here as it is only called once
		// Reset is called every time a bullet is created, use that instead

		// However decriptors can be set up here
	}

	/// <summary>
	/// Callback: Called as soon as the bullet is created
	/// </summary>
	public virtual void OnShoot()
	{
		this.rigidbody.velocity = this.transform.forward * this.desc.moveSpeed;
		this.lastPosition = this.transform.position;
		this.specialCollision = false;
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	protected virtual void Update()
	{
		this.distanceTravelled += this.desc.moveSpeed * Time.deltaTime;
		if ( this.distanceTravelled >= this.desc.maxDistance )
		{
			this.OnLifetimeExpiration();
		} 

		//Advanced collision checking - note that it calls OnTriggerEnter by hand.
		/*switch(desc.collisionType)
		{
		case COLLISION_TYPE.COLLISION_SPHERE:
				DetectSphereCollision();
			break;
		case COLLISION_TYPE.COLLISION_DEFAULT:
			break;
		case COLLISION_TYPE.COLLISION_RAY:
				DetectRaycastCollision();
			break;
		default:
			break;

		}*/


		lastPosition = this.transform.position;
	}
	/// <summary>
	/// Custom collision detection method to handle high-speed physics.
	/// </summary>
	protected virtual void DetectRaycastCollision()
	{
		//TODO: Convert raycast to spherecast for shot radius to matter, using the collider radius as the size

		Ray positionCheckRay = new Ray(this.transform.position, this.lastPosition - this.transform.position);
		RaycastHit[] rayInfo;
		float distance = Vector3.Distance (this.transform.position, this.lastPosition);
		int layerMask = ~(1 << 10);
		//print(layerMask);
		rayInfo = Physics.RaycastAll (positionCheckRay, distance, layerMask );

		foreach (RaycastHit hit in rayInfo)
		{
			//	DebugConsole.Log("Collided with " + hit.collider.name + " at distance " + distance);
				OnTriggerEnter(hit.collider);
				
		}

		if(rayInfo.Length > 0)
		{
			this.specialCollision = true;
		}

	}

	protected virtual void DetectSphereCollision()
	{
		//TODO: Convert raycast to spherecast for shot radius to matter, using the collider radius as the size

		RaycastHit[] sphereInfo;
		float distance = Vector3.Distance (this.transform.position, this.lastPosition);
		int layerMask = ~(1 << 10);
		//print(layerMask);
		float sphereRadius = this.GetComponent<SphereCollider>().radius;
		//print(sphereRadius);
		sphereInfo = Physics.SphereCastAll (this.lastPosition, sphereRadius, 
		                                 this.transform.position - this.lastPosition, distance, layerMask );
		
		foreach (RaycastHit hit in sphereInfo)
		{
			//DebugConsole.Log("Sphere collision with " + hit.collider.name + " at distance " + distance);
			OnTriggerEnter(hit.collider);
			//this.specialCollision = true;
			//BulletManager.instance.DestroyLocalBullet (this);
		}
		if(sphereInfo.Length > 0)
		{
			this.specialCollision = true;
		}
		
	}

	/// <summary>
	/// Unity callback on collision with another object
	/// </summary>
	protected virtual void OnTriggerEnter( Collider _collider )
	{
		if ( this.gameObject.activeSelf == false
		  || this.enabled == false )
		{
			return;
		}

		if ( Network.peerType != NetworkPeerType.Disconnected
		  && this.networkView != null
		  && !this.networkView.isMine )
		{
			return;
		}

		//TODO: Quick and nasty fix, may have to be repaired to manage long-term missile collisions LM 08/05/14
		if ( this.source != null
		  && this.source.collider == _collider )
		{
			return;
		}

		if(this.specialCollision)
		{
			Debug.Log("Caught a double trigger event after a freak cosmic ray or something.");
			Debug.Log("Contact me - Rigel");
			return;
		}

		if ( this.desc.areaOfEffect )
		{
			TargetManager.instance.AreaOfEffectDamage( 
	          this.transform.position,
	          this.desc.aoeRadius,
	          this.desc.damage,
	          false, //TODO: Friendly fire bool
	          this.source.health.Owner.team,
	          this.source.health.Owner );
		}

		BaseHealth health = _collider.gameObject.GetComponent<BaseHealth>();
		if ( health != null )
		{
			this.OnTargetCollision( health );
		}
		else
		{
			this.OnEmptyCollision();
		}
	}

	/// <summary>
	/// Called once this bullet has run its distance
	/// </summary>
	public virtual void OnLifetimeExpiration()
	{
		BulletManager.instance.DestroyLocalBullet( this );
	}

	/// <summary>
	/// Called when this bullet hits an object that has a target attached
	/// </summary>
	/// <param name="_target">_target.</param>
	public virtual void OnTargetCollision( BaseHealth _health )
	{
		//DebugConsole.Log("Collided with " + _health.name + " in target collision");
		if ( _health.GetComponent<SeekingBullet>() == null )
		{
			_health.DealDamage( this.desc.damage * this.damageScale, true, this.source.health.Owner );
		}

		BulletManager.instance.DestroyLocalBullet( this );
	}

	/// <summary>
	/// Called when this bullet hits an object that does not have a target attached
	/// </summary>
	public virtual void OnEmptyCollision()
	{
		BulletManager.instance.DestroyLocalBullet( this );
	}

	/// <summary>
	/// Reset this bullet.
	/// </summary>
	public virtual void Reset()
	{
		this.source = null;

		this.distanceTravelled = 0.0f;

		this.rigidbody.velocity = Vector3.zero;
		this.rigidbody.angularVelocity = Vector3.zero;
		this.damageScale = 1.0f;

		//this.lastPosition = this.transform.position;
	}
}

