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

	private GamePlayer lastHitBy;
	public GamePlayer LastHitBy { get { return lastHitBy; } set { lastHitBy = value; } }

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
		if ( _damage <= 0.0f )
		{
			throw new System.ArgumentException( "Damage can't be less than zero! Damage:"
			              + _damage + " broadcast:" + _broadcast + " source:" + _sourcePlayer.id );
		}

		this.LastHitBy = _sourcePlayer;

		if ( this.isIndestructible == true )
		{
			return;
		}

		// Tell the owner about that damage, and let him tell me the health
		if ( _broadcast && Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendDealDamageMessage( this.GetComponent<NetworkView>().viewID, _damage, _sourcePlayer );
		}

		// If it's mine, take the damage
		if ( this.GetComponent<NetworkView>().isMine || Network.peerType == NetworkPeerType.Disconnected )
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
				GameNetworkManager.instance.SendSetHealthMessage( this.GetComponent<NetworkView>().viewID, this.currentHealth, this.currentShield );
			}
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
					GameNetworkManager.instance.SendSetHealthMessage( this.GetComponent<NetworkView>().viewID, this.currentHealth, this.currentShield );
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
			GameNetworkManager.instance.SendSetHealthMessage( this.GetComponent<NetworkView>().viewID, this.currentHealth, this.currentShield );
		}
	}
}
