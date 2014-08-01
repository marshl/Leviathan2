using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	
	public Dictionary<NetworkViewID, BaseHealth> targetMap;

#if UNITY_EDITOR
	public List<BaseHealth> debugTargets;
#endif

	private void Awake()
	{
		if ( TargetManager.instance != null )
		{
			DebugConsole.Error( "Duplicate TargetManagers detected", TargetManager.instance );
		}
		TargetManager.instance = this;

		this.targetMap = new Dictionary<NetworkViewID, BaseHealth>();
	}

	public void AddTarget( NetworkViewID _viewID, BaseHealth _health )
	{
		if ( this.targetMap.ContainsKey( _viewID ) )
		{
			DebugConsole.Warning( "Target Manager already contains " + _viewID + "(" + _health.gameObject.name + ")", _health );
			return;
		}
#if UNITY_EDITOR
		this.debugTargets.Add( _health );
#endif
		DebugConsole.Log( "Adding target " + _viewID + " (" + _health.gameObject.name + ") to TargetManager", _health );
		this.targetMap.Add( _viewID, _health );
	}

	public void RemoveTarget(NetworkViewID _viewID)
	{
		if ( this.targetMap.ContainsKey( _viewID ) )
		{
#if UNITY_EDITOR
			this.debugTargets.Remove( this.targetMap[_viewID] );
#endif
			DebugConsole.Log( "Removing target" + _viewID );
			this.targetMap.Remove( _viewID );

		}
		else
		{
			DebugConsole.Warning( "No target with id " + _viewID + " found in healthmap" );
		}
	}

	public BaseHealth GetTargetWithID( NetworkViewID _viewID )
	{
		BaseHealth target = null;
		if ( this.targetMap.TryGetValue( _viewID, out target ) )
		{
			return target;
		}
		DebugConsole.Warning( "Could not find target with ID " + _viewID );
		return null;
	}

	public void GetTargets( BaseWeaponManager _weaponScript, Transform _transform,
	                                float _maxDistance = -1.0f, float _maxAngle = -1.0f, TEAM _team = TEAM.NEUTRAL )
	{
		_weaponScript.targetList.Clear();

		foreach ( KeyValuePair<NetworkViewID, BaseHealth> pair in this.targetMap )
		{
			BaseHealth health = pair.Value;

			// Ignore dead targets
			if ( health.currentHealth <= 0.0f 
			    || health.enabled == false 
			    || health.gameObject.activeInHierarchy == false )
			{
				continue;
			}

			// Ignore targets out of range
			Vector3 v = health.transform.position - _transform.position;
			float dist = v.magnitude;
			if ( _maxDistance > 0.0f && dist > _maxDistance )
			{
				continue;
			}

			// Ignore targets that are out of angle
			float angle = Vector3.Angle( _transform.forward, v.normalized );
			if ( _maxAngle > 0.0f && angle >= _maxAngle )
			{
				continue;
			}

			if ( _team != TEAM.NEUTRAL && health.team != _team )
			{
				continue;
			}

			_weaponScript.targetList.Add( health );
		}
	}

	public void AreaOfEffectDamage( Vector3 _position, float _radius, float _damage, bool _friendlyFire, TEAM _sourceTeam )
	{
		foreach ( KeyValuePair<NetworkViewID, BaseHealth> pair in this.targetMap )
		{
			if ( _friendlyFire == true && _sourceTeam == pair.Value.team )
			{
				return;
			}

			float distance = ( pair.Value.transform.position - _position ).magnitude;
			if ( distance < _radius )
			{
				float multiplier = 1.0f - distance / _radius;
				pair.Value.DealDamage( _damage * multiplier, true );
			}
		}
	}

	public void DealDamageNetwork( NetworkViewID _id, float _damage )
	{
		GameNetworkManager.instance.SendDealDamageMessage( _id, _damage );
	}

	public void OnNetworkDamageMessage( NetworkViewID _id, float _damage ) 
	{
		BaseHealth target;
		if ( !this.targetMap.TryGetValue( _id, out target ) )
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
