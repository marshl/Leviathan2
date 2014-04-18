﻿using UnityEngine;
using System.Collections;

public class BaseHealth : MonoBehaviour 
{
	public int teamNumber;

	public float currentHealth;
	public float maxHealth;
	public float currentShield;
	public float maxShield;
	public float shieldRegen; //Percentage of shields that gets regenerated per second
	public float shieldRegenDelay; // Seconds after being hit for the shield to start regenerating again
	protected float shieldRegenTimer;
	
	public bool isIndestructible;

	protected virtual void Start()
	{
		TargetManager.instance.AddTarget( this );
	}

	public virtual void DealDamage( float _damage )
	{

		if ( this.isIndestructible == true )
		{
			return;
		}

		if( this.currentShield > 0 )
		{
			this.currentShield -= _damage;
			if( this.currentShield < 0 ) //We inflict damage to the hull based on how much the shield is below 0
			{
				this.currentHealth += this.currentShield; //Shield is negative by the points that break through
				this.currentShield = 0;
			}
		}
		else
		{
			this.currentHealth -= _damage;
		}

		this.shieldRegenTimer = this.shieldRegenDelay;
	}

	public virtual void Update()
	{
		if( this.shieldRegenTimer <= 0 )
		{
			this.shieldRegenTimer = 0;
			if( this.currentShield < this.maxShield )
			{
				this.currentShield += (this.maxShield * this.shieldRegen * Time.deltaTime);
				if( this.currentShield > this.maxShield )
				{
					this.currentShield = this.maxShield;
				}
			}

		}
		else
		{
			this.shieldRegenTimer -= Time.deltaTime;
		}
	}
}