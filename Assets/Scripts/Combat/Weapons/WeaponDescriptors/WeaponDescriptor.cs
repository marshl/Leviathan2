﻿using UnityEngine;
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

	public bool usesAmmunition = false;
	public int ammunitionMax;

	public bool requiresWeaponLock = false;
	public float lockOnDuration;
	public float lockOnAngle;

	public GameObject muzzleFlashPrefab;

	public BulletDescriptor bulletDesc;
}
