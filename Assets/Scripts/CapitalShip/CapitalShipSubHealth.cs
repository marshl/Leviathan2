using UnityEngine;
using System.Collections;

public class CapitalShipSubHealth : BaseHealth
{
	public CapitalHealth mainHealth;
	public float damageMultiplier = 1.0f;

#if UNITY_EDITOR
	protected override void Start()
	{
		// Override base to prevent this from being added to the TargetManager
	}
#endif

	protected override void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		// Override base to prevent this from being added to the TargetManager
	}

	public override void DealDamage( float _damage, bool _broadcast, GamePlayer _sourcePlayer )
	{
		this.mainHealth.DealDamage( _damage * this.damageMultiplier, _broadcast, _sourcePlayer );
	}
}
