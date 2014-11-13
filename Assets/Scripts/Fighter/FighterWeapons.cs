using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FighterWeapons : BaseWeaponManager
{
	public FighterMaster master;

	public WeaponBase primaryWeapon;
	public WeaponBase secondaryWeapon;

	public bool usingPrimary = true;

	public WeaponBase tertiaryWeapon;

	protected override void Awake()
	{
		base.Awake();
		this.restrictions.types = (int)( TARGET_TYPE.FIGHTER
		                               | TARGET_TYPE.MISSILE
		                               | TARGET_TYPE.CAPITAL_SHIP_PRIMARY
		                               | TARGET_TYPE.TURRET );
		this.restrictions.maxDistance = -1.0f;
	}

	private void Update()
	{
#if UNITY_EDITOR
		if ( this.master.isDummyShip == false )
#endif
		if ( ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected )
		 && this.master.state == FighterMaster.FIGHTERSTATE.FLYING )
		{
			if ( TargetManager.instance.IsValidTarget( this.currentTarget, this ) == false )
			{
				this.currentTarget = null;
			}

			if ( Input.GetKeyDown( KeyCode.LeftShift ) )
			{
				//TODO: Play switch sound etc
				this.usingPrimary = !usingPrimary;
			}

			if ( Input.GetMouseButton( 1 ) ) // Right click - Fire main weapons
			{
				WeaponBase currentWeapon = this.usingPrimary ? this.primaryWeapon : this.secondaryWeapon;

				currentWeapon.FocusFirePoints( Common.MousePointHitDirection( this.gameObject ) );
				currentWeapon.SendFireMessage();
			}

			if ( this.tertiaryWeapon.weaponDesc != null )
			{
				if ( this.tertiaryWeapon.weaponDesc.requiresWeaponLock )
				{
					if ( Input.GetKey( KeyCode.Space ) )
					{
						if ( this.tertiaryWeapon.CanTrackTarget() )
						{
							this.tertiaryWeapon.currentLockOn += Time.deltaTime;
						}
						else
						{
							if ( this.tertiaryWeapon.currentLockOn != 0.0f )
							{
								//TODO: Play lock fail sound
							}
							this.tertiaryWeapon.currentLockOn = 0.0f;
						}
					}
					else if ( Input.GetKeyUp( KeyCode.Space ) )
					{
						this.tertiaryWeapon.SendFireMessage();
						this.tertiaryWeapon.currentLockOn = 0.0f;
					}
				}
				else
				{
					if ( Input.GetKey ( KeyCode.Space ) )
					{
						this.tertiaryWeapon.SendFireMessage();
					}
				}
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
				this.primaryWeapon.SetWeaponType( WEAPON_TYPE.FIGHTER_LIGHT_LASER_1 );
			}
			else if ( Input.GetKeyDown( KeyCode.Alpha2 ) )
			{
				this.primaryWeapon.SetWeaponType( WEAPON_TYPE.FIGHTER_LIGHT_LASER_2 );
			}
			else if ( Input.GetKeyDown( KeyCode.Alpha3 ) )
			{
				this.primaryWeapon.SetWeaponType( WEAPON_TYPE.FIGHTER_LIGHT_LASER_3 );
			}
		}
	}

	public void OnRespawn()
	{
		this.currentTarget = null;
	}
}
