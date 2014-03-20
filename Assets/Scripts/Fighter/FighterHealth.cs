using UnityEngine;
using System.Collections;

public class FighterHealth : BaseHealth
{
	private void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		TargetManager.instance.AddFighter( this, 1 );
		//TODO: Figure out the team  

		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
		}
	}

	public override void DealDamage( float _damage )
	{
		base.DealDamage( _damage );
	}

	private void OnGUI()
	{
		GUI.Label( new Rect(0, 0, 150, 50), this.currentHealth + "/" + this.maxHealth );
	} 
}
