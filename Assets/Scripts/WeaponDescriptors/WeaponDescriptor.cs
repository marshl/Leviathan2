using UnityEngine;
using System.Collections;
using System;

public abstract class WeaponDescriptor : MonoBehaviour
{
	public BulletDescriptor bulletDesc;

	public float fireRate;
	public float spread;
	public bool fireInSync;
}

public class WeaponTypeAttribute : Attribute
{
	public System.Type type;

	public WeaponTypeAttribute( System.Type _type )
	{
		this.type = _type;
	}
}
