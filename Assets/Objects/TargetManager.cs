using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TargetManager : MonoBehaviour {

	public List<PunchingBag> RedFighters;
	public List<PunchingBag> YellowFighters;

	// Use this for initialization
	void Start () {

		InitialiseLists();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void InitialiseLists()
	{
		PunchingBag[] AllFighters = GameObject.FindObjectsOfType(typeof (PunchingBag)) as PunchingBag[];

		foreach (PunchingBag fighters in AllFighters)
		{
			ReadTeam(fighters);
		}
	}

	void ReadTeam(PunchingBag fighter)
	{
		switch(fighter.teamNumber)
		{
		case 0:
			RedFighters.Add (fighter);
			break;
		case 1:
			YellowFighters.Add (fighter);
			break;
		default:
			break;
		};
	}

	public void AddFighter(PunchingBag newFighter)
	{
		ReadTeam(newFighter);
	}

	/*public List<PunchingBag> GetActiveFighters(int askingTeamID)
	{
		switch(askingTeamID)
		{
		case 0:
			return YellowFighters;

		case 1:
			return RedFighters;

		default:
			return YellowFighters;
		}
	}*/
}
