using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FighterWeapons : BaseWeaponManager
{
	public FighterMaster master;

	public WeaponBase laserWeapon;
	public WeaponBase missileWeapon;

	protected override void Awake()
	{
		base.Awake();
		this.restrictions.types = (int)(TARGET_TYPE.FIGHTER
		                               | TARGET_TYPE.MISSILE
		                               | TARGET_TYPE.CAPITAL_SHIP_PRIMARY
		                               | TARGET_TYPE.TURRET);
		this.restrictions.maxDistance = -1.0f;
	}

	private void Update()
	{
#if UNITY_EDITOR
		if ( this.master.dummyShip == false )
#endif
		if ( ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected )
		 && this.master.state == FighterMaster.FIGHTERSTATE.FLYING )
		{
			if ( TargetManager.instance.IsValidTarget( this.currentTarget, this ) == false )
			{
				this.currentTarget = null;
			}

			if ( Input.GetMouseButton( 1 ) ) // Right click - Fire main weapons
			{
				this.laserWeapon.FocusFirePoints( Common.MousePointHitDirection( this.gameObject ) );
				this.laserWeapon.SendFireMessage();
			}
			
			if ( Input.GetKey( KeyCode.Space ) ) // Space bar - Fire missile
			{
				this.missileWeapon.SendFireMessage();
			}

			if ( Input.GetKey( KeyCode.T ) )
			{
				this.currentTarget = TargetManager.instance.GetCentreTarget( this );
			}

			if ( Input.GetKeyDown( KeyCode.LeftBracket ) )
			{
				this.SwitchToPreviousTarget();
			}
			else if ( Input.GetKeyDown( KeyCode.RightBracket ) )
			{
				this.SwitchToNextTarget();
			}

			if ( Input.GetKeyDown( KeyCode.Colon ) )
			{
				this.SwitchToPreviousFriendlyTarget();
			}
			else if ( Input.GetKeyDown( KeyCode.Quote ) )
			{
				this.SwitchToNextFriendlyTarget();
			}

			if ( Input.GetKeyDown( KeyCode.I ) )
			{
				this.SwitchToNextMissileTargettingMe();
			}

			if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
			{
				this.laserWeapon.SetWeaponType( WEAPON_TYPE.FIGHTER_LIGHT_LASER_1 );
			}
			else if ( Input.GetKeyDown( KeyCode.Alpha2 ) )
			{
				this.laserWeapon.SetWeaponType( WEAPON_TYPE.FIGHTER_LIGHT_LASER_2 );
			}
			else if ( Input.GetKeyDown( KeyCode.Alpha3 ) )
			{
				this.laserWeapon.SetWeaponType( WEAPON_TYPE.FIGHTER_LIGHT_LASER_3 );
			}
		}
	}

	public void OnRespawn()
	{
		this.currentTarget = null;
	}
}
