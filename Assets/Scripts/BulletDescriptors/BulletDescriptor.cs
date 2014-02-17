using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// The Bullet Descriptor class is an abstract class that is subclassed by each bullet type and each individual bullet
/// These descriptors store information about the bullet in general that does NOT need to be stored
///    on each individual bullet object.
/// WHEN SUBCLASSING, add the attribute [BulletTypeAttribute( typeof( <BULLETCLASS> ) )] 
///    where <BULLETCLASS> is the class of the actual bullet
/// </summary>
public class BulletDescriptor : MonoBehaviour
{
	public BULLET_TYPE bulletType;
	//private System.Type bulletType;
	public GameObject prefab;

	/// <summary>
	/// The number of bullet objects that will be initially created in the scene
	/// </summary>
	public int count;

	/// <summary>
	/// The speed at which the bullet travels 
	/// </summary>
	public float moveSpeed;

	/// <summary>
	/// The point at which the bullet will expire
	/// </summary>
	public float maxDistance;

	/// <summary>
	/// Returns the type of the Bullet Base class on the prefab
	/// </summary>
	/// <returns>The bullet type.</returns>
	/*public BULLET_TYPE GetBulletType()
	{
		if ( this.bulletType == null )
		{
			this.bulletType = this.prefab.GetComponent<BulletBase>().GetType();
		}
		return this.bulletType;
	}*/
}
/*
public class BulletTypeAttribute : Attribute
{
	public System.Type bulletType;
	public BulletTypeAttribute( System.Type _bulletType )
	{
		this.bulletType = _bulletType;
	}
}*/
