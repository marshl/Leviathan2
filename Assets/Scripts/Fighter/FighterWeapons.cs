using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FighterWeapons : BaseWeaponManager
{
	public WeaponBase laserWeapon;
	public WeaponBase missileWeapon;
	public ChargeUpWeapon chargeUpWeapon;


	public float maxTargetDistance;
	public float maxTargetAngle;

	protected override void Awake()
	{
		base.Awake();
	}

	private void Update()
	{
		if( this.networkView.isMine )
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
		}
		this.possibleTargets.Clear();

		//TODO: Determine team
		GamePlayer player = GamePlayerManager.instance.GetNetworkPlayer( this.networkView.owner );
		TargetManager.instance.GetTargetsFromPlayer
		( ref this.possibleTargets, 
			 this.transform,
			 this.maxTargetAngle,
			 this.maxTargetDistance,
			 Common.OpposingTeam( player.team ) );
	}
}
