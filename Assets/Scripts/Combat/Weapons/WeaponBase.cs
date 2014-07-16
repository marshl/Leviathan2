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
	[HideInInspector]
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
	
	public BaseWeaponManager source;

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
					this.weaponDesc.bulletType,
					currentFirePoint.transform.position, 
					currentFirePoint.transform.forward, 
					this.weaponDesc.spread
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
				this.weaponDesc.bulletType,
				currentFirePoint.transform.position, 
					currentFirePoint.transform.forward, 
				this.weaponDesc.spread
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

	/*protected Vector3 GetAimVector(WeaponFirePoint _firePoint)
	{
		//Cast a ray to determine if there's an object under the cursor or not

		RaycastHit targetAim = new RaycastHit();
		Ray rayToCast = Camera.main.ScreenPointToRay( Input.mousePosition );

		//DebugConsole.Log ("Ray casting: " + rayToCast.direction.ToString ());
		//DebugConsole.Log ("Input.MousePosition: " + Input.mousePosition.ToString ());

		if(this.gameObject.GetComponent<FighterMaster>() != null)
		{
			if(Physics.Raycast(rayToCast, out targetAim))
			{
				if(targetAim.collider == this.collider)
				{
					//DebugConsole.Log ("Self-collision");
					return rayToCast.direction;
				}

				//DebugConsole.Log ("Final result: " + (targetAim.point - _firePoint.transform.position).ToString ());
				return targetAim.point - _firePoint.transform.position ;
				
			}
			else
			{
				//DebugConsole.Log ("Final result: " + rayToCast.direction.ToString());
				return rayToCast.direction;
			}
		}

		return _firePoint.transform.forward;
		

	}*/

	public void FocusFirePoints( Vector3 _direction )
	{
		foreach ( WeaponFirePoint firePoint in this.firePoints )
		{
			firePoint.transform.LookAt( firePoint.transform.position + _direction );
		}
	}
}
