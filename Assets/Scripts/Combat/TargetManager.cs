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
	//TODO: Differentiate between missiles and bombs?
};

public class TargetManager : MonoBehaviour
{
	[System.Serializable]
	public class TargetRestriction
	{
		public Transform transform;
		public float maxDistance = -1.0f;
		public bool ignoreBelowHorizon = false;
		public int types = -1;
		public int teams = -1;
	};

	public static TargetManager instance;
	
	public Dictionary<NetworkViewID, BaseHealth> targetMap;
	public LayerMask lineOfSightBlockingLayers;

#if UNITY_EDITOR
	// A handy list of all targets when in editor mode
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
			NetworkViewID viewID = _health.GetComponent<NetworkView>().viewID;
			if ( this.targetMap.ContainsKey( viewID ) )
			{
				DebugConsole.Warning( "Target Manager already contains " + viewID
				                     + " (" + this.GetTargetWithID( viewID ).gameObject.name+ ")", _health );
				return;
			}

			DebugConsole.Log( "Adding target " + viewID + " (" + _health.gameObject.name
			                 + ") to TargetManager", _health );
			this.targetMap.Add( viewID, _health );
		}

#if UNITY_EDITOR
		this.debugTargets.Add( _health );
#endif
	}

	public void RemoveTargetByID( NetworkViewID _viewID )
	{
#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			Debug.LogError( "Cannot remove target by NetworkViewID when in offline mode" );
			return;
		}
#endif

		if ( this.targetMap.ContainsKey( _viewID ) )
		{
			DebugConsole.Log( "Removing target" + _viewID );
			this.targetMap.Remove( _viewID );

		}
		else
		{
			DebugConsole.Warning( "No target with id " + _viewID + " found in healthmap" );
		}
	}

	public void RemoveTarget( BaseHealth _target )
	{
#if UNITY_EDITOR
		this.debugTargets.Remove( _target );
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			this.debugTargetMap.Remove( _target.debugTargetID );
		}
		else
#endif
		{
			this.targetMap.Remove( _target.GetComponent<NetworkView>().viewID );
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

	public List<BaseHealth> GetTargets( BaseWeaponManager _weaponScript )
	{
		List<BaseHealth> targetList = new List<BaseHealth>();
#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			foreach ( KeyValuePair<int, BaseHealth> pair in this.debugTargetMap )
			{
				if ( this.IsValidTarget( pair.Value, _weaponScript ) )
				{
					targetList.Add( pair.Value );
				}
			}
		}
		else
#endif
		{
			foreach ( KeyValuePair<NetworkViewID, BaseHealth> pair in this.targetMap )
			{
				if ( this.IsValidTarget( pair.Value, _weaponScript ) )
				{
					targetList.Add( pair.Value );
				}
			}
		}


		return targetList;
	}
	
	public List<BaseHealth> GetTargetsOfType( TARGET_TYPE _targetType )
	{
		List<BaseHealth> targetList = new List<BaseHealth>();
#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			foreach ( KeyValuePair<int, BaseHealth> pair in this.debugTargetMap )
			{
				if ( pair.Value.targetType == _targetType )
				{
					targetList.Add( pair.Value );
				}
			}
		}
		else
#endif
		{
			foreach ( KeyValuePair<NetworkViewID, BaseHealth> pair in this.targetMap )
			{
				if ( pair.Value.targetType == TARGET_TYPE.FIGHTER )
				{
					targetList.Add( pair.Value );
				}
			}
		}

		return targetList;
	}

	public bool IsValidTarget( BaseHealth _health, BaseWeaponManager _weaponScript )
	{
		// Ignore dead targets
		if ( _health == null
		  || _health.currentHealth <= 0.0f 
		  || _health.enabled == false 
		  || _health.gameObject.activeInHierarchy == false
		    // The target that is doing the search
		  || _health == _weaponScript.health
		    // And those that aren't of the type specfied
		  || ((int)(_health.targetType) & _weaponScript.restrictions.types) == 0 ) 
		{
			return false;
		}

		Vector3 vectorToTarget = _health.transform.position - _weaponScript.transform.position;
		if ( _weaponScript.restrictions.ignoreBelowHorizon && Vector3.Dot( vectorToTarget, _weaponScript.transform.up ) < 0.0f )
		{
			return false;
		}
		
		// Ignore targets out of range
		Vector3 v = _health.transform.position - _weaponScript.restrictions.transform.position;
		float dist = v.magnitude;
		if ( _weaponScript.restrictions.maxDistance > 0.0f && dist > _weaponScript.restrictions.maxDistance )
		{
			return false;
		}

		// Ignore targets not in the team target list
		if ( _health.Owner == null || ((int)_health.Owner.team & _weaponScript.restrictions.teams ) == 0 ) 
		{
			return false;
		}

		RaycastHit hitInfo;
		bool hit = Physics.Linecast( _weaponScript.transform.position, _health.transform.position,
		                            out hitInfo, this.lineOfSightBlockingLayers ); 

		if ( hit
		  && hitInfo.collider.gameObject != _health.gameObject 
		  && hitInfo.collider.gameObject != _weaponScript.gameObject )
		{
			return false;
		}

		return true;
	}

	public void AreaOfEffectDamage( Vector3 _position, float _radius, float _damage, 
	                               bool _friendlyFire, GamePlayer _sourcePlayer )
	{
#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			foreach ( KeyValuePair<int, BaseHealth> pair in this.debugTargetMap )
			{
				this.AreaOfEffectDamageCheck( pair.Value,
		             _position, _radius, _damage,
		             _friendlyFire,
				     _sourcePlayer );
			}
		}
		else
