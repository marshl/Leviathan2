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

	public BULLET_TYPE bulletType;

	public LocalBulletBucket parentBucket;

	/// <summary>
	/// The distance this bullet has travelled so far (used for deletion)
	/// </summary>
	public float distanceTravelled;

	/// <summary>
	/// The associated bullet desciptor (set elsewhere)
	/// </summary>
	[HideInInspector]
	public BulletDescriptor desc;

	protected virtual void Awake()
	{
		this.desc = BulletDescriptorManager.instance.GetDescOfType( this.bulletType );
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
	}

	/// <summary>
	/// Unity callback on collision with another object
	/// </summary>
	protected virtual void OnTriggerEnter( Collider _collider )
	{
		TargettableObject target = _collider.gameObject.GetComponent<TargettableObject>();
		if ( target != null )
		{
			this.OnTargetCollision( target );
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
	public virtual void OnTargetCollision( TargettableObject _target )
	{
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
		this.distanceTravelled = 0.0f;

		this.rigidbody.velocity = Vector3.zero;
		this.rigidbody.angularVelocity = Vector3.zero;
	}

	protected void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
		}
	}
}

