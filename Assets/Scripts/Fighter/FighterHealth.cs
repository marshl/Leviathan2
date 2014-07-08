using UnityEngine;
using System.Collections;

public class FighterHealth : BaseHealth
{
	private void OnGUI()
	{
		if ( this.networkView.isMine )
		{
			GUI.Label( new Rect(0, 50, 150, 50), "Shields: " + this.currentShield + " / " + this.maxShield );
			GUI.Label( new Rect(0, 0, 150, 50), "Hull: " + this.currentHealth + " / " + this.maxHealth );
		}
	} 
}
