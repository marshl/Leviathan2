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
	
	public Dictionary<NetworkViewID, BaseHealth> healthMap;

	private void Awake()
	{
		if ( TargetManager.instance != null )
		{
			Debug.LogError( "Duplicate TargetManagers detected", TargetManager.instance );
		}
		TargetManager.instance = this;

		this.healthMap = new Dictionary<NetworkViewID, BaseHealth>();
	}

	public void AddTarget( NetworkViewID _viewID, BaseHealth _health )
	{
		if ( this.healthMap.ContainsKey( _viewID ) )
		{
			Debug.LogWarning( "Target Manager already contains " + _viewID + "(" + _health.gameObject.name + ")", _health );
			return;
		}

		Debug.Log( "Adding target " + _viewID + " (" + _health.gameObject.name + ") to TargetManager", _health );
		this.healthMap.Add( _viewID, _health );
	}

	public BaseHealth GetTargetWithID( NetworkViewID _viewID )
	{
		BaseHealth target = null;
		if ( this.healthMap.TryGetValue( _viewID, out target ) )
		{
			return target;
		}
		Debug.LogWarning( "Could not find target with ID " + _viewID );
		return null;
	}

	public int GetTargetsFromPlayer( ref List<Target> _list, Transform _transform, float _maxAngle, float _maxDistance, int _teamNumber = -1 )
	{
		int targetsFound = 0;
		foreach ( KeyValuePair<NetworkViewID, BaseHealth> pair in this.healthMap )
		{
			BaseHealth health = pair.Value;
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
			++targetsFound;
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

	public void OnNetworkDamageMessage( NetworkViewID _id, float _damage ) 
	{
		BaseHealth target;
		if ( !this.healthMap.TryGetValue( _id, out target ) )
		{
			Debug.LogError( "Cannot find target with ID " + _id );
			return;
		}
		target.DealDamage( _damage, false );
		Debug.Log( target.gameObject.name + " has been dealt " + _damage, target );
	}

	//TODO: Shift this into its own script on the capital ship LM 07/05/14
	public DockingBay.DockingSlot GetDockingSlotByID( int _id )
	{

		//This is a terrible, quick and dirty method. Grabs every docking bay in the scene,
		//asks each bay if they have a slot of the ID. Each bay will return null if they do not
		//or the bay ID if they do.
		DockingBay[] allBays = GameObject.FindObjectsOfType<DockingBay>();

		foreach( DockingBay bay in allBays )
		{
			DockingBay.DockingSlot slotByID = bay.GetSlotByID ( _id );
			if(slotByID != null)
			{
				return slotByID;
			}
		}

		print("No slot of ID " + _id + " found");
		return null;
	}
}
