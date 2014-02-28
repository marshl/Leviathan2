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

	/*private Dictionary<NetworkViewID, BulletBase> networkedBullets;
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

		BulletDescriptor[] descriptors = this.GetComponents<BulletDescriptor>();
		this.bulletDictionary = new Dictionary<BULLET_TYPE, BulletBucket>();

		//this.networkedBullets = new Dictionary<NetworkViewID, BulletBase>();

		foreach ( BulletDescriptor desc in descriptors )
		{
			if ( desc.smartBullet == false )
			{
				this.CreateBulletBucket( desc );
			}
		}
	}
	
	private BulletBase GetBulletOfType( BULLET_TYPE _bulletType ) 
	{
		return this.bulletDictionary[ _bulletType ].GetAvailableBullet( -1, -1 );
	} 
	 
	public BulletBase CreateBullet( BULLET_TYPE _bulletType, 
	                              Vector3 _pos, Vector3 _forward,
	                              float _spread = 0.0f )
	{
		BulletDescriptor desc = BulletDescriptorManager.instance.GetDescOfType( _bulletType );

		BulletBase bulletScript = null;
		GameObject bulletObj = null;
		if ( desc.smartBullet == false )
		{
			bulletScript = this.GetBulletOfType( _bulletType );

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
			//NetworkViewID viewID = bulletObj.networkView.viewID;
			//Debug.Log( "ViewID: " + viewID );

			//this.networkedBullets.Add( viewID, bulletScript );
		}
		            
		bulletScript.Reset();
		bulletScript.state = BulletBase.BULLET_STATE.ACTIVE_OWNED;

		bulletObj.SetActive( true );
		bulletObj.collider.enabled = true;

		if ( _spread != 0.0f )
		{
			Vector3 perp = new Vector3( UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f );
			perp.Normalize();
			perp = bulletObj.transform.TransformDirection( perp );
			bulletObj.transform.Rotate( perp, UnityEngine.Random.Range( -_spread, _spread ) );
		}

		if ( desc.smartBullet == false && Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendShootBulletMessage( _bulletType, bulletScript.index, _pos, bulletObj.transform.forward );
		}

		bulletScript.OnShoot();
		return bulletScript;
	}

	public void CreateBulletRPC( int _ownerID, float _creationTime, BULLET_TYPE _bulletType, int _index, Vector3 _position, Vector3 _forward )
	{
		BulletBase bulletScript = this.bulletDictionary[_bulletType].GetAvailableBullet( _index, _ownerID );

		bulletScript.Reset();
		bulletScript.state = BulletBase.BULLET_STATE.ACTIVE_NOT_OWNED;
		
		GameObject bulletObj = bulletScript.gameObject;
		bulletObj.SetActive( true );
		bulletObj.transform.position = _position;
		bulletObj.transform.rotation = Quaternion.LookRotation( _forward );

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
			//throw new NotImplementedException();
			if ( Network.peerType != NetworkPeerType.Disconnected )
			{
				Network.Destroy( _bullet.gameObject );
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
				Debug.LogError( "Attempting to disable non active bullet", _bullet );
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

		if ( _desc.smartBullet == true )
		{
			NetworkBulletBucket networkBucket = bucketObj.AddComponent<NetworkBulletBucket>();
			networkBucket.Initialise( _desc );
			//this.networkBuckets.Add( _desc.bulletType, networkBucket );
			this.bulletDictionary.Add( _desc.bulletType, networkBucket );
		}
		else
		{
			LocalBulletBucket bulletBucket = bucketObj.AddComponent<LocalBulletBucket>();
			bulletBucket.Initialise( _desc, false );
			this.bulletDictionary.Add ( _desc.bulletType, bulletBucket );
		}
	}
}
