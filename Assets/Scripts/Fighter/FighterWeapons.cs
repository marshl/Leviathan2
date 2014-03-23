using UnityEngine;
using System.Collections;

public class FighterWeapons : MonoBehaviour
{
	public WeaponBase laserWeapon;
	public WeaponBase missileWeapon;

	private void Update()
	{
		if ( Input.GetMouseButton( 1 ) ) // Right click - Fire main weapons
		{
			this.laserWeapon.SendFireMessage();
		}
		
		if ( Input.GetKey( KeyCode.Space ) ) // Space bar - Fire missile
		{
			this.missileWeapon.SendFireMessage();
		}
	}
}
