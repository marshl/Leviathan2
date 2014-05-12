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
	MISSILE_LAUNCHER,
	GATLING_LASER,
	BURST_FIRE_TEST,
	CHARGE_UP_TEST,
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

	private Dictionary<NetworkViewID, BulletBase> networkedBullets;
	/*
#if UNITY_EDITOR
	private int networkBulletID = 0;
#endif*/ 

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
	
		this.bulletDictionary = new Dictionary<BULLET_TYPE, BulletBucket>();
	
		foreach ( KeyValuePair<BULLET_TYPE, BulletDescriptor> pair in BulletDescriptorManager.instance.descMap )
		{ 
			if ( pair.Value.smartBullet == false )
			{
				this.CreateBulletBucket( pair.Value );
			}
		}
	}

	public BulletBase CreateBullet( BaseWeaponManager _source, BULLET_TYPE _bulletType, 
	                              Vector3 _pos, Vector3 _forward,
	                              float _spread = 0.0f )
	{
		BulletDescriptor desc = BulletDescriptorManager.instance.GetDescOfType( _bulletType );

		BulletBase bulletScript = null;
		GameObject bulletObj = null;
		if ( desc.smartBullet == false )
		{
			bulletScript = this.bulletDictionary[ _bulletType ].GetAvailableBullet( -1, -1 );

			if ( bulletScript == null )
			{
				Debug.LogError( "Error shooting bullet of type \"" + _bulletType + "\"", this );
				return null;
			}

			bulletObj = bulletScript.gameObject;
			bulletObj.transform.position = _pos;
			bulletObj.transform.rotation = Quaternion.LookRotation( _forward );
		}
		else 
		{
			if ( Network.peerType != NetworkPeerType.Disconnected )
			{
				bulletObj = Network.Instantiate( desc.prefab, _pos, Quaternion.LookRotation( _forward ), 0) as GameObject;
				if ( bulletObj.networkView == null )
				{
					Debug.LogError( "Smart bullets require a network view to function correctly", bulletObj );
				}
			}
			else
			{
				bulletObj = GameObject.Instantiate( desc.prefab, _pos, Quaternion.LookRotation( _forward ) ) as GameObject;
			}
			bulletScript = bulletObj.GetComponent<BulletBase>();
		}
		            
		bulletScript.Reset();
		bulletScript.state = BulletBase.BULLET_STATE.ACTIVE_OWNED;
		bulletScript.enabled = true;

		bulletObj.SetActive( true );
		bulletObj.collider.enabled = true;

		bulletScript.source = _source;// TODO: Crap, just realised this ain't gonna fly when networked. Will have to set up target manager
		Physics.IgnoreCollision( bulletObj.collider, _source.collider );
		  
		if ( _spread != 0.0f )
		{
			Vector3 perp = new Vector3( UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f );
			perp.Normalize();
			perp = bulletObj.transform.TransformDirection( perp );
			bulletObj.transform.Rotate( perp, UnityEngine.Random.Range( -_spread, _spread ) );
		}

		if ( desc.smartBullet == false && Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendShootBulletMessage( _bulletType, bulletScript.index, _pos, bulletObj.transform.rotation );//bulletObj.transform.forward );
		}

		bulletScript.OnShoot();
		_source.OnBulletCreated( bulletScript );
		return bulletScript;
	}

	/// <summary>
	/// Called by the network manager to create a dumb local bullet, until it is destroyed by a similar call
	/// </summary>
	/// <param name="_ownerID">The owner of the bullet</param>
	/// <param name="_creationTime">_creation time.</param>
	/// <param name="_bulletType">_bullet type.</param>
	/// <param name="_index">_index.</param>
	/// <param name="_position">_position.</param>
	/// <param name="_forward">The forward direction of the bullet</param>
	public void CreateBulletRPC( int _ownerID, float _creationTime, BULLET_TYPE _bulletType, int _index, Vector3 _position, Quaternion _rot )//Vector3 _forward )
	{  
		BulletBase bulletScript = this.bulletDictionary[_bulletType].GetAvailableBullet( _index, _ownerID );

		bulletScript.Reset();
		bulletScript.state = BulletBase.BULLET_STATE.ACTIVE_NOT_OWNED;
		
		GameObject bulletObj = bulletScript.gameObject;
		bulletObj.SetActive( true );
		bulletObj.transform.position = _position;
		bulletObj.transform.rotation = _rot;//Quaternion.LookRotation( _forward );
		 
		bulletScript.enabled = false;

		if ( bulletScript.desc.smartBullet == false )
		{
			bulletObj.collider.enabled = false;

			float delta = (float)Network.time - _creationTime;
			Vector3 offset = bulletObj.transform.forward * bulletScript.desc.moveSpeed * delta;
			bulletObj.transform.Translate( offset );
		}
		bulletScript.OnShoot();
	}

	public void DestroyLocalBullet( BulletBase _bullet )
	{
		if ( _bullet.desc.smartBullet == true )
		{
			if ( Network.peerType != NetworkPeerType.Disconnected )
			{
				if ( _bullet.networkView.isMine )
				{
					Network.Destroy( _bullet.gameObject );
				}
				else
				{
					//TODO: Tell the owner to nuke this bullet
				}
			}
			else
			{
				GameObject.Destroy( _bullet.gameObject );
			}
		}
		else
		{
			if ( _bullet.state == BulletBase.BULLET_STATE.INACTIVE
			  || _bullet.gameObject.activeSelf == false )
			{
				Debug.LogWarning( "Attempting to disable non active bullet", _bullet );
				return;
			}

			BulletBase bulletScript = this.bulletDictionary[_bullet.bulletType].GetAvailableBullet( _bullet.index, -1 );
			bulletScript.gameObject.SetActive( false );
			bulletScript.state = BulletBase.BULLET_STATE.INACTIVE;

			if ( _bullet.state == BulletBase.BULLET_STATE.ACTIVE_OWNED )
			{
				if ( Network.peerType != NetworkPeerType.Disconnected )
				{
					GameNetworkManager.instance.SendDestroyDumbBulletMessage( _bullet.bulletType, _bullet.index );
				}
			}
			else if ( _bullet.state == BulletBase.BULLET_STATE.ACTIVE_NOT_OWNED )
			{
				if ( Network.peerType != NetworkPeerType.Disconnected )
				{
					GameNetworkManager.instance.SendDestroySmartBulletMessage( bulletScript.parentBucket.ownerID, _bullet.bulletType, _bullet.index );
				}
			}
		}
	}
	
	public void DestroySmartBulletRPC( int _ownerID, BULLET_TYPE _bulletType, int _index )
	{
		BulletBase bulletScript = this.bulletDictionary[_bulletType].GetAvailableBullet( _index, _ownerID );
		bulletScript.gameObject.SetActive( false );
		bulletScript.state = BulletBase.BULLET_STATE.INACTIVE;
		Debug.Log( "Smart bullet destroyed" );
	} 

	public void DestroyDumbBulletRPC( BULLET_TYPE _bulletType, int _index )
	{
		BulletBase bulletScript = this.bulletDictionary[_bulletType].GetAvailableBullet( _index, -1 );
		bulletScript.gameObject.SetActive( false );
		bulletScript.state = BulletBase.BULLET_STATE.INACTIVE;
		Debug.Log( "Dumb bullet destroyed" );
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

		LocalBulletBucket bulletBucket = bucketObj.AddComponent<LocalBulletBucket>();
		bulletBucket.Initialise( _desc, false );
		this.bulletDictionary.Add ( _desc.bulletType, bulletBucket );

	}
}
