using UnityEngine;
using System.Collections;

public class CapitalShipAvoidance : MonoBehaviour
{
	public CapitalShipMovement movementScript;

	public void OnTriggerEnter( Collider _other )
	{
		CapitalShipAvoidance avoidanceScript = _other.GetComponent<CapitalShipAvoidance>();
		if ( avoidanceScript == null )
		{
			DebugConsole.Warning( "CapitalShipAvoidance has collided with an object that is not an avoidance collider", this );
			DebugConsole.Warning( "CapitalShipAvoidance has collided with an object that is not an avoidance collider", _other );
			return;
		}
		CapitalShipMovement script = _other.transform.parent.GetComponent<CapitalShipMovement>();
		this.movementScript.OnAvoidanceAreaEnter( script );
	}

	public void OnTriggerExit( Collider _collider )
	{
		this.movementScript.OnAvoidanceAreaExit();
	}
}
