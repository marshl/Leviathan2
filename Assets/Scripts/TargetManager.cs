using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
	public static TargetManager instance;

	public List<FighterHealth> team1Fighters;
	public List<FighterHealth> team2Fighters;

	public Dictionary<NetworkViewID, FighterHealth> fighterIDMap;
	 
	private void Awake()
	{
		if ( TargetManager.instance != null )
		{
			Debug.LogError( "Duplicate TargetManagers detected", TargetManager.instance );
		}
		TargetManager.instance = this;

		this.fighterIDMap = new Dictionary<NetworkViewID, FighterHealth>();

		this.team1Fighters = new List<FighterHealth>();
		this.team2Fighters = new List<FighterHealth>();
	}
	   
	public void AddFighter( FighterHealth _target, int _team )
	{
		if ( _target.networkView == null )
		{
			Debug.LogError( "Invalid target " + _target, _target );
			return;
		}
		NetworkViewID id = _target.networkView.viewID;

		if ( this.fighterIDMap.ContainsKey( id ) )
		{
			Debug.LogWarning( "Duplicate target " + _target, _target );
			return;
		}

		this.fighterIDMap.Add( id, _target );

		if ( _team == 1 )
		{
			this.team1Fighters.Add( _target );
		}
		else if ( _team == 2 )
		{
			this.team2Fighters.Add( _target );
		}
		else
		{
			Debug.LogError( "Bad team argument " + _team, _target );
			return;
		}
	}

	public void DealDamageNetwork( NetworkViewID _id, float _damage )
	{
		GameNetworkManager.instance.SendDealDamageMessage( _id, _damage );
	}

	public void OnDealDamage( NetworkViewID _id, float _damage )
	{
		FighterHealth target;
		if ( this.fighterIDMap.TryGetValue( _id, out target ) == false )
		{
			Debug.LogWarning( "Unknown target " + _id );
			return;
		}

		target.DealDamage( _damage );
	}
}
