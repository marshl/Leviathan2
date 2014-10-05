using UnityEngine;
using System.Collections;

public class CapitalShipDamageablePart : BaseHealth
{
	public CapitalHealth capitalHealth;

	public override void DealDamage( float _damage, bool _broadcast, GamePlayer _sourcePlayer )
	{
		if ( this.capitalHealth == null )
		{
			DebugConsole.Warning( this + " is not connected to capital health script", this );
			return;
		}
		this.capitalHealth.DealDamage( _damage, _broadcast, _sourcePlayer );
	}
}
