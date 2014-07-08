using UnityEngine;
using System.Collections;

public class FighterHealth : BaseHealth
{

	private BaseWeaponManager lastHitBy;

	protected override void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		base.OnNetworkInstantiate( _info );

		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
		}
	}

	private void OnGUI()
	{
		if ( this.networkView.isMine )
		{
			GUI.Label( new Rect(0, 50, 150, 50), "Shields: " + this.currentShield + " / " + this.maxShield );
			GUI.Label( new Rect(0, 0, 150, 50), "Hull: " + this.currentHealth + " / " + this.maxHealth );
		}
	} 

	public override void Update()
	{
		if(this.currentHealth <= 0 && this.GetComponent<Fighter>().state != Fighter.FIGHTERSTATE.DEAD)
		{

			this.currentHealth = 0;
			this.GetComponent<Fighter>().Die (DetermineExplosion());
		}
		RegenerateShields();
	}

	protected void OnTriggerEnter( Collider _collider )
	{
		BulletBase collisionBullet = _collider.GetComponent<BulletBase>();

		if( collisionBullet != null)
		{
			if(collisionBullet.state == BulletBase.BULLET_STATE.ACTIVE_NOT_OWNED)
			{
				lastHitBy = collisionBullet.source;
			}
		}
	}

	protected int DetermineExplosion()
	{
		float leftoverDamage = this.currentHealth * -1;
		int deathType = 0; //0 is the default death, 1 is if they really got wrecked
		
		if(leftoverDamage > (this.maxHealth / 2))
		{
			deathType = 1;
		}

		return deathType;
	}

	public void FighterDestroyedNetwork()
	{
		this.gameObject.collider.enabled = false;
		//this.gameObject.
	}

	public void FighterRespawnedNetwork()
	{
		this.gameObject.collider.enabled = true;
		//this.gameObject.renderer.enabled = true;
	}
}
