using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FighterWeapons : BaseWeaponManager
{
	public WeaponBase laserWeapon;
	public WeaponBase missileWeapon;

	public float maxTargetDistance;

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
				this.laserWeapon.FocusFirePoints( Common.MousePointHitDirection( this.gameObject ) );
				this.laserWeapon.SendFireMessage();
			}
			
			if ( Input.GetKey( KeyCode.Space ) ) // Space bar - Fire missile
			{
				this.missileWeapon.SendFireMessage();
			}
		
			this.otherTargets.Clear();

			GamePlayer player = GamePlayerManager.instance.myPlayer;

			TargetManager.instance.GetTargetsFromPlayer( this, this.transform, this.maxTargetDistance, -1, Common.OpposingTeam( player.team ) );
		}
	}

}
