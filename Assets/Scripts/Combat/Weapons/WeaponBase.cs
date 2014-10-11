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

	[Tooltip( "Descriptors are set automatically and do not have to be preset" )]
	public WeaponDescriptor weaponDesc;

	public WeaponFirePoint[] firePoints;
	public int firePointIndex;
	public EnergySystem energySystem;
	public BaseWeaponManager source;

	public float timeSinceShot;
	public int ammunition;

	public float currentLockOn;

	protected virtual void Start()
	{
		this.InitialiseDescriptors();
		this.ammunition = this.weaponDesc.ammunitionMax;
	}

	protected virtual void Update()
	{
		this.timeSinceShot += Time.deltaTime;
	}

	public void SetWeaponType( WEAPON_TYPE _weaponType )
	{
		this.weaponType = _weaponType;
		this.InitialiseDescriptors();
	}

	/// <summary>
	/// Initialises all of the descriptors on this weapon, including all inherited weapon types
	/// </summary>
	protected virtual void InitialiseDescriptors()
	{
		this.weaponDesc = WeaponDescManager.instance.GetDescOfType( this.weaponType );

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
		return this.timeSinceShot > this.weaponDesc.fireRate
		  && ( this.weaponDesc.usesAmmunition == false || this.ammunition > 0 );
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
			DebugConsole.Error( "No fire points found on weapon \"" + this.ToString() + "\"", this );
			return null;
		}
		this.timeSinceShot = 0.0f;

		float damageScale = 1.0f;
		if ( this.weaponDesc.usesEnergy )
		{
			this.energySystem.ReduceEnergy( this.weaponDesc.energyCostPerShot );
			damageScale = this.energySystem.GetDamageScale();
		}

		// Create a new list of bullets with capacity equal to number of bullets to be fired
		List<BulletBase> bulletsFired = new List<BulletBase>( this.weaponDesc.fireInSync ? this.firePoints.Length : 1 );

		if ( this.weaponDesc.fireInSync == true )
		{
			for ( int i = 0; i < this.firePoints.Length; ++i )
			{
				WeaponFirePoint currentFirePoint = this.firePoints[i];
				bulletsFired.Add( this.FireBulletFromFirePoint( currentFirePoint, damageScale ) );     
			}
		}
		else
		{
			WeaponFirePoint currentFirePoint = this.firePoints[this.firePointIndex];
			bulletsFired.Add( this.FireBulletFromFirePoint( currentFirePoint, damageScale ) );
			this.IncrementFireIndex();
		}
		return bulletsFired;
	}

	private BulletBase FireBulletFromFirePoint( WeaponFirePoint _firePoint, float _damageScale )
	{
		BulletBase bullet = BulletManager.instance.CreateBullet
		(
			this,
			_firePoint, 
			_damageScale );

		--this.ammunition;

		return bullet;
	}

	protected void IncrementFireIndex()
	{
		this.firePointIndex++;
		if ( this.firePointIndex >= this.firePoints.Length )
		{
			this.firePointIndex = 0;
		}
	}

	public void FocusFirePoints( Vector3 _direction )
	{
		foreach ( WeaponFirePoint firePoint in this.firePoints )
		{
			firePoint.transform.LookAt( firePoint.transform.position + _direction );
		}
	}

	public void LockOnUpdate()
	{
		if ( this.IsTrackingTarget() )
		{
			this.currentLockOn += Time.deltaTime;
		}
		else
		{
			this.currentLockOn = 0.0f;
		}
	}

	public void ResetLockOn()
	{
		this.currentLockOn = 0.0f;
	}

	public bool IsTrackingTarget()
	{
		if ( this.source.currentTarget == null )
		{
			return false;
		}

		Vector3 direction = this.source.currentTarget.transform.position - this.transform.position;
		float angle = Vector3.Angle( this.transform.forward, direction );
		
		return angle < this.weaponDesc.lockOnAngle;
	}

	public bool IsLockedOntoTarget()
	{
		return this.source.currentTarget != null
			&& this.currentLockOn >= this.weaponDesc.lockOnDuration;
	}
}
