using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponsTest : MonoBehaviour
{
	private void Update()
	{
		if ( Input.GetKey( KeyCode.Space ) )
		{
			WeaponBase[] weapons = this.GetComponents<WeaponBase>();
			foreach ( WeaponBase weapon in weapons )
			{
				weapon.SendFireMessage();
			}
		}
	}
}
