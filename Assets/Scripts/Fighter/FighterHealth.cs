using UnityEngine;
using System.Collections;

public class FighterHealth : BaseHealth
{
	public FighterMaster masterScript;

	public override void Update()
	{
		if ( Network.peerType == NetworkPeerType.Disconnected
		    || this.networkView.isMine )
		{
			if ( this.currentHealth <= 0
			  && this.masterScript.state != FighterMaster.FIGHTERSTATE.DEAD
			  && this.masterScript.state != FighterMaster.FIGHTERSTATE.OUT_OF_CONTROL )
			{
				this.currentHealth = 0;
				this.masterScript.OnLethalDamage();
			}
			else
			{
				this.RegenerateShields();
			}
		}
	}

	/*protected int DetermineExplosion()
	{
		float leftoverDamage = this.currentHealth * -1;
		int deathType = 0; //0 is the default death, 1 is if they really got wrecked
		
		if(leftoverDamage > (this.maxHealth / 2))
		{
			deathType = 1;
		}

		return deathType;
	}*/
}
