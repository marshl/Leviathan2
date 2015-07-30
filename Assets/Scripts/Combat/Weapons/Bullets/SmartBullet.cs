using UnityEngine;
using System.Collections;

/// <summary>
/// An subclass of the base bullet class that is used to control bullets that can move around
/// </summary>
public class SmartBullet : BulletBase
{
	public BaseHealth health;

#if UNITY_EDITOR
	public int debugID;
#endif
	
	[Tooltip( "The descriptor of this bullet, converted into the right subclass DO NOT SET" ) ]
	public SmartBulletDesc smartDesc;
	
	protected virtual void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		BulletManager.instance.smartBulletMap.Add( this.GetComponent<NetworkView>().viewID, this );
		if ( this.GetComponent<NetworkView>().isMine == false )
		{
			this.enabled = false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this.smartDesc = this.desc as SmartBulletDesc;

#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			this.debugID = BulletManager.instance.debugSeekingID;
			BulletManager.instance.debugSmartBulletMap.Add( this.debugID, this );
			++BulletManager.instance.debugSeekingID;
		}
#endif
	}

	protected override void Update()
	{
		base.Update();

		if ( this.state != BULLET_STATE.FADING
		  && this.health.currentHealth <= 0.0f )
		{
			BulletManager.instance.DestroyLocalBullet( this );
		}
	}
}

