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
#if UNITY_EDITOR
		this.CheckDescriptors();
#endif
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

	private void CheckDescriptors()
	{
		WEAPON_TYPE[] weaponTypes = System.Enum.GetValues( typeof(WEAPON_TYPE) ) as WEAPON_TYPE[];

		foreach ( WEAPON_TYPE weaponType in weaponTypes )
		{
			if ( weaponType == WEAPON_TYPE.NONE )
			{
				continue;
			}

			if ( this.descMap.ContainsKey( weaponType ) == false )
			{
				throw new System.Exception( "No bullet descriptor found for " + weaponType );
			}
		}
	}
}
