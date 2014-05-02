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
		base.Awake();
		GameNetworkManager.instance = this;
	}

	protected void Start()
	{
		base.Start();

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
			this.networkView.RPC( "OnConnectedToGameRPC", RPCMode.All, Network.player, (int)info.playerType );
		}
	}

	protected void Update()
	{
		base.Start();

		if ( Network.peerType != NetworkPeerType.Disconnected 
		  && Network.isServer && !this.gameHasStarted
		  && Network.time - this.timeStarted > this.startPauseDuration )
		{
			this.gameHasStarted = true;

			this.networkView.RPC( "OnGameStartedRPC", RPCMode.All );
		}

		if(Network.peerType == NetworkPeerType.Disconnected && !this.gameHasStarted)
		{
			this.gameHasStarted = true;
			LocalGameStart();
		}
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
		GamePlayerManager.instance.DisconnectPlayer( _player );
	}

	[RPC]
	//private void OnConnectedToGameRPC( NetworkPlayer _netPlayer, int _playerType )
	private void OnConnectedToGameRPC( int _playerID, int _playerType )
	{ 
		if ( !System.Enum.IsDefined( typeof(PLAYER_TYPE), _playerType ) )
		{
			Debug.LogError( "Player type " + _playerType + " not defined" );
		}
		GamePlayerManager.instance.AddPlayerOfType( _playerID, (PLAYER_TYPE)_playerType );
		//GamePlayerManager.instance.AddPlayerOfType( _netPlayer, (PLAYER_TYPE)_playerType );
	}

	[RPC] 
	private void OnGameStartedRPC()
	{
		//int id = Common.NetworkID();
		//PLAYER_TYPE state = MenuToGameInfo.instance.playerTypeMap[ id ];
		GamePlayer player = GamePlayerManager.instance.GetNetworkPlayer( Network.player );

		PlayerInstantiator.instance.CreatePlayerObject( player );
	}
	
	public void SendShootBulletMessage( BULLET_TYPE _bulletType, 
	                                   int _index,
	                                   Vector3 _pos, Vector3 _forward )
	{
		this.networkView.RPC( "OnShootBulletRPC", RPCMode.Others, Common.MyNetworkID(), (float)Network.time, (int)_bulletType, _index, _pos, _forward );
	}

	[RPC]
	private void OnShootBulletRPC( int _ownerID, float _creationTime, int _bulletType,
	                              int _index,
	                              Vector3 _pos, Vector3 _forward )
	{
		BulletManager.instance.CreateBulletRPC( _ownerID, _creationTime, (BULLET_TYPE)_bulletType, _index, _pos, _forward );
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
		TargetManager.instance.OnDealDamage( _id, _damage );
	}

	public void SendDockedMessage( NetworkViewID _id, int landedSlot )
	{
		this.networkView.RPC ( "OnFighterDockedRPC", RPCMode.Others, _id, landedSlot );
	}

	[RPC]
	private void OnFighterDockedRPC( NetworkViewID _id, int landedSlotID )
	{
		//_id.gameObject.transform.position = landedSlot.landedPosition.transform.position;
		//_id.gameObject.transform.rotation = landedSlot.landedPosition.transform.rotation;

		NetworkView dockingFighterView = NetworkView.Find (_id);

		GameObject dockingFighter = dockingFighterView.gameObject;

		DockingBay.DockingSlot landedSlot = TargetManager.instance.GetDockingSlotByID ( landedSlotID );

		dockingFighter.transform.position = landedSlot.landedPosition.transform.position;
		dockingFighter.transform.parent = landedSlot.landedPosition;

		dockingFighter.transform.rotation = landedSlot.landedPosition.transform.rotation;
		//dockingFighter.transform.localScale = new Vector3(1.0f,1.0f,1.0f);

		dockingFighter.GetComponent<NetworkPositionControl>().TogglePositionUpdates( false );

		landedSlot.landedFighter = dockingFighter.GetComponent<Fighter>();

		print("Received docked RPC");
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
		DockingBay.DockingSlot landedSlot = TargetManager.instance.GetDockingSlotByID ( landedSlotID );

		dockingFighter.transform.parent = null;
		dockingFighter.transform.localScale = new Vector3(1.0f,1.0f,1.0f);

		dockingFighter.GetComponent<NetworkPositionControl>().TogglePositionUpdates( true );
		//dockingFighter.GetComponent<NetworkPositionControl>().lerp = true;
		landedSlot.landedFighter = null;
	}

	private void LocalGameStart()
	{
		//GamePlayerManager.instance.AddPlayer( -1, PLAYER_TYPE.COMMANDER1);
		GamePlayerManager.instance.AddPlayerOfType( -1, this.defaultPlayerType );
		//GamePlayerManager.instance.AddPlayerOfType( Network.player, this.defaultPlayerType );
		GamePlayer player = GamePlayerManager.instance.GetNetworkPlayer( Network.player );
		 
		PlayerInstantiator.instance.CreatePlayerObject( player );
	}

}
