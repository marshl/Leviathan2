using UnityEngine;
using System.Collections;

public class FighterWeapons : MonoBehaviour
{
	public WeaponBase[] weapons;

	public int currentWeaponIndex = 0;

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
		{
			WeaponBase weapon = this.weapons[this.currentWeaponIndex];

			weapon.SendFireMessage();
		}
	}
}
