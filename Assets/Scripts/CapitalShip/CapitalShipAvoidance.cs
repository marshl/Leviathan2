using UnityEngine;
using System.Collections;

/// <summary>
/// To prevent the capital ships from colliding, the CapitalShipAvoidance collider is a box stretching
/// out the front of the CS that tells the movement script to begin avoidance when it collides
/// with the the avoidance collider of the other CS
/// </summary>
public class CapitalShipAvoidance : MonoBehaviour
{
	public CapitalShipMovement movementScript;

	private void OnTriggerEnter( Collider _other )
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
