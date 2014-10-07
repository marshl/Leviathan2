using UnityEngine;
using System.Collections;

public class DockingBay : MonoBehaviour
{
	public CapitalShipMaster capitalShip;

	public int maximumShips;
	public int currentShips = 0;

	[System.Serializable]
	public class DockingSlot
	{
		public Transform landedPosition;
		public FighterMaster landedFighter;
		public int slotID;
	}

	public DockingSlot[] slots = new DockingSlot[4];
	
	public int bayID;

	//TODO: Something broke here when networked
	private void Start()
	{
		//int idCounter = ((int)(this.capitalShip.health.Owner.team) * 1000) + (bayID * 10 + 1);
		//foreach ( DockingSlot newSlot in slots )
		for ( int i = 0; i < this.slots.Length; ++i )
		{
			slots[i].slotID = i;
			//newSlot.slotID = idCounter;
			//idCounter++;
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
			DebugConsole.Log( "Fighter received" );

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

	/*public DockingSlot GetSlotByID( int _id )
	{
		foreach ( DockingSlot slot in slots )
		{
			if( slot.slotID == _id)
			{
				return slot;
			}
		}
		DebugConsole.Log("Slot search in bay " + bayID + " for id " + _id + " returned no results");
		return null;
	}*/

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
