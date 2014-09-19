using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// The base descriptor for any weapon
/// </summary>
public class WeaponDescriptor : MonoBehaviour
{
	public WEAPON_TYPE weaponType;
	public string label;
	public float fireRate;
	public float energyCost;
	public float spread;
	public bool fireInSync;
	public BulletDescriptor bulletDesc;
}
