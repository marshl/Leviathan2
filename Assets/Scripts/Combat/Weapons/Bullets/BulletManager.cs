using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum WEAPON_TYPE : int
{
	NONE,

	TURRET_SHREDDER,
	TURRET_RIPPER,


	CAPITAL_GAUSS,

	FIGHTER_LIGHT_LASER_1,
	FIGHTER_LIGHT_LASER_2,
	FIGHTER_LIGHT_LASER_3,

	FIGHTER_ENERGY_CHAINGUN_1,
	FIGHTER_PARTICLE_ACCELERATOR_1,
	FIGHTER_ION_GUN_1,
	FIGHTER_HEAVY_LASER_1,
	FIGHTER_PLASMA_CANNON_1,
	FIGHTER_MESON_BLASTER_1,
	FIGHTER_TACHYON_CANNON_1,
	FIGHTER_GUIDED_MISSILE_1,
	TURRET_IONBURST,
	CAPITAL_HELLFIRE,
	CAPITAL_FUSION_BEAM,
	REMOTE_LASER,
	SPECIAL_BARRAGE_MISSILE,
};

public enum COLLISION_TYPE : int
{
	COLLISION_DEFAULT,
	COLLISION_RAY,
	COLLISION_SPHERE,
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
	private Dictionary<WEAPON_TYPE, BulletBucket> bulletDictionary;

	public Dictionary<NetworkViewID, SeekingBullet> seekingBulletMap;

#if UNITY_EDITOR
	// We can't map by NetworkViewID in offline mode, so we'll have to use ints
	public int debugSeekingID = 0;
	public Dictionary<int, SeekingBullet> debugSeekingBulletMap;
#endif

	private void Awake()
	{
		if ( instance != null )
		{
			DebugConsole.Error( "Duplicate instance of Bullet Manager found", instance );
			DebugConsole.Error( "Duplicate instance of Bullet Manager found", this );
		}
		instance = this;

		this.seekingBulletMap = new Dictionary<NetworkViewID, SeekingBullet>();

#if UNITY_EDITOR
		this.debugSeekingBulletMap = new Dictionary<int, SeekingBullet>();
#endif
	}

	private void Start()
	{
		this.bulletDictionary = new Dictionary<WEAPON_TYPE, BulletBucket>();

		foreach ( KeyValuePair<WEAPON_TYPE, BulletDescriptor> pair in BulletDescriptorManager.instance.descMap )
		{
			if ( pair.Value.smartBullet == false )
			{
				this.CreateBulletBucket( pair.Value );
			}
		}
	}

	public BulletBase CreateBullet( WeaponBase _weapon, WeaponFirePoint _firePoint, float _damageScale,
	                               bool _local, float _sentTime )
	{
		BulletDescriptor descriptor = BulletDescriptorManager.instance.GetDescOfType( _weapon.weaponType );

		if ( descriptor.smartBullet && !_local )
		{
			return null;
		}

		BulletBase bulletScript = null;
		GameObject bulletObj = null;
		if ( descriptor.smartBullet == false )
		{
			bulletScript = this.bulletDictionary[ _weapon.weaponType ].GetAvailableBullet( -1, -1 );

			if ( bulletScript == null )
			{
				DebugConsole.Error( "Error shooting bullet of type \"" + _weapon.weaponType + "\"", this );
				return null;
			}

			bulletObj = bulletScript.gameObject;
			bulletObj.transform.position = _firePoint.transform.position;
			bulletObj.transform.rotation = Quaternion.LookRotation( _firePoint.transform.forward );
		
			bulletObj.SetActive( true );
			bulletScript.Reset();
			bulletScript.enabled = true;
		}
		else // Smart bullets
		{
#if UNITY_EDITOR
			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				bulletObj = GameObject.Instantiate(
					descriptor.prefab,
					_firePoint.transform.position,
					Quaternion.LookRotation( _firePoint.transform.forward ) ) as GameObject;
			}
			else
#endif
			{
				bulletObj = Network.Instantiate(
					descriptor.prefab,
					_firePoint.transform.position,
					Quaternion.LookRotation( _firePoint.transform.forward ),
					0) as GameObject;
			}
			bulletScript = bulletObj.GetComponent<BulletBase>();

			BaseHealth bulletHealth = bulletObj.GetComponent<BaseHealth>();
			bulletHealth.Owner = _weapon.source.health.Owner;
		}

