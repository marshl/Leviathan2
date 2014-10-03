using UnityEngine;
using System.Collections;

/// <summary>
/// The Bullet Bucket objects are created by the Bullet Manager object 
/// </summary>
public class LocalBulletBucket : BulletBucket
{
	private BulletBase[] bulletList;
	private int currentIndex = 0;

	public bool networked;

	public void Initialise( BulletDescriptor _desc, bool _networked )
	{
		this.bulletDesc = _desc;
		this.networked = _networked;

		if ( this.bulletDesc.count <= 0 )
		{
			DebugConsole.Error( "Bullet list for " + this.bulletDesc + " has unusable count of " + this.bulletDesc.count, this.bulletDesc );
		}
		this.bulletList = new BulletBase[ this.bulletDesc.count ];
		this.bulletDesc.prefab.GetComponent<BulletBase>().weaponType = this.bulletDesc.weaponType;

		for ( int i = 0; i < this.bulletDesc.count; ++i )
		{
			BulletBase bulletScript = this.CreateNewBullet( i );
			this.bulletList[i] = bulletScript;
		}

		this.currentIndex = Random.Range( 0, this.bulletDesc.count );
	}
	
	private BulletBase CreateNewBullet( int _index )
	{
		//GameObject bulletObj = GameObject.Instantiate( this.bulletDesc.prefab ) as GameObject;
		GameObject bulletObj;
		if ( this.networked )
		{
			bulletObj = Network.Instantiate( this.bulletDesc.prefab, Vector3.zero, Quaternion.identity, 0 ) as GameObject;
		}
		else
		{
			bulletObj = GameObject.Instantiate( this.bulletDesc.prefab ) as GameObject;
		}
		bulletObj.name = this.bulletDesc.weaponType + "-"+ _index ;
		bulletObj.transform.parent = this.transform;
		bulletObj.SetActive( false );
		
		BulletBase bulletScript = bulletObj.GetComponent<BulletBase>();
		bulletScript.index = _index;
		bulletScript.state = BulletBase.BULLET_STATE.INACTIVE;
		bulletScript.parentBucket = this;
		bulletScript.desc = this.bulletDesc;

		return bulletScript;
	}

	/// <summary>
	/// Gets the first available bullet, increasing the size of the list if there is not one available
	/// </summary>
	/// <returns>The script of the first available bullet</returns>
	public override BulletBase GetAvailableBullet( int _index, int _ownerID )
	{
		if ( _index != -1 )
		{
			while ( _index >= this.bulletList.Length )
			{
				if ( this.DoubleBucketSize() == false )
					break;
			}
			return this.bulletList[_index];
		}

		int startIndex = currentIndex;
		while ( true ) // This thing isn't infinite, I promise
		{
			++currentIndex;
			// If we go over the end of the list, wrap back around
			if ( currentIndex >= this.bulletList.Length )
			{
				currentIndex = 0;
			}

			// Aha, a free one, that'll do
			if ( this.bulletList[currentIndex].gameObject.activeSelf == false )
			{
				return this.bulletList[currentIndex];
			}

			// If we've wrapped the search, no bullets are free: increase the bucket size and return a new bullet
			if ( currentIndex == startIndex )
			{
				DebugConsole.Warning( "Bucket size " + this.bulletList.Length + " for "
				     + this.bulletDesc.weaponType + " was inadequate: increasing bucket size." );
				// Lets try again, starting from the first of the new bullets
				if ( this.DoubleBucketSize() == true )
				{
					return this.GetAvailableBullet( -1, -1 );
				}
				else // Doubling didn't work, abort!
				{
					return null;
				}
			}
		}
	}

	/// <summary>
	/// Doubles the size of the bullet contained, creating new bullets to fill out the gap
	/// </summary>
	/// <returns><c>true</c>, if bucket size was increased, <c>false</c> otherwise.</returns>
	private bool DoubleBucketSize()
	{
		int oldLength = this.bulletList.Length;
		// Attempting to double an empty list ain't gonna work so well
		if ( oldLength == 0 )
		{
			DebugConsole.Error( "Cannot double the size of an empty list" );
			return false;
		}
		int newLength = oldLength * 2;
		BulletBase[] newList = new BulletBase[ newLength ];
		this.bulletList.CopyTo( newList, 0 );

		for ( int i = oldLength; i < newLength; ++i )
		{
			BulletBase bulletScript = this.CreateNewBullet( i );
			newList[i] = bulletScript;
		}
		this.bulletList = newList;
		this.currentIndex = oldLength;
		return true;
	}
}
