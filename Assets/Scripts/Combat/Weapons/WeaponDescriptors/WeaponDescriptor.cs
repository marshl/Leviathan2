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

	public float spread;
	public bool fireInSync;

	public bool usesEnergy;
	public float energyCostPerShot;


	public BulletDescriptor bulletDesc;
}
