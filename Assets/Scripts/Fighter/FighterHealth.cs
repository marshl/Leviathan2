using UnityEngine;
using System.Collections;

public class FighterHealth : BaseHealth
{
	protected override void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		//TargetManager.instance.AddFighter( this, 1 );
		base.OnNetworkInstantiate( _info );
		//TargetManager.instance.AddTarget( this.networkView.viewID, this );
		//TODO: Figure out the team  

		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
		}
	}
	
	private void OnGUI()
	{
		GUI.Label( new Rect(0, 50, 150, 50), "Shields: " + this.currentShield + " / " + this.maxShield );
		GUI.Label( new Rect(0, 0, 150, 50), "Hull: " + this.currentHealth + " / " + this.maxHealth );
	} 
}