		bulletScript.state = BulletBase.BULLET_STATE.ACTIVE_OWNED;
		bulletScript.damageScale = _damageScale;
		bulletScript.source = _weapon.source;
		if ( _weapon.source.collider != null )
		{
			Physics.IgnoreCollision( bulletObj.collider, _weapon.source.collider );
		}

		// Bullet spread
		if ( _weapon.weaponDesc.spread != 0.0f )
		{
			System.Random newRand = new System.Random( (int)( _sentTime * 1000.0f ) );
			Vector3 perp = new Vector3( (float)(newRand.NextDouble() * 2.0f - 1.0f),
			                            (float)(newRand.NextDouble() * 2.0f - 1.0f), 0.0f );
			perp.Normalize();
			perp = bulletObj.transform.TransformDirection( perp );
			bulletObj.transform.Rotate( perp, UnityEngine.Random.Range( -_weapon.weaponDesc.spread, _weapon.weaponDesc.spread ) );
		}

		/*
		// Tell everyone else to fire the bullet
		if ( descriptor.smartBullet == false
		  && Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendShootBulletMessage(
	           _weapon.weaponType,
	           bulletScript.index,
	           _firePoint.transform.position,
	           bulletObj.transform.rotation );
		}
        
*/
		if ( !_local && !descriptor.smartBullet )
		{
			bulletScript.Reset();
			bulletScript.state = BulletBase.BULLET_STATE.ACTIVE_NOT_OWNED;

			bulletObj.collider.enabled = false;
				
			float delta = (float)Network.time - _sentTime;
			Vector3 offset = bulletObj.transform.forward * descriptor.moveSpeed * delta;
			bulletObj.transform.Translate( offset );
		}

		bulletScript.OnShoot();
		_weapon.source.OnBulletCreated( _weapon, bulletScript );


