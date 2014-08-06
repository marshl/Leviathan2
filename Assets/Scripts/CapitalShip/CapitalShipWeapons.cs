using UnityEngine;
using System.Collections;

public class CapitalShipWeapons : BaseWeaponManager
{
	public CapitalShipMaster master;
	public WeaponBase weapon;

	private void Update()
	{
#if UNITY_EDITOR
		if ( this.master.dummyShip == false )
#endif
		{
			if ( (this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected)
			    && Input.GetKeyDown( KeyCode.Space ) ) 
			{
				this.weapon.SendFireMessage();
			}
		}
	}
}
