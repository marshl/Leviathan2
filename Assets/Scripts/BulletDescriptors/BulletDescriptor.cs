using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// The Bullet Descriptor class is subclassed by each bullet type and each individual bullet
/// These descriptors store information about the bullet in general that does NOT need to be stored
///    on each individual bullet object.
/// </summary>
public class BulletDescriptor : MonoBehaviour
{
	public BULLET_TYPE bulletType;

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

	public bool smartBullet;
}

