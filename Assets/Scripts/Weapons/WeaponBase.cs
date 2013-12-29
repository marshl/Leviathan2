using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;

public abstract class WeaponBase : MonoBehaviour
{
	public WeaponDescriptor weaponDesc;

	public WeaponFirePoint[] firePoints;
	public int firePointIndex;

	public float timeSinceShot;

	protected virtual void Start()
	{
		this.SetupDescriptors();
	}

	protected virtual void Update()
	{
		this.timeSinceShot += Time.deltaTime;
	}

	protected System.Type GetDescriptorType()
	{
		object[] attributes = this.GetType().GetCustomAttributes( true );
		foreach ( object attribute in attributes )
		{
			WeaponTypeAttribute typeAttribute = attribute as WeaponTypeAttribute;
			if ( typeAttribute != null )
			{
				return typeAttribute.type;
			}
		}
		Debug.LogError( "No Weapon Descriptor found on \"" + this.GetType().ToString() + "\"" );
		return null;
	}

	protected virtual void SetupDescriptors()
	{
		System.Type descType = this.GetDescriptorType();
		this.weaponDesc = WeaponDescManager.instance.GetDescOfType( descType );

		FieldInfo[] fields = this.GetType().GetFields();
		foreach ( var field in fields )
		{
			System.Type fieldType = field.FieldType;
			if ( fieldType.IsSubclassOf( typeof(WeaponDescriptor) ) )
			{
				field.SetValue( this, this.weaponDesc );
			}
		}
	}

	public virtual bool CanFire()
	{
		if ( this.weaponDesc == null )
		{
			Debug.LogError( "Weapon descriptor on \"" + this.GetType().ToString() + "\" is null.", this );
		}
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

	public virtual List<BulletBase> Fire()
	{
		this.timeSinceShot = 0.0f;

		if ( this.firePoints.Length == 0 )
		{
			Debug.LogError( "No weapon points found on weapon \"" + this.ToString() + "\"", this );
			return null;
		}

		List<BulletBase> bulletsFired = new List<BulletBase>();
		if ( this.weaponDesc.fireInSync == true )
		{
			for ( int i = 0; i < this.firePoints.Length; ++i )
			{
				WeaponFirePoint currentFirePoint = this.firePoints[i];
				BulletBase bullet = BulletManager.instance.ShootBullet
				(
					this.weaponDesc.bulletDesc, 
					currentFirePoint.transform.position, 
					currentFirePoint.transform.forward, 
					this.weaponDesc.spread
				);
				if ( bullet == null )
				{
					Debug.LogError( "Error firing weapon \"" + this.GetType().ToString() + "\"", this );
				}
				bulletsFired.Add( bullet );
			}
			return bulletsFired;
		}
		else
		{
			WeaponFirePoint currentFirePoint = this.firePoints[this.firePointIndex];
			BulletBase bullet = BulletManager.instance.ShootBullet
			(
				this.weaponDesc.bulletDesc, 
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
			return bulletsFired;
		}
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
