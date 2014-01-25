using UnityEngine;
using System.Collections;
using System;


public abstract class BulletBase : MonoBehaviour 
{
	public float distanceTravelled;
	public BulletDescriptor desc;

	public virtual void Start()
	{

	}

	public virtual void OnShoot()
	{
		this.rigidbody.velocity = this.transform.forward * this.desc.moveSpeed;
	}

	protected virtual void Update()
	{
		this.distanceTravelled += this.desc.moveSpeed * Time.deltaTime;
		if ( this.distanceTravelled >= this.desc.maxDistance )
		{
			this.OnLifetimeExpiration();
		}
	}

	protected virtual void OnCollision( Collision _collision )
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

	public virtual void OnLifetimeExpiration()
	{
		this.gameObject.SetActive( false );
	}

	public virtual void OnTargetCollision( TargettableObject _target )
	{
		this.gameObject.SetActive( false );
		return;
	}

	public virtual void OnEmptyCollision()
	{
		this.gameObject.SetActive( false );
	}

	public virtual void Reset()
	{
		this.distanceTravelled = 0.0f;

		this.rigidbody.velocity = Vector3.zero;
		this.rigidbody.angularVelocity = Vector3.zero;
	}
}

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

public class BulletDescAttribute : Attribute
{
	public System.Type descType;

	public BulletDescAttribute( System.Type _type )
	{
		this.descType = _type;
	}
}


