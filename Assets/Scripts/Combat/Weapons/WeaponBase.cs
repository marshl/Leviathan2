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

	[HideInInspector]
	public WeaponDescriptor weaponDesc;
	
	public WeaponFirePoint[] firePoints;
	public int firePointIndex;
	public float timeSinceShot;

	public float currentEnergy = 1.0f;

	public BaseWeaponManager source;

	protected virtual void Start()
	{
		this.InitialiseDescriptors();
	}

	protected virtual void Update()
	{
		this.timeSinceShot += Time.deltaTime;

		this.currentEnergy = Mathf.Clamp( this.currentEnergy
		                                      + this.weaponDesc.energyRechargePerSecond * Time.deltaTime,
		                                      0.0f, 1.0f );
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
	/// Starts the firing procedure, accepting in a damage scale parameter
	/// </summary>

	public virtual bool SendFireMessage(float damageScale)
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
			this.currentEnergy -= this.weaponDesc.energyCostPerShot;
			damageScale = Mathf.Max( this.currentEnergy / WeaponDescManager.instance.energyPenaltyThreshold,
			                        WeaponDescManager.instance.minEnergyDamageMultiplier );
		}

		// Create a new list of bullets with capacity equal to number of bullets to be fired
		List<BulletBase> bulletsFired = new List<BulletBase>( this.weaponDesc.fireInSync ? this.firePoints.Length : 1 );

		if ( this.weaponDesc.fireInSync == true )
		{
			for ( int i = 0; i < this.firePoints.Length; ++i )
			{
				WeaponFirePoint currentFirePoint = this.firePoints[i];
				BulletBase bullet = BulletManager.instance.CreateBullet
				(
					this.source,
					this.weaponDesc.weaponType,
					currentFirePoint.transform.position, 
					currentFirePoint.transform.forward, 
					this.weaponDesc.spread,
					damageScale
				);
			
				bulletsFired.Add( bullet );
			}
		}
		else
		{
			WeaponFirePoint currentFirePoint = this.firePoints[this.firePointIndex];
			BulletBase bullet = BulletManager.instance.CreateBullet
			(
				this.source,
				this.weaponDesc.weaponType,
				currentFirePoint.transform.position, 
				currentFirePoint.transform.forward, 
				this.weaponDesc.spread,
				damageScale
			);

			if ( bullet == null )
			{
				DebugConsole.Error( "Error firing weapon \"" + this.GetType().ToString() + "\"", this );
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

	public void FocusFirePoints( Vector3 _direction )
	{
		foreach ( WeaponFirePoint firePoint in this.firePoints )
		{
			firePoint.transform.LookAt( firePoint.transform.position + _direction );
		}
	}
}
