using UnityEngine;
using System.Collections;

/// <summary>
/// The base descriptor for any burst fire style weapon
/// </summary>
public class BurstFireWeaponDesc : WeaponDescriptor
{
	/// <summary>
	/// The time it takes to reload a burst
	///    (fire rate determines the time between each shot in a burst)
	/// </summary>
	public float timeBetweenBursts;

	/// <summary>
	/// The number of bullets fired in a single burst
	/// </summary>
	public int shotsInBurst;
}

