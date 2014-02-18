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
	
	private BulletBase FindAvailableBullet( BULLET_TYPE _bulletType ) 
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
	
	public BulletBase ShootBullet( BULLET_TYPE _bulletType, 
	                              Vector3 _pos, Vector3 _forward,
	                              float _spread = 0.0f )
	{
		BulletBase bulletScript = this.FindAvailableBullet( _bulletType );
		if ( bulletScript == null )
		{
			Debug.LogError( "Error shooting bullet of type \"" + _bulletType + "\"", this );
			return null;
		}
		bulletScript.Reset();

		GameObject bulletObj = bulletScript.gameObject;
		bulletObj.SetActive( true );
		bulletObj.transform.position = _pos;
		bulletObj.transform.rotation = Quaternion.LookRotation( _forward );
		bulletObj.collider.enabled = true;

		if ( _spread != 0.0f )
		{
			Vector3 perp = new Vector3( UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f );
			perp.Normalize();
			perp = bulletObj.transform.TransformDirection( perp );
			bulletObj.transform.Rotate( perp, UnityEngine.Random.Range( -_spread, _spread ) );
		}

		if ( GameNetworkManager.instance != null )
		{
			GameNetworkManager.instance.SendShootBulletMessage( _bulletType, bulletScript.index, _pos, bulletObj.transform.forward );
		}

		bulletScript.OnShoot();
		return bulletScript;
	}

	public void CreateDumbBullet( BULLET_TYPE _bulletType, int _index, Vector3 _position, Vector3 _forward )
	{
		BulletBase bulletScript = this.bulletDictionary[_bulletType].bulletList[_index];
		bulletScript.Reset();
		
		GameObject bulletObj = bulletScript.gameObject;
		bulletObj.SetActive( true );
		bulletObj.transform.position = _position;
		bulletObj.transform.rotation = Quaternion.LookRotation( _forward );
		bulletObj.collider.enabled = false;

		bulletScript.OnShoot();
	}


	public void DestroyActiveBullet( BULLET_TYPE _bulletType, int _index )
	{
		BulletBase bulletScript = this.bulletDictionary[_bulletType].bulletList[_index];
		bulletScript.gameObject.SetActive( false );
		GameNetworkManager.instance.SendDestroyBulletMessage( _bulletType, _index );
	}

	public void DestroyInactiveBullet( BULLET_TYPE _bulletType, int _index )
	{
		BulletBase bulletScript = this.bulletDictionary[_bulletType].bulletList[_index];
		bulletScript.gameObject.SetActive( false );
	}

	private void CreateBulletBucket( BulletDescriptor _desc )
	{
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
