using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BulletManager : MonoBehaviour 
{
	public static BulletManager instance;
	// Inspector Variables
	public GameObject[] bulletPrefabs;
	public BulletDescriptor[] bulletDescriptors;
	public GameObject bulletDescriptorObj;

	// Private variables
	private Dictionary<System.Type, BulletBase[]> bulletDictionary;
	private GameObject bulletBucketObj;

	private void Awake()
	{
		if ( instance != null )
		{
			Debug.LogError( "Duplicate instance of Bullet Manager found", instance );
			Debug.LogError( "Duplicate instance of Bullet Manager found", this );
		}
		instance = this;
	}

	private void Start()
	{
		this.bulletBucketObj = new GameObject();
		bulletBucketObj.name = "BulletContainer";

		this.bulletDictionary = new Dictionary<System.Type, BulletBase[]>( this.bulletPrefabs.Length );

		BulletDescriptor[] descriptors = this.bulletDescriptorObj.GetComponents<BulletDescriptor>();
		 
		foreach ( BulletDescriptor desc in descriptors )
		//foreach ( GameObject prefab in this.bulletPrefabs )
		{
			if ( desc.prefab == null )
			{
				Debug.LogError( "No prefab bullet found on descriptor \"" + desc.name + "\"", desc );
				continue;
			}
			BulletBase prefabScript = desc.prefab.GetComponent<BulletBase>();
			System.Type bulletType = prefabScript.GetType();
			//desc.bulletType = bulletType;

			if ( this.bulletDictionary.ContainsKey( bulletType ) )
			{
				Debug.LogError( "Bullet type \"" + bulletType.ToString() + "\" already used.", desc.prefab );
			}

			/*BulletStatsAttribute bulletStats = this.GetStatsOfBullet( bulletType );

			if ( bulletStats == null )
			//if ( bulletType.IsDefined( typeof(BulletStatsAttribute), false ) == false )
			{
				Debug.LogError( "Bullet stats attribute not found on prefab object \"" + desc.prefab.name + "\"", desc.prefab );
				continue;
			}*/

			//int bulletCount = bulletStats.count;
			int bulletCount = desc.count;

			GameObject subBucket = new GameObject();
			subBucket.name = bulletType.ToString() + "Bucket";
			subBucket.transform.parent = this.bulletBucketObj.transform;

			BulletBase[] bulletList = new BulletBase[bulletCount];
			for ( int i = 0; i < bulletCount; ++i )
			{
				GameObject bulletObj = GameObject.Instantiate( desc.prefab ) as GameObject;
				bulletObj.name = bulletType.ToString() + i.ToString();
				bulletObj.transform.parent = subBucket.transform;
				bulletObj.SetActive( false );

				BulletBase bulletScript = bulletObj.GetComponent<BulletBase>();
				//bulletScript.stats = bulletStats;
				bulletScript.desc = desc;
				bulletList[i] = bulletScript;
			}

			this.bulletDictionary.Add( bulletType, bulletList );
		}

		/*GameObject obj = new GameObject();
		Missile missile = this.ShootBullet<Missile>( this.transform.position, this.transform.forward );
		missile.target = obj.AddComponent<TargettableObject>();
		Debug.Log (missile.transform.position);*/
	}

	public BulletStatsAttribute GetStatsOfBullet( System.Type _type )
	{
		object[] attributes = _type.GetCustomAttributes( false );
		foreach ( object attribute in attributes )
		{
			BulletStatsAttribute statsAttribute = attribute as BulletStatsAttribute;
			if ( statsAttribute != null )
			{
				return statsAttribute;
			}
		}
		Debug.LogError( "No bullet stats attribute found on class \"" + typeof(BulletBase).ToString() + "\"" );
		return null;
	}

	public BulletBase FindAvailableBullet( System.Type _bulletType ) 
	{
		BulletBase[] bulletList;
		bool foundList = this.bulletDictionary.TryGetValue( _bulletType, out bulletList );
		if ( foundList == false )
		{
			Debug.LogError( "Could not find bullet list of type \"" + _bulletType.ToString() + "\"" );
			return null;
		}
		
		foreach ( BulletBase bulletScript in bulletList )
		{
			if ( bulletScript.gameObject.activeSelf == false )
			{
				return bulletScript;
			}
		}
		
		Debug.LogError( "Ran out of bullets of type \"" + _bulletType.ToString() + "\"" );
		return null;
	}

	public T FindAvailableBullet<T>() where T : BulletBase
	{
		return this.FindAvailableBullet( typeof(T) ) as T;
	}

	[System.Obsolete()]
	public T ShootBullet<T>( Vector3 _position, Vector3 _forward, float _rotation = 0.0f ) where T : BulletBase
	{
		BulletBase bulletScript = this.FindAvailableBullet<T>();
		if ( bulletScript == null )
		{
			Debug.LogError( "Error shooting bullet of type \"" + typeof(T).ToString() + "\"" );
			return null;
		}
		bulletScript.Reset();

		GameObject bulletObj = bulletScript.gameObject;
		bulletObj.SetActive( true );
		bulletObj.transform.position = _position;
		bulletObj.transform.rotation = Quaternion.AngleAxis( _rotation, _forward );

		float spread = bulletScript.desc.spread;
		if ( spread != 0.0f )
		{
			Vector3 perp = new Vector3( UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), 0.0f );
			perp.Normalize();
			perp = bulletObj.transform.TransformDirection( perp );
			bulletObj.transform.Rotate( perp, UnityEngine.Random.Range( -spread, spread ) );
		}

		return bulletScript as T;
	}

	public BulletBase ShootBullet( BulletDescriptor _bulletDesc,
	                              Vector3 _pos, Vector3 _forward,
	                              float _spread = 0.0f, float _rotation = 0.0f )
	{
		BulletBase bulletScript = this.FindAvailableBullet( _bulletDesc.GetBulletType() );
		if ( bulletScript == null )
		{
			Debug.LogError( "Error shooting bullet of type \"" + _bulletDesc.GetBulletType().ToString() + "\"" );
			return null;
		}
		bulletScript.Reset();
		bulletScript.OnShoot();
		
		GameObject bulletObj = bulletScript.gameObject;
		bulletObj.SetActive( true );
		bulletObj.transform.position = _pos;
		bulletObj.transform.rotation = Quaternion.AngleAxis( _rotation, _forward );
		
		float spread = bulletScript.desc.spread;
		if ( spread != 0.0f )
		{
			Vector3 perp = new Vector3( UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), 0.0f );
			perp.Normalize();
			perp = bulletObj.transform.TransformDirection( perp );
			bulletObj.transform.Rotate( perp, UnityEngine.Random.Range( -spread, spread ) );
		}
		
		return bulletScript;
	}

	public BulletBase ShootWeapon( WeaponDescriptor _weapon )
	{
		return this.ShootBullet( _weapon.bulletDesc, _weapon.transform.position, _weapon.transform.forward );
	}
}