#endif
		{
			foreach ( KeyValuePair<NetworkViewID, BaseHealth> pair in this.targetMap )
			{
				this.AreaOfEffectDamageCheck( pair.Value,
                     _position, _radius, _damage,
                     _friendlyFire,
				     _sourcePlayer );
			}
		}
	}

	private void AreaOfEffectDamageCheck( BaseHealth _health, 
	      Vector3 _position, float _radius, float _damage, bool _friendlyFire,
	      GamePlayer _sourcePlayer )
	{
		if ( _friendlyFire && _sourcePlayer.team == _health.Owner.team )
		{
			return;
		}
		
		float distance = (_health.transform.position - _position ).magnitude;
		if ( distance < _radius )
		{
			float multiplier = 1.0f - distance / _radius;
			_health.DealDamage( _damage * multiplier, true, _sourcePlayer );
		}
	}

	public void OnNetworkDamageMessage( NetworkViewID _id, float _damage, GamePlayer _sourcePlayer ) 
	{
		BaseHealth target;
		if ( !this.targetMap.TryGetValue( _id, out target ) )
		{
			DebugConsole.Error( "Cannot find target with ID " + _id + " while trying to deal " + _damage
			                   + " damage from player " + _sourcePlayer.id );
			return;
		}
		DebugConsole.Log( target.gameObject.name + " has been dealt " + _damage, target );
		target.DealDamage( _damage, false, _sourcePlayer );
	}

	public void OnSetHealthMessage( NetworkViewID _id, float _health, float _shield )
	{
		BaseHealth target;
		if ( !this.targetMap.TryGetValue( _id, out target ) ) 
		{
			DebugConsole.Error( "Cannot find target with ID " + _id + " while trying to set health to "
			                   + _health + " and shield to " + _shield );
			return;
		}

		target.currentHealth = _health;
		target.currentShield = _shield;
	}

	public BaseHealth GetCentreTarget( BaseWeaponManager _weaponScript )
	{
		List<BaseHealth> targets = this.GetTargets( _weaponScript );
		
		BaseHealth currentTarget = null;
		float closestAngle = float.MaxValue;
		foreach ( BaseHealth target in targets )
		{
			if ( target.currentHealth <= 0.0f )
				continue;

			Vector3 vectorToTarget = target.transform.position - _weaponScript.restrictions.transform.position;
			float angle = Vector3.Angle( _weaponScript.restrictions.transform.forward, vectorToTarget );
			
			if ( angle < closestAngle )
			{
				currentTarget = target;
				closestAngle = angle;
			}
		}
		return currentTarget;
	}

	public void ShiftTargetIndex( BaseWeaponManager _weaponScript, int _direction )
	{
		List<BaseHealth> validTargets = this.GetTargets( _weaponScript );

		if ( validTargets.Count == 0 )
		{
			_weaponScript.currentTarget = null;
			return;
		}

		if ( _weaponScript.currentTarget == null
		  || validTargets.Count == 1 )
		{
			_weaponScript.currentTarget = validTargets[0];
			return;
		}

		int index = validTargets.IndexOf( _weaponScript.currentTarget );

		if ( index == -1 )
		{
			_weaponScript.currentTarget = validTargets[0];
			return;
		}

		index += _direction;

		while ( index >= validTargets.Count )
		{
			index -= validTargets.Count;
		}

		while ( index < 0 )
		{
			index += validTargets.Count;
		}
		_weaponScript.currentTarget = validTargets[index];
	}
}
