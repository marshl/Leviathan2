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
{ // Yes, I know this looks obvious, but I'm ensuring the binary math works
	NEUTRAL = 0,
	TEAM_1 = 1,
	TEAM_2 = 2,
}

[System.Serializable]
public class GamePlayer
{
	public int id;
	
	public string name = "DEFAULT";
	public PLAYER_TYPE playerType = PLAYER_TYPE.UNDEFINED;
	public FIGHTER_TYPE fighterType = FIGHTER_TYPE.NONE;
	public bool isConnected;

	public int kills;
	public int assists; // This one could be tricky to implement
	public int deaths;
	
	public TEAM team;

	public int personalScore; // Only used for display purposes

	public FighterMaster fighter;
	public CapitalShipMaster capitalShip;

	public NetworkPlayer networkPlayer;

	public float GetKillDeathRatio()
	{
		return deaths == 0 ? kills : kills / deaths;
	} 
}
