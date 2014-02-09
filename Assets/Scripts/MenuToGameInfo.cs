using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuToGameInfo : MonoBehaviour
{
	public static MenuToGameInfo instance;

	public Dictionary<int, PLAYER_TYPE> playerTypeMap;


	public void Print()
	{
		Debug.Log( "Printing MenuToGameInfo", this );
		Debug.Log( "int : PLAYER_TYPE" );
		foreach ( KeyValuePair<int, PLAYER_TYPE> pair in this.playerTypeMap )
		{
			Debug.Log( pair.Key + " : " + pair.Value );
		}
	}
}