		// Tell everyone else about the owner of this bullet
		if ( descriptor.smartBullet == true
		  && Network.peerType != NetworkPeerType.Disconnected )
		{
			BaseHealth target = bulletObj.GetComponent<SeekingBullet>().target;
			NetworkViewID viewID = target != null ? target.networkView.viewID : NetworkViewID.unassigned;

			GameNetworkManager.instance.SendSetSmartBulletTeamMessage(
                  bulletObj.networkView.viewID,
			      GamePlayerManager.instance.myPlayer.id,
                  viewID );
		}

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
	public void CreateBulletRPC( int _ownerID, float _creationTime, WEAPON_TYPE _weaponType, int _index, Vector3 _position, Quaternion _rot )
	{
		BulletBase bulletScript = this.bulletDictionary[_weaponType].GetAvailableBullet( _index, _ownerID );

		bulletScript.Reset();
		bulletScript.state = BulletBase.BULLET_STATE.ACTIVE_NOT_OWNED;

		GameObject bulletObj = bulletScript.gameObject;
		bulletObj.SetActive( true );
		bulletObj.transform.position = _position;
		bulletObj.transform.rotation = _rot;

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
#if UNITY_EDITOR
			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				if ( _bullet.desc.fadeOut > 0.0f )
				{
					_bullet.OnFadeBegin();
				}
				else
				{
					GameObject.Destroy( _bullet.gameObject );
				}

				TargetManager.instance.RemoveDebugTarget( _bullet.GetComponent<BaseHealth>().debugTargetID );
				this.debugSeekingBulletMap.Remove( ((SeekingBullet)_bullet).debugID );
			}
			else
#endif
			{
				if ( _bullet.networkView.isMine == false )
				{
					Debug.LogWarning( "You cannot destroy a seeking bullet you don't own!", _bullet );
					return;
				}
				this.seekingBulletMap.Remove( _bullet.networkView.viewID );

				TargetManager.instance.RemoveTarget( _bullet.networkView.viewID );
				GameNetworkManager.instance.SendDestroySmartBulletMessage( _bullet.networkView.viewID );

				// If it fades, fade it out and let its own script destroy it
				if ( _bullet.desc.fadeOut > 0.0f )
				{
					_bullet.OnFadeBegin();
				}
				else
				{
					Network.Destroy( _bullet.gameObject );
				}

			}
		}
		else //Dumb bullet
		{
			if ( _bullet.state == BulletBase.BULLET_STATE.INACTIVE
			  || _bullet.gameObject.activeSelf == false )
			{
				DebugConsole.Warning( "No point disabling a currently inactive bullet (" + _bullet.gameObject.name, _bullet );
				return;
			}

			if ( _bullet.desc.fadeOut > 0.0f )
			{
				_bullet.OnFadeBegin();
			}
			else
			{
				_bullet.gameObject.SetActive( false );
				_bullet.state = BulletBase.BULLET_STATE.INACTIVE;
			}

			if ( Network.peerType != NetworkPeerType.Disconnected )
			{
				GameNetworkManager.instance.SendDestroyDumbBulletMessage( _bullet.weaponType, _bullet.index, _bullet.transform.position );
			}
		}
	}

	public void DestroySmartBulletRPC( NetworkViewID _viewID )
	{
		TargetManager.instance.RemoveTarget( _viewID );

		SeekingBullet bullet = null;
		if ( this.seekingBulletMap.TryGetValue( _viewID, out bullet ) )
		{
			this.seekingBulletMap.Remove( _viewID );

			if ( bullet != null && bullet.desc.fadeOut > 0.0f )
			{
				bullet.OnFadeBegin();
				// Bullet will handle its own destruction
			}
		}
		else
		{
			DebugConsole.Error( "Could not find bullet with ID " + _viewID );
		}
	}

	public void OnDisableDumbBulletMessage( WEAPON_TYPE _weaponType, int _index, Vector3 _bulletPosition )
	{
		BulletBase bulletScript = this.bulletDictionary[_weaponType].GetAvailableBullet( _index, -1 );

		if ( bulletScript.desc.fadeOut > 0.0f )
		{
            bulletScript.transform.position = _bulletPosition;
			bulletScript.OnFadeBegin();
		}
		else
		{
			bulletScript.gameObject.SetActive( false );
			bulletScript.state = BulletBase.BULLET_STATE.INACTIVE;
		}
	}

	private void CreateBulletBucket( BulletDescriptor _desc )
	{
		if ( this.bulletDictionary.ContainsKey( _desc.weaponType ) )
		{
			DebugConsole.Error( "Bullet type \"" + _desc.weaponType + "\" already used.", _desc.prefab );
			return;
		}

		GameObject bucketObj = new GameObject();
		bucketObj.name = _desc.weaponType + "Bucket";
		bucketObj.transform.parent = this.transform;

		LocalBulletBucket bulletBucket = bucketObj.AddComponent<LocalBulletBucket>();
		bulletBucket.Initialise( _desc, false );
		this.bulletDictionary.Add ( _desc.weaponType, bulletBucket );
	}

	public BaseHealth GetNextMissileLock( BaseWeaponManager _weaponManager, int _direction )
	{
		List<BaseHealth> missiles  = new List<BaseHealth>();

		foreach ( KeyValuePair<NetworkViewID, SeekingBullet> pair in this.seekingBulletMap )
		{
			if ( pair.Value.target == _weaponManager.health )
			{
				missiles.Add( pair.Value.health );
			}
		}

#if UNITY_EDITOR
		foreach ( KeyValuePair<int, SeekingBullet> pair in this.debugSeekingBulletMap )
		{
			if ( pair.Value.target == _weaponManager.health )
			{
				missiles.Add( pair.Value.health );
			}
		}
#endif

		if ( missiles.Count == 0 )
		{
			return null;
		}

		int index = missiles.IndexOf( _weaponManager.currentTarget );

		if ( index == -1 )
		{
			return missiles[0];
		}
		index += _direction;

		while ( index >= missiles.Count )
		{
			index -= missiles.Count;
		}

		while ( index < 0 )
		{
			index += missiles.Count;
		}

		return missiles[index];
	}
}
