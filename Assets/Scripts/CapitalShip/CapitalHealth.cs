﻿using UnityEngine;
using System.Collections;

public class CapitalHealth : BaseHealth 
{
	public int shieldGenerators = 10;
	public float baseShields; //Our base shields, the capital ship gets this * shieldGenerators final max shield

	protected override void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		base.OnNetworkInstantiate( _info );
		
		if ( this.networkView.isMine == false )
		{  
			this.enabled = false;
		}
	}

	public void Awake()
	{
		this.RecalculateMaxShields();
	}

	public override void Update()
	{
		base.Update ();
	}

	public void RecalculateMaxShields()
	{
		this.maxShield = this.baseShields * shieldGenerators;
	}
	
	private void OnGUI()
	{
		GUI.Label( new Rect(0, 50, 150, 50), "Shields: " + this.currentShield + " / " + this.maxShield );
		GUI.Label( new Rect(0, 0, 150, 50), "Hull: " + this.currentHealth + " / " + this.maxHealth );
	} 
}
