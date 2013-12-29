using UnityEngine;
using System.Collections;

public abstract class SpinUpWeaponDesc : WeaponDescriptor
{
	public float spinUpTime;
	public float spinDownTime;
	public float spinDownDelay;

	public float maxFireRate;
}

