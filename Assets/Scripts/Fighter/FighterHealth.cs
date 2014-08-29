using UnityEngine;
using System.Collections;

public class FighterHealth : BaseHealth
{
	public FighterMaster masterScript;

	private void OnGUI()
	{
		if ( this.networkView.isMine )
		{
			GUI.Label( new Rect(0, 50, 150, 50), "Shields: " + this.currentShield + " / " + this.maxShield );
			GUI.Label( new Rect(0, 0, 150, 50), "Hull: " + this.currentHealth + " / " + this.maxHealth );
		}
	} 

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
