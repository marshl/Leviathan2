using UnityEngine;
using System.Collections;

public class GameNetworkManager : BaseNetworkManager
{
	public static GameNetworkManager instance;

	public double startPauseDuration;
	private double timeStarted;
	private bool gameHasStarted = false;

	public PLAYER_TYPE defaultPlayerType;

	protected void Awake()
	{
		GameNetworkManager.instance = this;
	}

	protected void Start()
	{
		this.timeStarted = Network.time;

		MenuToGameInfo info = MenuToGameInfo.instance;

		if ( info == null )
		{
			GameObject obj = new GameObject();
			info = obj.AddComponent<MenuToGameInfo>();
			info.UseDefaults();

			Debug.LogWarning( "No menu to game info found. Using default values", info );
		}

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			//TODO: This may have issues with laggy players LM 28/04/14
			this.networkView.RPC( "OnConnectedToGameRPC", RPCMode.All, Common.MyNetworkID(), (int)info.playerType );      
		}
	}

	protected void Update()
	{
		if ( Network.peerType != NetworkPeerType.Disconnected
		  && Network.isServer && !this.gameHasStarted
		  && Network.time - this.timeStarted > this.startPauseDuration )
		{
			this.gameHasStarted = true;

			this.networkView.RPC( "OnGameStartedRPC", RPCMode.All );
		}

		if ( Network.peerType == NetworkPeerType.Disconnected
		  && !this.gameHasStarted )
		{
			this.gameHasStarted = true;
			LocalGameStart();
		}
	}

	private void OnGUI()
	{
		GUI.Label( new Rect( Screen.width - 50, 25, 50, 25 ), "Player " + Common.MyNetworkID() );  
	}

	// Unity Callback
	protected override void OnDisconnectedFromServer( NetworkDisconnection _info )
	{
		Debug.Log( "Disconnected from server" );
		Application.LoadLevel( "MenuTest" );
	}

	// Unity Callback
	protected override void OnPlayerDisconnected( NetworkPlayer _player )
	{
		Debug.Log( _player.ipAddress + " has disconnected" );

		int playerID = Common.NetworkID( _player );
		this.networkView.RPC( "OnDisconnectPlayerRPC", RPCMode.All, playerID );
	}
	[RPC]
	private void OnDisconnectPlayerRPC( int _playerID )
	{
		GamePlayerManager.instance.DisconnectPlayer( _playerID );
	}

	[RPC]
	private void OnConnectedToGameRPC( int _playerID, int _playerType )
	{
		if ( !System.Enum.IsDefined( typeof(PLAYER_TYPE), _playerType ) )
		{
			Debug.LogError( "Player type " + _playerType + " not defined" );
		}
		GamePlayerManager.instance.AddPlayerOfType( _playerID, (PLAYER_TYPE)_playerType );
	}

	[RPC]
	private void OnGameStartedRPC()
	{
		GamePlayer player = GamePlayerManager.instance.GetPlayerWithID( Common.MyNetworkID() );

		PlayerInstantiator.instance.CreatePlayerObject( player );
		this.gameHasStarted = true;
	}

	public void SendShootBulletMessage( BULLET_TYPE _bulletType, int _index, Vector3 _pos, Quaternion _rot )
	{
		this.networkView.RPC( "OnShootBulletRPC", RPCMode.Others, Common.MyNetworkID(), (float)Network.time, (int)_bulletType, _index, _pos, _rot );// _forward );
	}

	[RPC]
	private void OnShootBulletRPC( int _ownerID, float _creationTime, int _bulletType,
	                              int _index,
	                              Vector3 _pos, Quaternion _rot )
	{
		if ( !System.Enum.IsDefined( typeof(BULLET_TYPE), _bulletType ) )
		{
			Debug.LogError( "Bullet type " + _bulletType + " not defined" );
		}

		BulletManager.instance.CreateBulletRPC( _ownerID, _creationTime, (BULLET_TYPE)_bulletType, _index, _pos, _rot );
	}

	public void SendDestroySmartBulletMessage( int _ownerID, BULLET_TYPE _bulletType, int _index )
	{
		Debug.Log( "Sending message to destroy" );
		this.networkView.RPC( "OnDestroySmartBulletRPC", RPCMode.Others, _ownerID, (int)_bulletType, _index );
	}

	[RPC]
	private void OnDestroySmartBulletRPC( int _ownerID, int _bulletType, int _index )
	{
		Debug.Log( "Received Destroy Message" );
		BulletManager.instance.DestroySmartBulletRPC( _ownerID, (BULLET_TYPE)_bulletType, _index );
	}

	public void SendDestroyDumbBulletMessage( BULLET_TYPE _bulletType, int _index )
	{
		this.networkView.RPC( "OnDestroyDumbBulletRPC", RPCMode.Others, (int)_bulletType, _index );
	}

	public void SendDeadFighterMessage( NetworkViewID _id)
	{
		this.networkView.RPC ("OnDeadFighterRPC",RPCMode.All,_id);
	}

	[RPC]
	private void OnDeadFighterRPC( NetworkViewID _id)
	{
		TargetManager.instance.GetTargetWithID(_id).GetComponent<FighterHealth>().FighterDestroyedNetwork();
	}

	public void SendRespawnedFighterMessage (NetworkViewID _id)
	{
		this.networkView.RPC ("OnRespawnedFighterRPC",RPCMode.All,_id);
	}

	[RPC]
	private void OnRespawnedFighterRPC(NetworkViewID _id)
	{
		TargetManager.instance.GetTargetWithID(_id).GetComponent<FighterHealth>().FighterRespawnedNetwork();
	}

	[RPC]
	private void OnDestroyDumbBulletRPC( int _bulletType, int _index )
	{
		BulletManager.instance.DestroyDumbBulletRPC( (BULLET_TYPE)_bulletType, _index );
	}

	public void SendDealDamageMessage( NetworkViewID _id, float _damage )
	{
		this.networkView.RPC( "OnDealDamageRPC", RPCMode.Others, _id, _damage );
	}

	[RPC]
	private void OnDealDamageRPC( NetworkViewID _id, float _damage )
	{
		TargetManager.instance.OnNetworkDamageMessage( _id, _damage );
	}

	public void SendDockedMessage( NetworkViewID _id, int landedSlot )
	{
		this.networkView.RPC( "OnFighterDockedRPC", RPCMode.Others, _id, landedSlot );
	}

	[RPC]
	private void OnFighterDockedRPC( NetworkViewID _id, int landedSlotID )
	{
		NetworkView dockingFighterView = NetworkView.Find( _id );

		GameObject dockingFighter = dockingFighterView.gameObject;

		if ( !dockingFighterView.isMine )
		{
			DockingBay.DockingSlot landedSlot = TargetManager.instance.GetDockingSlotByID ( landedSlotID );
			dockingFighter.transform.position = landedSlot.landedPosition.transform.position;
			dockingFighter.transform.parent = landedSlot.landedPosition;
			dockingFighter.transform.rotation = landedSlot.landedPosition.transform.rotation;
			dockingFighter.GetComponent<NetworkPositionControl>().SetUpdatePosition( false );

			landedSlot.landedFighter = dockingFighter.GetComponent<FighterMaster>();

			Debug.Log( "Received docked RPC", this );
		}
	}

	public void SendUndockedMessage( NetworkViewID _id, int landedSlot )
	{
		this.networkView.RPC ( "OnFighterUndockedRPC", RPCMode.Others, _id, landedSlot );
	}

	[RPC]
	private void OnFighterUndockedRPC( NetworkViewID _id, int landedSlotID )
	{
		NetworkView dockingFighterView = NetworkView.Find (_id);

		GameObject dockingFighter = dockingFighterView.gameObject;
		dockingFighter.GetComponent<NetworkPositionControl>().enabled = true;
		DockingBay.DockingSlot landedSlot = TargetManager.instance.GetDockingSlotByID( landedSlotID );

		dockingFighter.transform.parent = null;
		dockingFighter.transform.localScale = Vector3.one;

		dockingFighter.GetComponent<NetworkPositionControl>().SetUpdatePosition( true );
		landedSlot.landedFighter = null;
	}

	private void LocalGameStart()
	{
		GamePlayerManager.instance.AddPlayerOfType( -1, this.defaultPlayerType );
		GamePlayer player = GamePlayerManager.instance.GetNetworkPlayer( Network.player );

		PlayerInstantiator.instance.CreatePlayerObject( player );
	}

	public void SendSetViewIDMessage( int _ownerID, NetworkViewID _id )
	{
		this.networkView.RPC( "OnSetViewIDRPC", RPCMode.Others, _ownerID, _id );
	}

	[RPC]
	private void OnSetViewIDRPC( int _ownerID, NetworkViewID _id )
	{
		NetworkOwnerManager.instance.ReceiveSetViewID( _ownerID, _id );
	}
}
