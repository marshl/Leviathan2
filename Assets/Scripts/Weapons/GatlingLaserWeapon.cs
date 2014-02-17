using UnityEngine;
using System.Collections;

//[WeaponTypeAttribute( typeof(GatlingLaserWeaponDesc) )]
public class GatlingLaserWeapon : SpinUpWeapon
{
	[HideInInspector]
	public GatlingLaserWeaponDesc gatlingLaserDesc;
}
