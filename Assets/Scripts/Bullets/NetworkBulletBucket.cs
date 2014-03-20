using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Obsolete]
public class NetworkBulletBucket : BulletBucket
{
	public Dictionary<int, LocalBulletBucket> buckets;

	public void Initialise( BulletDescriptor _desc )
	{
		this.bulletDesc = _desc;
		this.buckets = new Dictionary<int, LocalBulletBucket>();

		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			this.CreateBucket( -1 );
		}
		else
		{
			this.CreateBucket( Common.NetworkID() );
		}
	}

	private void CreateBucket( int _playerID )
	{
		GameObject obj = new GameObject();
		obj.name = this.bulletDesc.prefab.name + "Bucket-" + _playerID;

		obj.transform.parent = this.transform;
		LocalBulletBucket bucket = obj.AddComponent<LocalBulletBucket>();
		bucket.ownerID = _playerID;
		bucket.Initialise( this.bulletDesc, true );
		this.buckets.Add( _playerID, bucket );
	}

	public override BulletBase GetAvailableBullet( int _index, int _ownerID )
	{
		if ( _ownerID == -1 && Network.peerType != NetworkPeerType.Disconnected )
		{
			_ownerID = Common.NetworkID();
		}

		if ( this.buckets.ContainsKey( _ownerID ) == false )
		{
			Debug.LogError( "No player " + _ownerID + " found in Network Bullet Bucket for " + this.bulletDesc.name, this.gameObject );
			return null;
		}
		return this.buckets[ _ownerID ].GetAvailableBullet( _index, -1 );
	}
}
