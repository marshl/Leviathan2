using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum BULLET_TYPE : int
{
	NONE,
	MISSILE,
	LIGHT_LASER,
};

public enum WEAPON_TYPE : int
{
	NONE,
	LIGHT_LASER,
	HEAVY_LASER,
	GATLING_LASER,
	BURST_FIRE_TEST,
};

/// <summary>
/// The singleton class that handles the creation and allocation of bullets in the game scene
/// Bullet descriptors should be attached to this object
/// </summary>
public class BulletManager : MonoBehaviour 
{
	public static BulletManager instance;

	/// <summary>
	/// The map of bullet types to bullet bucket
	/// </summary>
	private Dictionary<BULLET_TYPE, BulletBucket> bulletDictionary;

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
		GameObject bulletBucketObj = new GameObject();
		bulletBucketObj.name = "BulletContainer";

		BulletDescriptor[] descriptors = this.GetComponents<BulletDescriptor>();
		this.bulletDictionary = new Dictionary<BULLET_TYPE, BulletBucket>( descriptors.Length );

		foreach ( BulletDescriptor desc in descriptors )
		{
			this.CreateBulletBucket( desc );
		}
	}
	
	public BulletBase FindAvailableBullet( BULLET_TYPE _bulletType ) 
	{
		BulletBucket bucket = null;
		bool foundList = this.bulletDictionary.TryGetValue( _bulletType, out bucket );
		if ( foundList == false )
		{
			Debug.LogError( "Could not find bullet bucket of type \"" + _bulletType.ToString() + "\"" );
			return null;
		}
		return bucket.GetAvailableBullet();
	}

	/*public T FindAvailableBullet<T>() where T : BulletBase
	{
		return this.FindAvailableBullet( typeof(T) ) as T;
	}*/

	public BulletBase ShootBullet( BulletDescriptor _bulletDesc,
	                              Vector3 _pos, Vector3 _forward,
	                              float _spread = 0.0f, float _rotation = 0.0f )
	{
		BulletBase bulletScript = this.FindAvailableBullet( _bulletDesc.bulletType );
		if ( bulletScript == null )
		{
			Debug.LogError( "Error shooting bullet of type \"" + _bulletDesc.bulletType + "\"" );
			return null;
		}
		bulletScript.Reset();

		GameObject bulletObj = bulletScript.gameObject;
		bulletObj.SetActive( true );
		bulletObj.transform.position = _pos;
		bulletObj.transform.rotation = Quaternion.LookRotation( _forward );

		if ( _spread != 0.0f )
		{
			Vector3 perp = new Vector3( UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f );
			perp.Normalize();
			perp = bulletObj.transform.TransformDirection( perp );
			bulletObj.transform.Rotate( perp, UnityEngine.Random.Range( -_spread, _spread ) );
		}
		bulletScript.OnShoot();
		return bulletScript;
	}

	public BulletBase ShootWeapon( WeaponDescriptor _weapon )
	{
		return this.ShootBullet( _weapon.bulletDesc, _weapon.transform.position, _weapon.transform.forward );
	}

	private void CreateBulletBucket( BulletDescriptor _desc )
	{
		//BulletBase prefabScript = _desc.prefab.GetComponent<BulletBase>();
		//BULLET_TYPE bulletType = _desc.bulletType;
		
		if ( this.bulletDictionary.ContainsKey( _desc.bulletType ) )
		{
			Debug.LogError( "Bullet type \"" + _desc.bulletType.ToString() + "\" already used.", _desc.prefab );
			return;
		}
	
		GameObject bucketObj = new GameObject();
		bucketObj.name = _desc.bulletType.ToString() + "Bucket";
		bucketObj.transform.parent = this.transform;

		BulletBucket bulletBucket = bucketObj.AddComponent<BulletBucket>();
		bulletBucket.bulletDesc = _desc;
		bulletBucket.CreateBulletList();
	
		this.bulletDictionary.Add ( _desc.bulletType, bulletBucket );
	}
}
