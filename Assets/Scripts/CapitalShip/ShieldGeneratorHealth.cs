using UnityEngine;
using System.Collections;

public class ShieldGeneratorHealth : BaseHealth {

	public float explosionRadius = 250;
	public float explosionDamage = 150;

	bool kaboom = false;

	public CapitalShipMaster capitalShip;

	public override void Update()
	{
		if ( this.currentHealth <= 0 && kaboom == false )
		{
			this.currentHealth = 0;
			Explode();
			kaboom = true;
			Debug.Log ("Kaboom!");
		}
		else
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
          this.capitalShip.health.owner.team, 
          this.capitalShip.health.owner );

		GameNetworkManager.instance.SendDeadShieldMessage( this.networkView.viewID );
	}

	public void ShieldDestroyedNetwork()
	{
		//Play explosion, decrement shield generator count then remove self

		//TODO: explosion

		this.capitalShip.health.shieldGenerators -= 1;
		this.capitalShip.health.RecalculateMaxShields();

#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			TargetManager.instance.RemoveDebugTarget( this.debugTargetID );
		}
#endif

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			TargetManager.instance.RemoveTarget( this.networkView.viewID );
		}
		Destroy( this.gameObject );
	}
}
