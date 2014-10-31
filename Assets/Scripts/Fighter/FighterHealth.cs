using UnityEngine;
using System.Collections;

public class FighterHealth : BaseHealth
{
	public FighterMaster masterScript;
	public int baseHealth = 300;
	public int baseShield = 300;

	protected override void Update()
	{
		if ( this.currentHealth <= 0
		  && this.masterScript.state != FighterMaster.FIGHTERSTATE.DEAD
		  && this.masterScript.state != FighterMaster.FIGHTERSTATE.OUT_OF_CONTROL )
		{
			this.currentHealth = 0;
			if ( Network.peerType == NetworkPeerType.Disconnected || this.networkView.isMine )
			{
				this.masterScript.OnLethalDamage();
			}
		}
		else if ( this.networkView.isMine )
		{
			this.RegenerateShields();
		}
	}
}
