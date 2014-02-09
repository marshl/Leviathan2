using UnityEngine;
using System.Collections;

/// <summary>
/// The Bullet Bucket objects are created by the Bullet Manager object 
/// </summary>
public class BulletBucket : MonoBehaviour
{
	public BulletDescriptor bulletDesc;
	public BulletBase[] bulletList;

	private int currentIndex = 0;

	public void CreateBulletList()
	{
		if ( this.bulletDesc.count <= 0 )
		{
			Debug.LogError( "Bullet list for " + this.bulletDesc + " has unusable count of " + this.bulletDesc.count, this.bulletDesc );
		}
		this.bulletList = new BulletBase[ this.bulletDesc.count ];
		for ( int i = 0; i < this.bulletDesc.count; ++i )
		{
			BulletBase bulletScript = this.CreateNewBullet( this.bulletDesc.name + i.ToString() );
			this.bulletList[i] = bulletScript;
		}
	}
	
	private BulletBase CreateNewBullet( string _name )
	{
		GameObject bulletObj = GameObject.Instantiate( this.bulletDesc.prefab ) as GameObject;
		bulletObj.name = _name;
		bulletObj.transform.parent = this.transform;
		bulletObj.SetActive( false );
		
		BulletBase bulletScript = bulletObj.GetComponent<BulletBase>();
		bulletScript.desc = this.bulletDesc;
		return bulletScript;
	}

	/// <summary>
	/// Gets the first available bullet, increasing the size of the list if there is not one available
	/// </summary>
	/// <returns>The script of the first available bullet</returns>
	public BulletBase GetAvailableBullet()
	{
		int startIndex = currentIndex;
		while ( true )
		{
			++currentIndex;
			// If we go over the end of the list, wrap back around
			if ( currentIndex >= this.bulletList.Length )
			{
				currentIndex = 0;
			}

			if ( this.bulletList[currentIndex].gameObject.activeSelf == false )
			{
				return this.bulletList[currentIndex];
			}

			// If we've wrapped the search, no bullets are free: incrase the bucket size and return a new bullet
			if ( currentIndex == startIndex )
			{
				Debug.LogWarning( "Bucket size " + this.bulletList.Length + " for " + this.bulletDesc.name + " was not adequate: increasing bucket size." );
				if ( this.IncreaseBucketSize() == true )
				{
					return this.GetAvailableBullet();
				}
				else // We don't want an infinite loop
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
	private bool IncreaseBucketSize()
	{
		int oldLength = this.bulletList.Length;

		if ( oldLength == 0 )
		{
			Debug.LogError( "Cannot double the size of an empty list" );
			return false;
		}
		int newLength = oldLength * 2;
		BulletBase[] newList = new BulletBase[ newLength ];
		this.bulletList.CopyTo( newList, 0 );

		for ( int i = oldLength; i < newLength; ++i )
		{
			BulletBase bulletScript = this.CreateNewBullet( this.bulletDesc.name + i.ToString() );
			newList[i] = bulletScript;
		}
		this.bulletList = newList;
		this.currentIndex = oldLength;
		return true;
	}
}
