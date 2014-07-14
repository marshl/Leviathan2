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
		if( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected )
		{
			if ( Input.GetMouseButton( 1 ) ) // Right click - Fire main weapons
			{
			//	Debug.Log ("Firing");
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
		
			this.targets.Clear();

			GamePlayer player = GamePlayerManager.instance.GetPlayerWithID( Common.MyNetworkID() );
			TargetManager.instance.GetTargetsFromPlayer( 
			     ref this.targets, 
				 this.transform,
				 this.maxTargetAngle,
				 this.maxTargetDistance,
				 Common.OpposingTeam( player.team ) );
		}
	}
}
