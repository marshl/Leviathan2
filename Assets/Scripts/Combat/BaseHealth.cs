using UnityEngine;
using System.Collections;

public class BaseHealth : MonoBehaviour 
{
	public TARGET_TYPE targetType;
#if UNITY_EDITOR
	public int ownerID = int.MinValue;
#endif
	private GamePlayer owner = null;

	public GamePlayer Owner
	{
		get
		{
			return this.owner;
		}
		set
		{
			this.owner = value;
#if UNITY_EDITOR
			this.ownerID = value != null ? value.id : 0;
#endif
		}
	}

	public int debugTargetID;

	public float currentHealth;
	public float maxHealth;
	public float currentShield;
	public float maxShield;
	public float shieldRegen; //Percentage of shields that gets regenerated per second
	public float shieldRegenDelay; // Seconds after being hit for the shield to start regenerating again
	protected float shieldRegenTimer;

	public bool isIndestructible;

	[HideInInspector]
	public GamePlayer lastHitBy = null;

	public Transform[] guiExtents;

#if UNITY_EDITOR
	protected virtual void Start()
	{
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			TargetManager.instance.AddTarget( this );
		}
	}
#endif

	protected virtual void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		TargetManager.instance.AddTarget( this );
	}

	public virtual void DealDamage( float _damage, bool _broadcast, GamePlayer _sourcePlayer )
	{
		this.lastHitBy = _sourcePlayer;

		if ( this.isIndestructible == true )
		{
			return;
		}

		// If it's mine, take the damage
		if ( Network.peerType == NetworkPeerType.Disconnected || this.networkView.isMine )
		{
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

			if ( Network.peerType != NetworkPeerType.Disconnected )
			{
				GameNetworkManager.instance.SendSetHealthMessage( this.networkView.viewID, this.currentHealth, this.currentShield );
			}
		}
		// Otherwise tell the owner about that damage, and let him tell me the health
		else if ( _broadcast )
		{
			GameNetworkManager.instance.SendDealDamageMessage( this.networkView.viewID, _damage, _sourcePlayer );
		}
	}

	protected virtual void Update()
	{
		if ( this.currentHealth > 0.0f )
		{
			RegenerateShields();
		}
	}

	public virtual void RegenerateShields()
	{
		this.shieldRegenTimer = Mathf.Max( this.shieldRegenTimer - Time.deltaTime, 0.0f );

		if( this.shieldRegenTimer <= 0.0f )
		{
			this.shieldRegenTimer = 0;
			if ( this.currentShield < this.maxShield )
			{
				this.currentShield = Mathf.Min( this.maxShield, this.currentShield + this.maxShield * this.shieldRegen * Time.deltaTime );
			
				//TODO: This might be way too frequent an update
				if ( Network.peerType != NetworkPeerType.Disconnected )
				{
					GameNetworkManager.instance.SendSetHealthMessage( this.networkView.viewID, this.currentHealth, this.currentShield );
				}
			}
		}
	}

	public virtual void FullHeal()
	{
		currentHealth = maxHealth;
		currentShield = maxShield;
		this.shieldRegenTimer = 0.0f;

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendSetHealthMessage( this.networkView.viewID, this.currentHealth, this.currentShield );
		}
	}
}
