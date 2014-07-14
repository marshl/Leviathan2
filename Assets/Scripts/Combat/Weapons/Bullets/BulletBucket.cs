using UnityEngine;
using System.Collections;

public abstract class BulletBucket : MonoBehaviour
{
	public BulletDescriptor bulletDesc;

	public abstract BulletBase GetAvailableBullet( int _index, int _ownerID ); 
}
