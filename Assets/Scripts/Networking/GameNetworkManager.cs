using UnityEngine;
using System.Collections;

public class GameNetworkManager : BaseNetworkManager
{
	public static GameNetworkManager instance;

	public double startPauseDuration;
	private double timeStarted;
	private bool gameHasStarted = false;

#if UNITY_EDITOR
	public PLAYER_TYPE defaultPlayerType;
	public bool createCapitalShip;
#endif

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

			DebugConsole.Warning( "No menu to game info found. Using default values", info );
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

#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected
		  && !this.gameHasStarted )
		{
			this.gameHasStarted = true;
			this.LocalGameStart();
		}
#endif
	}

	private void OnGUI()
	{
		GUI.Label( new Rect( Screen.width - 50, 25, 50, 25 ), "Player " + Common.MyNetworkID() );  
	}

	// Unity Callback
	protected override void OnDisconnectedFromServer( NetworkDisconnection _info )
	{
		DebugConsole.Log( "Disconnected from server" );
		Application.LoadLevel( "MenuTest" );
	}

	// Unity Callback
	protected override void OnPlayerDisconnected( NetworkPlayer _player )
	{
		DebugConsole.Log( _player.ipAddress + " has disconnected" );

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
			DebugConsole.Error( "Player type " + _playerType + " not defined" );
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
			DebugConsole.Error( "Bullet type " + _bulletType + " not defined" );
		}

		BulletManager.instance.CreateBulletRPC( _ownerID, _creationTime, (BULLET_TYPE)_bulletType, _index, _pos, _rot );
	}

	public void SendDestroySmartBulletMessage( NetworkViewID _viewID )
	{
		this.networkView.RPC( "OnDestroySmartBulletRPC", RPCMode.Others, _viewID );
	}

	[RPC]
	private void OnDestroySmartBulletRPC( NetworkViewID _viewID )
	{
		BulletManager.instance.DestroySmartBulletRPC( _viewID );
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
		BaseHealth health = TargetManager.instance.GetTargetWithID( _id );
		if ( health == null )
		{
			DebugConsole.Error( "Could not find fighter" );
			return;
		}
		FighterHealth fighterHealth = health as FighterHealth;
		if ( fighterHealth == null )
		{
			DebugConsole.Error( "Could not convert BaseHealth to FigherHealth" );
			return;
		}

		fighterHealth.masterScript.FighterDestroyedNetwork();
	}

	public void SendDeadShieldMessage( NetworkViewID _id)
	{
		this.networkView.RPC ("OnDeadShieldRPC",RPCMode.All,_id);
	}
	
	[RPC]
	private void OnDeadShieldRPC( NetworkViewID _id)
	{
		TargetManager.instance.GetTargetWithID(_id).GetComponent<ShieldGeneratorHealth>().ShieldDestroyedNetwork();
	}

	public void SendRespawnedFighterMessage( NetworkViewID _id )
	{
		this.networkView.RPC ("OnRespawnedFighterRPC",RPCMode.All,_id);
	}

	[RPC]
	private void OnRespawnedFighterRPC( NetworkViewID _id )
	{
		BaseHealth health = TargetManager.instance.GetTargetWithID( _id );
		if ( health == null )
		{
			DebugConsole.Error( "Could not find fighter" );
			return;
		}
		FighterHealth fighterHealth = health as FighterHealth;
		if ( fighterHealth == null )
		{
			DebugConsole.Error( "Could not convert BaseHealth to FigherHealth" );
			return;
		}
		fighterHealth.masterScript.FighterRespawnedNetwork();
	}

	[RPC]
	private void OnDestroyDumbBulletRPC( int _bulletType, int _index )
	{
		//TODO: Enum check
		if ( System.Enum.IsDefined( typeof(BULLET_TYPE), _bulletType ) == false )
		{
			DebugConsole.Error( "Undefined bullet type " + _bulletType );
			return;
		}
		BulletManager.instance.DestroyDumbBulletRPC( (BULLET_TYPE)_bulletType, _index );
	}

	public void SendDealDamageMessage( NetworkViewID _id, float _damage, NetworkViewID _sourceID )
	{
		this.networkView.RPC( "OnDealDamageRPC", RPCMode.Others, _id, _damage, _sourceID );
	}

	[RPC]
	private void OnDealDamageRPC( NetworkViewID _id, float _damage, NetworkViewID _sourceID )
	{
		TargetManager.instance.OnNetworkDamageMessage( _id, _damage, _sourceID );
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

			DebugConsole.Log( "Received docked RPC", this );
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

#if UNITY_EDITOR
	private void LocalGameStart()
	{
		DebugConsole.Log( "Local game start" );

		GamePlayerManager.instance.AddPlayerOfType( -1, this.defaultPlayerType );
		PlayerInstantiator.instance.CreatePlayerObject( GamePlayerManager.instance.myPlayer );

		if ( this.createCapitalShip )
		{
			PLAYER_TYPE dummyType = GamePlayerManager.instance.myPlayer.playerType == PLAYER_TYPE.COMMANDER1
				? PLAYER_TYPE.COMMANDER2 : PLAYER_TYPE.COMMANDER1;

			GamePlayer capitalPlayer = GamePlayerManager.instance.AddPlayerOfType( -2, dummyType );
			PlayerInstantiator.instance.CreatePlayerObject( capitalPlayer );
		}
	}
#endif

	public void SendSetViewIDMessage( int _ownerID, NetworkViewID _id )
	{
		this.networkView.RPC( "OnSetViewIDRPC", RPCMode.Others, _ownerID, _id );
	}

	[RPC]
	private void OnSetViewIDRPC( int _ownerID, NetworkViewID _id )
	{
		NetworkOwnerManager.instance.ReceiveSetViewID( _ownerID, _id );
	}

	public void SendSetSmartBulletTeamMessage( NetworkViewID _viewID, TEAM _team, NetworkViewID _targetID )
	{
		this.networkView.RPC( "OnSetSmartBulletTeamRPC", RPCMode.Others, _viewID, (int)_team, _targetID );
	}

	[RPC]
	private void OnSetSmartBulletTeamRPC( NetworkViewID _viewID, int _team, NetworkViewID _targetID )
	{
		if ( !System.Enum.IsDefined( typeof(TEAM), _team ) )
		{
			DebugConsole.Error( "Parameter could not be converted to team: " + _team );
			return;
		}

		if ( BulletManager.instance.seekingBulletMap.ContainsKey( _viewID ) )
		{
			SeekingBullet bullet = BulletManager.instance.seekingBulletMap[_viewID];
			DebugConsole.Log( "Changing smart bullet " + _viewID + " to team " + (TEAM)_team, bullet  );
			bullet.health.team = (TEAM)_team;

			if ( _targetID != NetworkViewID.unassigned )
			{
				bullet.target = TargetManager.instance.GetTargetWithID( _targetID );
			}
		}
		else
		{
			DebugConsole.Warning( "Could not find smart bullet with view ID " + _viewID );
		}

	}

	public void SendTractorStartMessage( NetworkViewID _viewID, TractorBeam.TractorFunction _tractorDirection, NetworkViewID _targetID)
	{

		switch(_tractorDirection)
		{
		case TractorBeam.TractorFunction.HOLD:
			this.networkView.RPC( "OnTractorStartRPC", RPCMode.Others, _viewID, 0, _targetID );
			break;
		case TractorBeam.TractorFunction.PULL:
			this.networkView.RPC( "OnTractorStartRPC", RPCMode.Others, _viewID, -1, _targetID );
			break;
		case TractorBeam.TractorFunction.PUSH:
			this.networkView.RPC( "OnTractorStartRPC", RPCMode.Others, _viewID, 1, _targetID );
			break;

		}


	}

	[RPC]
	private void OnTractorStartRPC( NetworkViewID _viewID, int _tractorDirection, NetworkViewID _targetID)
	{
		NetworkView tractorSource = NetworkView.Find (_viewID);
		NetworkView targetSource = NetworkView.Find (_targetID);

		switch(_tractorDirection)
		{
		case 0:
			tractorSource.GetComponent<TractorBeam>().FireAtTarget (targetSource.gameObject, TractorBeam.TractorFunction.HOLD);
			break;
		case -1:
			tractorSource.GetComponent<TractorBeam>().FireAtTarget (targetSource.gameObject, TractorBeam.TractorFunction.PULL);
			break;
		case 1:
			tractorSource.GetComponent<TractorBeam>().FireAtTarget (targetSource.gameObject, TractorBeam.TractorFunction.PUSH);
			break;

		}


	}

	public void SendTractorStopMessage(NetworkViewID _viewID)
	{
		this.networkView.RPC( "OnTractorStopRPC", RPCMode.Others, _viewID );
	}

	[RPC]
	private void OnTractorStopRPC(NetworkViewID _viewID)
	{
		NetworkView TractorSource = NetworkView.Find (_viewID);

		TractorSource.GetComponent<TractorBeam>().RefreshTarget();	
	}
}
