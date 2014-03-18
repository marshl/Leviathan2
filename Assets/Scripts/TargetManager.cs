using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
	public static TargetManager instance;

	public List<FighterHealth> team1Fighters;
	public List<FighterHealth> team2Fighters;
	
	private void Awake()
	{
		if ( TargetManager.instance != null )
		{
			Debug.LogError( "Duplicate TargetManagers detected", TargetManager.instance );
		}
		TargetManager.instance = this;

		this.team1Fighters = new List<FighterHealth>();
		this.team2Fighters = new List<FighterHealth>();
	}
}
