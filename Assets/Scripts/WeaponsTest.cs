using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponsTest : MonoBehaviour
{
	public LightLaserWeapon weapon;
	public GatlingLaserWeapon gatlingLaser;

	private void Awake()
	{

	}

	private void Update()
	{
		/*if ( Input.GetKey( KeyCode.Space ) && this.gatlingLaser.CanFire() )
		{
			//Debug.Log ("Bam! " + Time.frameCount );
			//this.weapon.Fire();
			this.gatlingLaser.Fire();
		}*/

		if ( Input.GetKey( KeyCode.Space ) )
		{
			//this.gatlingLaser.SendFireMessage( ref bulletsFired );
			this.GetComponent<BurstFireWeaponTest>().SendFireMessage();
		}
	}
}
