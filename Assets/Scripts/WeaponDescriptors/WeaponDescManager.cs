using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WeaponDescManager : MonoBehaviour
{
	public static WeaponDescManager instance;

	private Dictionary<System.Type, WeaponDescriptor> descriptorMap;

	private void Awake()
	{
		if ( WeaponDescManager.instance != null )
		{
			Debug.Log( "Duplicate WeaponDescManager found.", WeaponDescManager.instance );
			Debug.Log( "Duplicate WeaponDescManager found.", this );
		}
		WeaponDescManager.instance = this;

		this.FindWeaponDescriptors();
	}

	private void FindWeaponDescriptors()
	{
		this.descriptorMap = new Dictionary<Type, WeaponDescriptor>();

		WeaponDescriptor[] descArray = this.gameObject.GetComponents<WeaponDescriptor>();
		foreach ( WeaponDescriptor weaponDesc in descArray )
		{
			System.Type descType = weaponDesc.GetType();
			if ( descriptorMap.ContainsKey( descType ) )
			{
				Debug.LogError( "Duplicate descriptor \"" + weaponDesc.name + "\" found.", weaponDesc );
				continue;
			}

			//System.Type bulletType = weaponDesc.bulletDesc.GetBulletType();
			this.descriptorMap.Add( descType, weaponDesc );
		}
	}

	public WeaponDescriptor GetDescOfType( System.Type _type )
	{
		WeaponDescriptor desc;
		if ( this.descriptorMap.TryGetValue( _type, out desc ) == false )
		{
			Debug.LogError( "Could not find weapon descriptor for weapon type \"" + _type.ToString() + "\"" );
			return null;
		}
		return desc;
	} 
}

