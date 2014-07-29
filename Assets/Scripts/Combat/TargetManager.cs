using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TargetManager : MonoBehaviour
{
	[System.Serializable]
	public class Target
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

#if UNITY_EDITOR
	public List<BaseHealth> targets;
#endif

	private void Awake()
	{
		if ( TargetManager.instance != null )
		{
			DebugConsole.Error( "Duplicate TargetManagers detected", TargetManager.instance );
		}
		TargetManager.instance = this;

		this.healthMap = new Dictionary<NetworkViewID, BaseHealth>();
	}

	public void AddTarget( NetworkViewID _viewID, BaseHealth _health )
	{
		if ( this.healthMap.ContainsKey( _viewID ) )
		{
			DebugConsole.Warning( "Target Manager already contains " + _viewID + "(" + _health.gameObject.name + ")", _health );
			return;
		}
#if UNITY_EDITOR
		this.targets.Add( _health );
#endif
		DebugConsole.Log( "Adding target " + _viewID + " (" + _health.gameObject.name + ") to TargetManager", _health );
		this.healthMap.Add( _viewID, _health );
	}

	public void RemoveTarget(NetworkViewID _viewID)
	{
		if ( this.healthMap.ContainsKey( _viewID ) )
		{
#if UNITY_EDITOR
			this.targets.Remove( this.healthMap[_viewID] );
#endif
			DebugConsole.Log( "Removing target" + _viewID );
			this.healthMap.Remove( _viewID );

		}
		else
		{
			DebugConsole.Warning( "No target with id " + _viewID + " found in healthmap" );
		}
	}

	public BaseHealth GetTargetWithID( NetworkViewID _viewID )
	{
		BaseHealth target = null;
		if ( this.healthMap.TryGetValue( _viewID, out target ) )
		{
			return target;
		}
		DebugConsole.Warning( "Could not find target with ID " + _viewID );
		return null;
	}

	public void GetTargetsFromPlayer( BaseWeaponManager _weaponScript, Transform _transform,
	                                float _maxDistance = -1.0f, float _maxAngle = -1.0f, TEAM _team = TEAM.NEUTRAL )
	{
		_weaponScript.currentTarget = null;
		_weaponScript.otherTargets.Clear();

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

			if ( _team != TEAM.NEUTRAL && health.team != _team )
			{
				continue;
			}

			Target t = new Target( health, angle, dist );

			_weaponScript.otherTargets.Add( t );
		}
	
		_weaponScript.otherTargets.Sort( 
		delegate( Target _x, Target _y ) 
		{ 
			return _x.angle.CompareTo( _y.angle ); 
		} );

		if ( _weaponScript.otherTargets.Count > 0 )
		{
			_weaponScript.currentTarget = _weaponScript.otherTargets[0];
		}
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
			DebugConsole.Error( "Cannot find target with ID " + _id );
			return;
		}
		target.DealDamage( _damage, false );
		DebugConsole.Log( target.gameObject.name + " has been dealt " + _damage, target );
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

		DebugConsole.Log("No slot of ID " + _id + " found");
		return null;
	}
}
