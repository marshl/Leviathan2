using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
	public static TargetManager instance;

	public List<FighterHealth> team1Fighters;
	public List<FighterHealth> team2Fighters;

	public Dictionary<NetworkViewID, FighterHealth> fighterIDMap;

	public List<BaseHealth> targets;

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

		this.targets = new List<BaseHealth>( GameObject.FindObjectsOfType<BaseHealth>() ); 
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

	public int GetTargetsFromPlayer( ref List<BaseHealth> _list, Transform _transform, float _angle, float _distance, int _teamNumber = -1 )
	{
		int targetsFound = 0;
		foreach ( BaseHealth target in this.targets )
		{
			Vector3 v = target.transform.position - _transform.position;
			if ( v.sqrMagnitude > _distance * _distance )
			{
				continue;
			}

			float angle = Vector3.Angle( _transform.forward, v.normalized );

			if ( angle >= _angle )
			{
				continue;
			}

			if ( _teamNumber != -1 && target.teamNumber != _teamNumber )
			{
				continue;
			}

			_list.Add( target );
			targetsFound++;
		}
		return targetsFound;
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
