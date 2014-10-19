using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseWeaponManager : MonoBehaviour
{
	public BaseHealth health;
	public WeaponBase[] weapons;

	public BaseHealth currentTarget;
	
	public int? targetIndex = null;

	public TargetManager.TargetRestriction restrictions;

	protected virtual void Awake()
	{
		this.restrictions = new TargetManager.TargetRestriction();
		this.restrictions.transform = this.transform;
		this.restrictions.ignoreBelowHorizon = false;

		for ( int i = 0; i < this.weapons.Length; ++i )
		{
			this.weapons[i].weaponIndex = i;
		}
	}

	public virtual void OnBulletCreated( WeaponBase _weapon, BulletBase _bullet )
	{
		SeekingBullet seekingScript = _bullet as SeekingBullet;
		if ( seekingScript != null
		&& ( _weapon.weaponDesc.requiresWeaponLock == false || _weapon.IsLockedOntoTarget() ) )
		{
			seekingScript.target = this.currentTarget;
			seekingScript.health.Owner = this.health.Owner;
		}
	}

	public void SwitchToNextTarget()
	{
		this.restrictions.teams = (int)Common.OpposingTeam( this.health.Owner.team );
		TargetManager.instance.ShiftTargetIndex( this, 1 );
	}

	public void SwitchToPreviousTarget()
	{
		this.restrictions.teams = (int)Common.OpposingTeam( this.health.Owner.team );
		TargetManager.instance.ShiftTargetIndex( this, -1 );
	}

	public void SwitchToNextFriendlyTarget()
	{
		this.restrictions.teams = (int)this.health.Owner.team;
		TargetManager.instance.ShiftTargetIndex( this, 1 );
	}

	public void SwitchToPreviousFriendlyTarget()
	{
		this.restrictions.teams = (int)this.health.Owner.team;
		TargetManager.instance.ShiftTargetIndex( this, -1 );
	}

	public void SwitchToNextMissileTargettingMe()
	{
		this.currentTarget = BulletManager.instance.GetNextMissileLock( this, 1 );
	}
}
