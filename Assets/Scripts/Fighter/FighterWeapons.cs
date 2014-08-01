﻿using UnityEngine;
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
	}

	private void Update()
	{
		if( ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected )
		 && this.master.state == FighterMaster.FIGHTERSTATE.FLYING )
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

			if ( Input.GetKey( KeyCode.T ) )
			{
				this.SwitchToCentreTarget( this.transform.forward, false );
			}

			if ( Input.GetKeyDown( KeyCode.LeftBracket ) )
			{
				this.SwitchToPreviousTarget();
			}
			else if ( Input.GetKeyDown( KeyCode.RightBracket ) )
			{
				this.SwitchToNextTarget();
			}

			if ( this.currentTarget == null )
			{
				this.SwitchToCentreTarget( this.transform.forward, false );
			}
		}
	}

	public void OnRespawn()
	{
		this.currentTarget = null;
		this.targetList.Clear();
	}
}
