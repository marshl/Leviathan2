using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletDescriptorManager : MonoBehaviour
{
	public static BulletDescriptorManager instance;
	
	public Dictionary<WEAPON_TYPE, BulletDescriptor> descMap;

	private void Awake()
	{
		BulletDescriptorManager.instance = this;

		this.descMap = new Dictionary<WEAPON_TYPE, BulletDescriptor>();
		BulletDescriptor[] descs = this.gameObject.GetComponentsInChildren<BulletDescriptor>();
		foreach ( BulletDescriptor desc in descs )
		{
			this.descMap.Add( desc.weaponType, desc );
		}
	}

	public BulletDescriptor GetDescOfType( WEAPON_TYPE _weaponType )
	{
		BulletDescriptor desc = null;

		if ( this.descMap.TryGetValue( _weaponType, out desc ) == false )
		{
			DebugConsole.Error( "Could not find bullet descriptor of type " + _weaponType, this );
		}

		return desc;
	}
}
