using UnityEngine;
using System.Collections;

public class CapitalShipDamageablePart : BaseHealth
{
	public CapitalHealth capitalHealth;

	public override void DealDamage( float _damage, bool _broadcast )
	{
		if ( this.capitalHealth == null )
		{
			Debug.LogWarning( this + " is not connected to capital health script", this );
			return;
		}
		this.capitalHealth.DealDamage( _damage, _broadcast );
	}
}
