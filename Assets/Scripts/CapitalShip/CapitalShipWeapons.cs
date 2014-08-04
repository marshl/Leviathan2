using UnityEngine;
using System.Collections;

public class CapitalShipWeapons : BaseWeaponManager
{
	public WeaponBase weapon;

	private void Update()
	{
		if ( (this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected)
		    && Input.GetKeyDown( KeyCode.Space ) ) 
		{
			this.weapon.SendFireMessage();
		}
	}

	public override void UpdateTargetList ()
	{
		throw new System.NotImplementedException ();
	}
}
