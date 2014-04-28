using UnityEngine;
using System.Collections;

public enum PLAYER_TYPE : int
{
	COMMANDER1,
	COMMANDER2,
	FIGHTER1,
	FIGHTER2,
};

public class GamePlayer
{
	public PLAYER_TYPE playerType;

	public bool isConnected;

	public int kills;
	public int assists; // This one could be tricky to implement
	public int deaths;
	
	public Fighter fighter;
	public CapitalShipMovement capitalShip; // TODO: Make a base CapitalShip class

	public float GetKillDeathRatio()
	{
		return deaths == 0 ? kills : kills / deaths;
	} 
}
