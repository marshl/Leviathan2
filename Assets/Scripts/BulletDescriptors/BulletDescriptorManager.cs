using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletDescriptorManager : MonoBehaviour
{
	public static BulletDescriptorManager instance = null;
	
	private Dictionary<BULLET_TYPE, BulletDescriptor> descMap;

	private void Awake()
	{
		BulletDescriptorManager.instance = this;

		this.descMap = new Dictionary<BULLET_TYPE, BulletDescriptor>();
		BulletDescriptor[] descs = this.gameObject.GetComponents<BulletDescriptor>();
		foreach ( BulletDescriptor desc in descs )
		{
			this.descMap.Add( desc.bulletType, desc );
		}
	}

	public BulletDescriptor GetDescOfType( BULLET_TYPE _bulletType )
	{
		BulletDescriptor desc = null;

		if ( this.descMap.TryGetValue( _bulletType, out desc ) == false )
		{
			Debug.LogError( "Could not find bullet descriptor of type " + _bulletType, this );
		}

		return desc;
	}
}
