using UnityEngine;
using System.Collections;

/// <summary>
/// The base descriptor for weapons that have to "spin up",
///    firing slowly at first but building up to a certain speed
/// </summary>
public class SpinUpWeaponDesc : WeaponDescriptor
{
	/// <summary>
	/// The time it takes for the weapon to reach maximum fire rate
	///   (while holding down the fire button)
	/// </summary>
	public float spinUpTime;

	/// <summary>
	/// The time it takes for the weapon to go back to normal fire rate
	/// </summary>
	public float spinDownTime;

	/// <summary>
	/// The time it takes before the weapon begins to "spin down"
	/// </summary>
	public float spinDownDelay;

	/// <summary>
	/// The maximum fire rate when the weapon is fully "spun up"
	/// (the base fire rate is used as the lowest fire rate)
	/// </summary>
	public float maxFireRate;
}

