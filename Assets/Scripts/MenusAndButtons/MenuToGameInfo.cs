using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuToGameInfo : MonoBehaviour
{
	public static MenuToGameInfo instance;

	public PLAYER_TYPE playerType;
	//public Dictionary<int, PLAYER_TYPE> playerTypeMap;

	public void Awake()
	{
		//this.playerTypeMap = new Dictionary<int, PLAYER_TYPE>();
	}

	public void UseDefaults()
	{
		this.playerType = PLAYER_TYPE.COMMANDER1;
		//this.playerTypeMap.Add( -1, PLAYER_TYPE.COMMANDER1 );
	}

	public void Print()
	{
		DebugConsole.Log( "Printing MenuToGameInfo", this );
		DebugConsole.Log( "Player Type: " + this.playerType );
	}
}
