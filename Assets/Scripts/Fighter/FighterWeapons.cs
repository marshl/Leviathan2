using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FighterWeapons : MonoBehaviour
{
	public WeaponBase laserWeapon;
	public WeaponBase missileWeapon;
	public ChargeUpWeapon chargeUpWeapon;

	private List<BaseHealth> possibleTargets;

	public float maxTargetDistance;
	public float maxTargetAngle;

	private void Awake()
	{
		this.possibleTargets = new List<BaseHealth>();
	}

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

		if ( Input.GetKey( KeyCode.LeftControl ) )
		{
			this.chargeUpWeapon.SendFireMessage();
		} 

		this.possibleTargets.Clear();

		//TODO: Determine team
		TargetManager.instance.GetTargetsFromPlayer( ref this.possibleTargets, this.transform, this.maxTargetAngle, this.maxTargetDistance, 2 );
	}

	public void OnBulletCreated( BulletBase _bullet )
	{
		SeekingBullet seeking = _bullet as SeekingBullet;
		if ( seeking != null )
		{
			BaseHealth target = this.possibleTargets.Count == 0 ? null : this.possibleTargets[0];
			seeking.target = target;
		}
	}
}
