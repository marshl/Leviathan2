using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// The base descriptor for any weapon
/// </summary>
public abstract class WeaponDescriptor : MonoBehaviour
{
	/// <summary>
	/// The bullet that is fired by this weapon
	/// </summary>
	public BulletDescriptor bulletDesc;

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
}

/// <summary>
/// The attribute used to store the System Type of the weapon attached to this descriptor
/// </summary>
public class WeaponTypeAttribute : Attribute
{
	public System.Type type;

	public WeaponTypeAttribute( System.Type _type )
	{
		this.type = _type;
	}
}
