using UnityEngine;
using System.Collections;

public class CapitalShipSubHealth : BaseHealth
{
	public CapitalHealth mainHealth;
	public float damageMultiplier = 1.0f;


	protected override void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		// Do nothing
	}

	public override void DealDamage( float _damage, bool _broadcast )
	{
		this.mainHealth.DealDamage( _damage * this.damageMultiplier, _broadcast );
	}
}
