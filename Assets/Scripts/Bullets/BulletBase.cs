using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// The base bullet class that subclassed and attached to each bullet fired
/// For non-instance information, see the associated bullet descriptor
/// </summary>
public abstract class BulletBase : MonoBehaviour 
{
	/// <summary>
	/// The distance this bullet has travelled so far (used for deletion)
	/// </summary>
	public float distanceTravelled;

	/// <summary>
	/// The associated bullet desciptor (set elsewhere)
	/// </summary>
	[HideInInspector]
	public BulletDescriptor desc;

	public virtual void Start()
	{

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
	protected virtual void OnCollisionEnter( Collision _collision )
	{
		TargettableObject target = _collision.gameObject.GetComponent<TargettableObject>();
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
		this.gameObject.SetActive( false );
	}

	/// <summary>
	/// Called when this bullet hits an object with a TargettableObject script attached
	/// </summary>
	/// <param name="_target">_target.</param>
	public virtual void OnTargetCollision( TargettableObject _target )
	{
		this.gameObject.SetActive( false );
		return;
	}

	/// <summary>
	/// Called when this bullet hits an object that is not targettable
	/// </summary>
	public virtual void OnEmptyCollision()
	{
		this.gameObject.SetActive( false );
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
}

/*
public class BulletStatsAttribute : Attribute
{
	public BulletStatsAttribute( int _count, float _moveSpeed )
	{
		this.count = _count;
		this.moveSpeed = _moveSpeed;
	}

	public int count { get; set; }
	public float moveSpeed { get; set; }
};
*/
/*
public class BulletDescAttribute : Attribute
{
	public System.Type descType;

	public BulletDescAttribute( System.Type _type )
	{
		this.descType = _type;
	}
}
*/

