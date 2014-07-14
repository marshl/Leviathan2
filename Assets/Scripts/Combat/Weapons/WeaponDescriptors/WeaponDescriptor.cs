using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// The base descriptor for any weapon
/// </summary>
public class WeaponDescriptor : MonoBehaviour
{
	public WEAPON_TYPE weaponType;
	
	/// <summary>
	/// The bullet that is fired by this weapon
	/// </summary>
	//public BulletDescriptor bulletDesc;

	/// <summary>
	/// The time between shots
	/// </summary>
	public float fireRate;

	/// <summary>
	/// The degrees of bullet spread when fired
	/// </summary>
	public float spread;

	/// <summary>
	/// Should this weapon fire from all fire points at once
	/// </summary>
	public bool fireInSync;

	public BULLET_TYPE bulletType;
}
