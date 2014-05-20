using UnityEngine;
using System.Collections;

public enum PLAYER_TYPE : int
{
	UNDEFINED,
	COMMANDER1,
	COMMANDER2,
	FIGHTER1,
	FIGHTER2,
};

public enum TEAM : int
{
	NEUTRAL,
	TEAM_1,
	TEAM_2,
}

[System.Serializable]
public class GamePlayer
{
	public int id;
	
	public string name = "DEFAULT";
	public PLAYER_TYPE playerType;

	public bool isConnected;

	public int kills;
	public int assists; // This one could be tricky to implement
	public int deaths;
	
	public TEAM team;

	public Fighter fighter;
	public CapitalShipMaster capitalShip;

	public float GetKillDeathRatio()
	{
		return deaths == 0 ? kills : kills / deaths;
	} 
}
