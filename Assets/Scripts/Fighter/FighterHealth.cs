using UnityEngine;
using System.Collections;

public class FighterHealth : BaseHealth
{
	protected override void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		base.OnNetworkInstantiate( _info );

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
