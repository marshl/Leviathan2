using UnityEngine;
using System.Collections;

public class DockingBay : MonoBehaviour
{
	public CapitalShipMaster capitalShip;

	[System.Serializable]
	public class DockingSlot
	{
		public Transform landedPosition;
		public FighterMaster landedFighter;
		public int slotID;
	}

	public DockingSlot[] slots = new DockingSlot[4];
	
	private void Start()
	{
		for ( int i = 0; i < this.slots.Length; ++i )
		{
			slots[i].slotID = i;
		}
	}

	private void OnTriggerEnter( Collider _other )
	{
		FighterMaster fighterScript = _other.GetComponent<FighterMaster>();
		if ( fighterScript != null
		  && fighterScript.enabled
	      && fighterScript.state == FighterMaster.FIGHTERSTATE.FLYING
	       )
		{
			DebugConsole.Log( "Fighter received", _other );

			//If we're on the same team
			if ( fighterScript.health.Owner.team == this.capitalShip.health.Owner.team ) 
			{
				this.FriendlyDockingProcedure( fighterScript ); //You may dock
			}
			else
			{
				this.EnemyDockingProcedure( fighterScript ); //Do bad stuff to them.
			}
		}
	}

	private void OnTriggerExit( Collider _other )
	{
		FighterMaster fighterScript = _other.GetComponent<FighterMaster>();
		if ( fighterScript != null )
		{
			if ( fighterScript.state == FighterMaster.FIGHTERSTATE.UNDOCKING )
			{
				fighterScript.ExitDock();
			}
		}
	}

	private void FriendlyDockingProcedure( FighterMaster _fighter )
	{
		DockingBay.DockingSlot slotToDock = this.GetFreeSlot();

		_fighter.Dock( slotToDock );
		slotToDock.landedFighter = _fighter;
	}

	public DockingSlot GetFreeSlot()
	{
		foreach ( DockingSlot friendlySlot in slots )
		{
			if ( friendlySlot.landedFighter == null)
			{
				return friendlySlot;
			}
		}

		return null;
	}

	private void EnemyDockingProcedure( FighterMaster _fighter )
	{
		_fighter.health.DealDamage( _fighter.health.currentHealth + _fighter.health.currentShield,
		                           true,
		                           this.capitalShip.health.Owner );
	}

	public void DockFighter( GameObject _fighterObj )
	{
		DockingSlot slot = this.GetFreeSlot();

		_fighterObj.transform.position = slot.landedPosition.transform.position;
		_fighterObj.transform.parent = slot.landedPosition;
		_fighterObj.transform.rotation = slot.landedPosition.transform.rotation;
		_fighterObj.GetComponent<NetworkPositionControl>().SetUpdatePosition( false );
		
		slot.landedFighter = _fighterObj.GetComponent<FighterMaster>();
		
		DebugConsole.Log( "Received docked RPC", _fighterObj );
	}
}
