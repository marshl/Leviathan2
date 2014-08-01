using UnityEngine;
using System.Collections;

[System.Serializable]
public class BaseHealth : MonoBehaviour 
{
	public TARGET_TYPE targetType;
	public TEAM team;

	public float currentHealth;
	public float maxHealth;
	public float currentShield;
	public float maxShield;
	public float shieldRegen; //Percentage of shields that gets regenerated per second
	public float shieldRegenDelay; // Seconds after being hit for the shield to start regenerating again
	protected float shieldRegenTimer;


	 
	public bool isIndestructible;
#if UNITY_EDITOR
	protected virtual void Start()
	{
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			TargetManager.instance.AddTarget( this.networkView.viewID, this );
		}
	}
#endif

	protected virtual void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		TargetManager.instance.AddTarget( this.networkView.viewID, this );
	}

	public virtual void DealDamage( float _damage, bool _broadcast )
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
				//Shield is a negative number right now, which is the leftover damage from the hit
				this.currentHealth -= (this.currentShield * -1); //Subtracting the sign-changed number for readability
				this.currentShield = 0;
			}
		}
		else
		{
			this.currentHealth -= _damage;
		}

		this.shieldRegenTimer = this.shieldRegenDelay;

		if ( _broadcast
		  && this.networkView != null
		  && Network.peerType != NetworkPeerType.Disconnected )
		{
			TargetManager.instance.DealDamageNetwork( this.networkView.viewID, _damage );
		}

	}

	public virtual void Update()
	{
		if ( this.currentHealth > 0.0f )
		{
			RegenerateShields();
		}
	}

	public virtual void RegenerateShields()
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

	public virtual void FullHeal()
	{
		currentHealth = maxHealth;
		currentShield = maxShield;
	}
}
