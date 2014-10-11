using UnityEngine;
using System.Collections;

/// <summary>
/// A capital ship shield generator. When it is destroyed, the capital ship gets weaker
/// </summary>
public class ShieldGeneratorHealth : BaseHealth {

	public float explosionRadius = 250;
	public float explosionDamage = 150;

	bool hasExploded = false;

	public CapitalShipMaster capitalShip;

	protected override void Update()
	{
		if ( this.currentHealth <= 0 && this.hasExploded == false )
		{
			this.currentHealth = 0;
			if ( this.networkView.isMine )
			{
				this.Explode();
			}
			this.hasExploded = true;
			Debug.Log( "Shield generator (" + this.gameObject.name + ") has exploded", this.gameObject );
		}
		else if ( this.networkView.isMine )
		{
			this.RegenerateShields();
		}
	}

	public void Explode()
	{
		TargetManager.instance.AreaOfEffectDamage( 
          this.transform.position,
          this.explosionRadius,
          this.explosionDamage,
          false, 
          this.capitalShip.health.Owner );

		this.ShieldDestroyedNetwork();
		GameNetworkManager.instance.SendDeadShieldMessage( this.networkView.viewID );
	}

	public void ShieldDestroyedNetwork()
	{
		this.capitalShip.health.shieldGenerators -= 1;
		this.capitalShip.health.RecalculateMaxShields();

#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			TargetManager.instance.RemoveDebugTarget( this.debugTargetID );
		}
		else
#endif
		{
			TargetManager.instance.RemoveTarget( this.networkView.viewID );
		}
		Destroy( this.gameObject );
	}
}
