﻿using UnityEngine;
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

	public override void Update()
	{
		base.Update ();
	}

	private void OnGUI()
	{
		GUI.Label( new Rect(0, 50, 150, 50), "Shields: " + this.currentShield + " / " + this.maxShield );
		GUI.Label( new Rect(0, 0, 150, 50), "Hull: " + this.currentHealth + " / " + this.maxHealth );
	} 
}
