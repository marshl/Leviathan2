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
	public WEAPON_TYPE weaponType;
	public COLLISION_TYPE collisionType;

	public GameObject prefab;
	public int count;
	
	public float moveSpeed;
	public float maxDistance;

	public float damage;
	public bool areaOfEffect = false;
	public float aoeRadius;
	public bool passesThroughTargets = false;

	public bool smartBullet;

	public float fadeOut = -1.0f;
}

