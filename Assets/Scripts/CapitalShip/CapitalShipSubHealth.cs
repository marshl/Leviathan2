using UnityEngine;
using System.Collections;

public class CapitalShipSubHealth : BaseHealth
{
	public CapitalHealth mainHealth;
	public float damageMultiplier = 1.0f;

#if UNITY_EDITOR
	protected override void Start()
	{

	}
#endif

	protected override void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		// Do nothing
	}

	public override void DealDamage( float _damage, bool _broadcast, GamePlayer _sourcePlayer )
	{
		this.mainHealth.DealDamage( _damage * this.damageMultiplier, _broadcast, _sourcePlayer );
	}
}
