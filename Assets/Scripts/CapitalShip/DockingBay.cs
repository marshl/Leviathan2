using UnityEngine;
using System.Collections;

public class DockingBay : MonoBehaviour
{
	public CapitalShipMaster capitalShip;

	public int maximumShips = 4;
	public int currentShips = 0;

	[System.Serializable]
	public class DockingSlot
	{
		public Transform landedPosition;
		public FighterMaster landedFighter;
		public bool occupied;
		public int slotID;
	}

	public DockingSlot[] slots = new DockingSlot[4];
	
	public int bayID;

	public Renderer bayRenderer;

	private void Start()
	{
		int idCounter = ((int)(this.capitalShip.health.team) * 1000) + (bayID * 10 + 1);
		foreach ( DockingSlot newSlot in slots )
		{
			newSlot.slotID = idCounter;
			idCounter++;
		}

		if(capitalShip.owner.team == TEAM.TEAM_1)
		{
			bayRenderer.material.color = new Color(0.5f,1.0f,0.5f);
		}
		else
		{
			bayRenderer.material.color = new Color(0.5f, 0.5f, 1.0f);
		}

	}

	private void OnTriggerEnter( Collider _other )
	{
		FighterMaster fighterScript = _other.GetComponent<FighterMaster>();
		if ( fighterScript != null && _other.networkView != null && _other.networkView.isMine )
		{
			if ( fighterScript.health.team == this.capitalShip.health.team
			  && fighterScript.state == FighterMaster.FIGHTERSTATE.FLYING
			  && fighterScript.enabled ) //If we're on the same team
			{
				DebugConsole.Log("Fighter received");
				FriendlyDockingProcedure( fighterScript ); //You may dock
			}
			else
			{
				EnemyDockingProcedure( fighterScript ); //Do bad stuff to them.
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
		slotToDock.occupied = true;
		slotToDock.landedFighter = _fighter;
	}

	public DockingSlot GetSlotByID( int _id )
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
	}

	public DockingSlot GetFreeSlot()
	{
		foreach ( DockingSlot friendlySlot in slots )
		{
			if ( !friendlySlot.occupied )
			{
				return friendlySlot;
			}
		}

		return null;
	}

	private void EnemyDockingProcedure( FighterMaster _fighter )
	{
		//TODO: Damage the fighter etc.
	}
}
