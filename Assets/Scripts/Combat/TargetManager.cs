using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This is used to select what targets a player/turret will target
// These need to be kept as powers of 2 so they can be |'d together
public enum TARGET_TYPE : int
{
	NONE = 0,
	FIGHTER = 1,
	TURRET = 2,
	CAPITAL_SHIP_PRIMARY = 4,
	CAPITAL_SHIP_SUB = 8,
	MISSILE = 16,
};

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

	public Dictionary<int, BaseHealth> debugTargetMap;
	public int currentDebugID;
#endif

	private void Awake()
	{
		if ( TargetManager.instance != null )
		{
			DebugConsole.Error( "Duplicate TargetManagers detected", TargetManager.instance );
		}
		TargetManager.instance = this;

		this.targetMap = new Dictionary<NetworkViewID, BaseHealth>();
#if UNITY_EDITOR
		this.debugTargetMap = new Dictionary<int, BaseHealth>();
		this.debugTargets = new List<BaseHealth>();
#endif
	}

	public void AddTarget( BaseHealth _health )
	{
#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			this.debugTargetMap.Add( this.currentDebugID, _health );
			DebugConsole.Log( "Added target " + this.currentDebugID, _health );
			_health.debugTargetID = this.currentDebugID;
			this.currentDebugID++;
		}
		else
#endif
		{
			NetworkViewID viewID = _health.networkView.viewID;
			if ( this.targetMap.ContainsKey( viewID ) )
			{
				DebugConsole.Warning( "Target Manager already contains " + viewID + " (" + this.GetTargetWithID( viewID ).gameObject.name+ ")", _health );
				return;
			}

			DebugConsole.Log( "Adding target " + viewID + " (" + _health.gameObject.name + ") to TargetManager", _health );
			this.targetMap.Add( viewID, _health );
		}

#if UNITY_EDITOR
		this.debugTargets.Add( _health );
#endif
	}

	public void RemoveTarget( NetworkViewID _viewID )
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

#if UNITY_EDITOR
	public void RemoveDebugTarget( int _id )
	{
		if ( this.debugTargetMap.ContainsKey( _id ) )
		{
			BaseHealth target = this.debugTargetMap[_id];
			this.debugTargetMap.Remove( _id );
			DebugConsole.Log( "Removed target " + _id, target );

			this.debugTargets.Remove( target );
		}
		else
		{
			DebugConsole.Warning( "Failed to remove target with ID " + _id );
		}
	}
#endif

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

#if UNITY_EDITOR
	public BaseHealth GetTargetWithDebugID( int _id )
	{
		BaseHealth target = null;
		if ( this.debugTargetMap.TryGetValue( _id, out target ) )
		{
			return target;
		}
		DebugConsole.Warning( "Could not find target with ID " + _id );
		return null;
	}
#endif

	public void GetTargets( BaseWeaponManager _weaponScript, Transform _transform, int _targetTypes,
	                                float _maxDistance, TEAM _team )
	{
		_weaponScript.targetList.Clear();

		if ( _targetTypes == 0 )
		{
			return;
		}

#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			foreach ( KeyValuePair<int, BaseHealth> pair in this.debugTargetMap )
			{
				if ( this.IsValidTarget( pair.Value, _transform, _targetTypes, _maxDistance, _team ) )
				{
					_weaponScript.targetList.Add( pair.Value );
				}
			}
		}
		else
#endif
		{
			foreach ( KeyValuePair<NetworkViewID, BaseHealth> pair in this.targetMap )
			{
				if ( this.IsValidTarget( pair.Value, _transform, _targetTypes, _maxDistance, _team ) )
				{
					_weaponScript.targetList.Add( pair.Value );
				}
			}
		}
	}

	private bool IsValidTarget( BaseHealth _health, Transform _transform, int _targetTypes,
	                           float _maxDistance, TEAM _team )
	{
		// Ignore dead targets
		if ( _health == null
		  || _health.currentHealth <= 0.0f 
		  || _health.enabled == false 
		  || _health.gameObject.activeInHierarchy == false
		  || ((int)(_health.targetType) & _targetTypes) == 0 )
		{
			return false;
		}
		
		// Ignore targets out of range
		Vector3 v = _health.transform.position - _transform.position;
		float dist = v.magnitude;
		if ( _maxDistance > 0.0f && dist > _maxDistance )
		{
			return false;
		}
		
		if ( _team != TEAM.NEUTRAL && _health.team != _team )
		{
			return false;
		}
		return true;
	}

	public void AreaOfEffectDamage( Vector3 _position, float _radius, float _damage, bool _friendlyFire, TEAM _sourceTeam )
	{
#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			foreach ( KeyValuePair<int, BaseHealth> pair in this.debugTargetMap )
			{
				this.AreaOfEffectDamageCheck( pair.Value, _position, _radius, _damage, _friendlyFire, _sourceTeam );
			}
		}
		else
#endif
		{
			foreach ( KeyValuePair<NetworkViewID, BaseHealth> pair in this.targetMap )
			{
				this.AreaOfEffectDamageCheck( pair.Value, _position, _radius, _damage, _friendlyFire, _sourceTeam );
			}
		}
	}

	private void AreaOfEffectDamageCheck( BaseHealth _health, 
	      Vector3 _position, float _radius, float _damage, bool _friendlyFire, TEAM _sourceTeam )
	{
		if ( _friendlyFire == true && _sourceTeam == _health.team )
		{
			return;
		}
		
		float distance = (_health.transform.position - _position ).magnitude;
		if ( distance < _radius )
		{
			float multiplier = 1.0f - distance / _radius;
			_health.DealDamage( _damage * multiplier, true );
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
