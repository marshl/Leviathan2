using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
	public struct Target
	{
		public Target( BaseHealth _health, float _angle, float _distance )
		{
			this.health = _health;
			this.angle = _angle;
			this.distance = _distance;
		}

		public BaseHealth health;
		public float angle;
		public float distance;
	};

	public static TargetManager instance;

	public List<FighterHealth> team1Fighters;
	public List<FighterHealth> team2Fighters;

	public Dictionary<NetworkViewID, FighterHealth> fighterIDMap;

	public List<BaseHealth> targetList;

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

		this.targetList = new List<BaseHealth>( GameObject.FindObjectsOfType<BaseHealth>() ); 
	}

	public void AddTarget( BaseHealth _target )
	{
		this.targetList.Add( _target );
		//TODO: Add fighter specific code
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

	public int GetTargetsFromPlayer( ref List<Target> _list, Transform _transform, float _maxAngle, float _maxDistance, int _teamNumber = -1 )
	{
		int targetsFound = 0;
		foreach ( BaseHealth health in this.targetList )
		{
			Vector3 v = health.transform.position - _transform.position;
			float dist = v.magnitude;
			if ( _maxDistance > 0.0f && dist > _maxDistance )
			{
				continue;
			}

			float angle = Vector3.Angle( _transform.forward, v.normalized );

			if ( _maxAngle > 0.0f && angle >= _maxAngle )
			{
				continue;
			}

			if ( _teamNumber != -1 && health.teamNumber != _teamNumber )
			{
				continue;
			}

			Target t = new Target( health, angle, dist );

			_list.Add( t );
			targetsFound++;
		}
		return targetsFound;
	}

	public BaseHealth GetBestTarget( Transform _transform, float _maxAngle, float _maxDistance, int _teamNumber = -1 )
	{
		List<Target> targets = new List<Target>();

		this.GetTargetsFromPlayer( ref targets, _transform, _maxAngle, _maxDistance, _teamNumber );

		if ( targets.Count == 0 )
		{
			return null;
		}

		//TODO: Rather inefficient, better change
		targets.Sort( delegate( Target t1, Target t2)
		{  
			return t1.angle.CompareTo( t2.angle );
		} );

		return targets[0].health;
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
