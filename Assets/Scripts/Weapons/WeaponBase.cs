using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// The base class for any weapon instance
/// </summary>
public class WeaponBase : MonoBehaviour
{
	public WEAPON_TYPE weaponType;

	/// <summary>
	/// The descriptor for this weapon
	/// </summary>
	public WeaponDescriptor weaponDesc;

	/// <summary>
	/// The points from which this weapon will fire
	/// </summary>
	public WeaponFirePoint[] firePoints;

	/// <summary>
	/// The index of the fire pint from which this weapon will fire next
	/// </summary>
	public int firePointIndex;

	/// <summary>
	/// The time since this weapon fired last
	/// </summary>
	public float timeSinceShot;


	protected virtual void Start()
	{
		this.InitialiseDescriptors();
	}

	protected virtual void Update()
	{
		this.timeSinceShot += Time.deltaTime;
	}

	/// <summary>
	/// Initialises all of the descriptors on this weapon, including all inherited weapon types
	/// </summary>
	protected virtual void InitialiseDescriptors()
	{
		//System.Type descType = this.GetDescriptorType();
		this.weaponDesc = WeaponDescManager.instance.GetDescOfType( this.weaponType );//descType );

		// Find all fields in this type
		FieldInfo[] fields = this.GetType().GetFields();
		foreach ( FieldInfo field in fields )
		{
			// If that field is a pointer to a type of weapon descriptor, then set it
			System.Type fieldType = field.FieldType;
			if ( fieldType.IsSubclassOf( typeof(WeaponDescriptor) ) )
			{
				field.SetValue( this, this.weaponDesc );
			}
		}
	}

	public virtual bool CanFire()
	{
		return this.timeSinceShot > this.weaponDesc.fireRate;
	}

	public virtual bool SendFireMessage()
	{
		if ( this.CanFire() == false )
		{
			return false;
		}

		this.Fire();
		return true;
	}

	/// <summary>
	/// Fires a bullet, or a series of bullets if FireInSync = true, and returns a list of them
	/// </summary>
	public virtual List<BulletBase> Fire()
	{
		if ( this.firePoints.Length == 0 )
		{
			Debug.LogError( "No fire points found on weapon \"" + this.ToString() + "\"", this );
			return null;
		}
		this.timeSinceShot = 0.0f;

		// Create a new list of bullets with capacity equal to number of bullets to be fired
		List<BulletBase> bulletsFired = new List<BulletBase>( this.weaponDesc.fireInSync ? this.firePoints.Length : 1 );

		if ( this.weaponDesc.fireInSync == true )
		{
			for ( int i = 0; i < this.firePoints.Length; ++i )
			{
				WeaponFirePoint currentFirePoint = this.firePoints[i];
				BulletBase bullet = BulletManager.instance.ShootBullet
				(
					this.weaponDesc.bulletType,
					currentFirePoint.transform.position, 
					currentFirePoint.transform.TransformDirection(Vector3.forward), 
					this.weaponDesc.spread
				);

				bulletsFired.Add( bullet );
			}
		}
		else
		{
			WeaponFirePoint currentFirePoint = this.firePoints[this.firePointIndex];
			BulletBase bullet = BulletManager.instance.ShootBullet
			(
				this.weaponDesc.bulletType,
				currentFirePoint.transform.position, 
				currentFirePoint.transform.forward, 
				this.weaponDesc.spread
			);

			if ( bullet == null )
			{
				Debug.LogError( "Error firing weapon \"" + this.GetType().ToString() + "\"", this );
			}
			this.IncrementFireIndex();

			bulletsFired.Add( bullet );
		}
		return bulletsFired;
	}

	protected void IncrementFireIndex()
	{
		this.firePointIndex++;
		if ( this.firePointIndex >= this.firePoints.Length )
		{
			this.firePointIndex = 0;
		}
	}
}
