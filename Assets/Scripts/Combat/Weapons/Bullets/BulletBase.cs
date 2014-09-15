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

	/// <summary>
	/// The distance this bullet has travelled so far (used for deletion)
	/// </summary>
	public float distanceTravelled;
	
	public BaseWeaponManager source;

	//public Vector3 lastPosition;

	/// <summary>
	/// The associated bullet desciptor (set elsewhere)
	/// </summary>
	[HideInInspector]
	public BulletDescriptor desc;

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
		//this.lastPosition = this.transform.position;
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

		//DetectCollision();

		//lastPosition = this.transform.position;
	}
	/// <summary>
	/// Custom collision detection method to handle high-speed physics.
	/// </summary>
	/*protected virtual void DetectCollision()
	{
		//TODO: Convert raycast to spherecast for shot radius to matter, using the collider radius as the size

		Ray positionCheckRay = new Ray(this.transform.position, this.lastPosition - this.transform.position);
		RaycastHit[] rayInfo;
		float distance = Vector3.Distance (this.transform.position, this.lastPosition);
		rayInfo = Physics.RaycastAll (positionCheckRay, distance);
		
		bool hitOccurred = false;
		
		foreach (RaycastHit hit in rayInfo)
		{

			//Ugly fix here - namecheck for capital collider should eventually be the physics layer
			if(hit.collider != null && hit.collider != source.collider && hit.transform.name != "CapitalCollider")
			{
				//DebugConsole.Log("Collided with " + hit.collider.name + " at distance " + distance);
				//OnTriggerEnter(hit.collider);
				
				BaseHealth health = hit.collider.gameObject.GetComponent<BaseHealth>();
				if ( health != null )
				{
					//_health.DealDamage( this.desc.damage, true );
					this.OnTargetCollision( health );
					hitOccurred = true;
				}
				else
				{
					//this.OnEmptyCollision();
				}
				
			}
		}
		if(hitOccurred)
		{
			BulletManager.instance.DestroyLocalBullet( this );
		}
	}
*/
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
		if ( this.source.collider == _collider )
		{
			return;
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
			_health.DealDamage( this.desc.damage, true, this.source.networkView.viewID );
		}

		BulletManager.instance.DestroyLocalBullet( this );
	}

	/// <summary>
	/// Called when this bullet hits an object that does not have a target attached
	/// </summary>
	public virtual void OnEmptyCollision()
	{
		//DebugConsole.Log("Empty collision");
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

		//this.lastPosition = this.transform.position;
	}
}

