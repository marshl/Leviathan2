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
		Debug.Log( "Printing MenuToGameInfo", this );
		/*Debug.Log( "int : PLAYER_TYPE" );
		foreach ( KeyValuePair<int, PLAYER_TYPE> pair in this.playerTypeMap )
		{
			Debug.Log( pair.Key + " : " + pair.Value );
		}*/

		Debug.Log( "Player Type: " + this.playerType );
	}
}
