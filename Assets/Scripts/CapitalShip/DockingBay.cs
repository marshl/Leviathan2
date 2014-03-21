﻿using UnityEngine;
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
	}

	public DockingSlot[] slots = new DockingSlot[4];

	public int team = 1;
	// Use this for initialization
	void Start () {
	

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
				_fighter.desiredSpeed = 0;
				_fighter.rigidbody.velocity = Vector3.zero;
				_fighter.rigidbody.angularVelocity = Vector3.zero;
				_fighter.transform.position = friendlySlot.landedPosition.position;
				_fighter.transform.rotation = friendlySlot.landedPosition.rotation;
				_fighter.currentSlot = friendlySlot;
				_fighter.docked = true;
				_fighter.transform.parent = friendlySlot.landedPosition;

				friendlySlot.occupied = true;
				friendlySlot.landedFighter = _fighter;
				break;
			}
		}
	}

	void EnemyDockingProcedure(Fighter _fighter)
	{

	}
}