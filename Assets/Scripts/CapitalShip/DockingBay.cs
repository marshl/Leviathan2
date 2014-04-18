using UnityEngine;
using System.Collections;



public class DockingBay : MonoBehaviour {

	public int maximumShips = 4;
	public int currentShips = 0;

	[System.Serializable]
	public class DockingSlot
	{
		public Transform landedPosition;
		public Fighter landedFighter;
		public bool occupied;
		public int slotID;
	}

	public DockingSlot[] slots = new DockingSlot[4];

	public int team = 1;
	public int bayID;
	// Use this for initialization
	void Start () {

		int idCounter = (team * 1000) + (bayID * 10 + 1);
		foreach(DockingSlot newSlot in slots)
		{
			newSlot.slotID = idCounter;
			idCounter++;
		}
	

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.GetComponent<Fighter>() != null)
		{
			Fighter fighter = other.GetComponent<Fighter>();
			if(fighter.team == this.team && !fighter.undocking) //If we're on the same team
			{
				FriendlyDockingProcedure(fighter); //You may dock
			}
			else
			{
				EnemyDockingProcedure(fighter); //Do bad stuff to them.
			}

		}
	}

	void FriendlyDockingProcedure(Fighter _fighter)
	{
		foreach(DockingSlot friendlySlot in slots)
		{
			if(!friendlySlot.occupied)
			{
				_fighter.Dock (friendlySlot);
				friendlySlot.occupied = true;
				friendlySlot.landedFighter = _fighter;
				break;
			}
		}
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
		print("Slot search in bay " + bayID + " for id " + _id + " returned no results");
		return null;
	}

	void EnemyDockingProcedure(Fighter _fighter)
	{

	}
}
