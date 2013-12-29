using UnityEngine;
using System.Collections;
using System;

public abstract class BulletDescriptor : MonoBehaviour
{
	private System.Type bulletType;
	public GameObject prefab;
	public int count;
	public float moveSpeed;
	//public float lifeTime;
	public float maxDistance;
	public float spread;

	public System.Type GetBulletType()
	{
		if ( this.bulletType == null )
		{
			this.bulletType = this.prefab.GetComponent<BulletBase>().GetType();
		}
		return this.bulletType;
	}
}

public class BulletTypeAttribute : Attribute
{
	public System.Type bulletType;
	public BulletTypeAttribute( System.Type _bulletType )
	{
		this.bulletType = _bulletType;
	}
}
