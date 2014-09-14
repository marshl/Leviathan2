using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// The manager for all weapon descriptors
/// </summary>
public class WeaponDescManager : MonoBehaviour
{
	public static WeaponDescManager instance;

	private Dictionary<WEAPON_TYPE, WeaponDescriptor> descriptorMap;

	private void Awake()
	{
		if ( WeaponDescManager.instance != null )
		{
			DebugConsole.Log( "Duplicate WeaponDescManager found.", WeaponDescManager.instance );
			DebugConsole.Log( "Duplicate WeaponDescManager found.", this );
		}
		WeaponDescManager.instance = this;

		this.descriptorMap = new Dictionary<WEAPON_TYPE, WeaponDescriptor>();
		this.FindWeaponDescriptors();
	}

	/// <summary>
	/// Used to find and add all descriptors attached to this object
	/// </summary>
	private void FindWeaponDescriptors()
	{
		WeaponDescriptor[] descArray = this.gameObject.GetComponentsInChildren<WeaponDescriptor>();
		foreach ( WeaponDescriptor weaponDesc in descArray )
		{
			if ( descriptorMap.ContainsKey( weaponDesc.weaponType ) )
			{
				DebugConsole.Warning( "Duplicate descriptor \"" + weaponDesc.name + "\" found.", weaponDesc );
				continue;
			}

			if ( weaponDesc.bulletType == BULLET_TYPE.NONE )
			{
				DebugConsole.Warning( "No bullet type set on Weapon Descriptor \"" + weaponDesc + "\"", weaponDesc );
				continue;
			}

			weaponDesc.bulletDesc = BulletDescriptorManager.instance.GetDescOfType( weaponDesc.bulletType );

			this.descriptorMap.Add( weaponDesc.weaponType, weaponDesc );
		}
	}

	/// <summary>
	/// Returns the descriptor object of the given type
	/// </summary>
	/// <returns>The desc of type.</returns>
	/// <param name="_type">_type.</param>
	public WeaponDescriptor GetDescOfType( WEAPON_TYPE _weaponType )//System.Type _type )
	{
		WeaponDescriptor desc;
		if ( this.descriptorMap.TryGetValue( _weaponType, out desc ) == false )
		{
			DebugConsole.Error( "Could not find weapon descriptor for weapon type \"" + _weaponType + "\"", this );
			return null;
		}
		return desc;
	} 
}