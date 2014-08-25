using UnityEngine;
using System.Collections;

public class CapitalShipComponent : MonoBehaviour
{
	public NetworkOwnerControl ownerControl;
	public BaseHealth health;

	private bool ownerInitialised = false;

	private void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		NetworkOwnerManager.instance.RegisterUnknownObject( this );
	}

#if UNITY_EDITOR
	private void Start()
	{
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			this.ParentToOwnerShip( GamePlayerManager.instance.myPlayer );
		}
	}
#endif


	private void Update()
	{
		if ( this.ownerInitialised == false 
		  && this.ownerControl.ownerID != -1 )
		{
			int playerID = this.ownerControl.ownerID;
			GamePlayer player = GamePlayerManager.instance.GetPlayerWithID( playerID );
			if ( player != null && player.capitalShip != null )
			{
				this.OwnerInitialise();
			}
		}
	}

	private void OwnerInitialise()
	{
		this.ownerInitialised = false;
		
		int ownerID = this.ownerControl.ownerID;
		GamePlayer ownerPlayer = GamePlayerManager.instance.GetPlayerWithID( ownerID );
		
		if ( ownerPlayer.capitalShip == null )
		{
			DebugConsole.Warning( "Turret instantiated by non-commander player", this );
		}
		
		this.ParentToOwnerShip( ownerPlayer );
		this.health.team = ownerPlayer.team;
	
		if ( !this.networkView.isMine )
		{
			this.enabled = false;
		}
	}

	private void ParentToOwnerShip( GamePlayer _player )
	{
		this.transform.parent = _player.capitalShip.depthControl;
	}
}
