using UnityEngine;
using System.Collections;

public class GameNetworkManager : BaseNetworkManager
{
	public enum GAME_STATE
	{
		UNDEF,
		PRE_GAME,
		PLAYING,
		CAPITAL_DESTRUCTION,
		POST_GAME,
	}

	public static GameNetworkManager instance;

	public GAME_STATE gameState = GAME_STATE.PRE_GAME;

	public double startPauseDuration;
	private double timeStarted;

	public int disconnectionTimeoutMS;

#if UNITY_EDITOR
	public PLAYER_TYPE defaultPlayerType;
	public bool createDummies;
	public PLAYER_TYPE[] dummiesToCreate;

	public GamePlayer lastCreatedDummy;

	public GameObject turretPrefab;
	public Transform[] turretPositions;
#endif

	protected void Awake()
	{
		BaseNetworkManager.baseInstance = this;
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
			this.GetComponent<NetworkView>().RPC( "OnConnectedToGameRPC", RPCMode.All, Common.MyNetworkID(), (int)info.playerType );      
		}
	}

	protected void Update()
	{
		if ( Network.peerType != NetworkPeerType.Disconnected
		  && Network.isServer && this.gameState == GAME_STATE.PRE_GAME
		  && Network.time - this.timeStarted >= this.startPauseDuration )
		{
			this.gameState = GAME_STATE.PRE_GAME;

			this.GetComponent<NetworkView>().RPC( "OnGameStartedRPC", RPCMode.All );
		}

#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected
		  && this.gameState == GAME_STATE.PRE_GAME )
		{
			this.gameState = GAME_STATE.PLAYING;
			this.LocalGameStart();
		}
#endif
	}
	
	public void SendCapitalShipDeathMessage( CapitalShipMaster _capitalShip )
	{
		if ( this.gameState == GAME_STATE.CAPITAL_DESTRUCTION
		  || this.gameState == GAME_STATE.POST_GAME )
		{
			//TODO: Capital ship has already died, now another one has, hm
			return;
		}
		else if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			this.GetComponent<NetworkView>().RPC( "OnCapitalShipDeathRPC", RPCMode.Others, _capitalShip.health.Owner.id );
		}

		this.OnCapitalShipDeathRPC( _capitalShip.health.Owner.id );
	}

	[RPC]
	private void OnCapitalShipDeathRPC( int _playerID )
	{
		DebugConsole.Log( "Capital ship has reached critical damage, moving into Capital Destruction" );
		GamePlayer commander = GamePlayerManager.instance.GetPlayerWithID( _playerID );
		commander.capitalShip.isDying = true;
		this.gameState = GAME_STATE.CAPITAL_DESTRUCTION;
	}

	public void SendCapitalShipExplodedMessage( CapitalShipMaster _capitalShip )
	{
		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			this.GetComponent<NetworkView>().RPC( "OnCapitalShipExplodedRPC", RPCMode.Others );
		}

		this.OnCapitalShipExplodedRPC();
	}

	[RPC]
	private void OnCapitalShipExplodedRPC()
	{
		DebugConsole.Log( "Capital ship has exploded, moving into post-game" );
		this.gameState = GAME_STATE.POST_GAME;

		GamePlayer myPlayer = GamePlayerManager.instance.myPlayer;
		if ( myPlayer.fighter != null )
		{
			myPlayer.fighter.fighterCamera.gameObject.SetActive( false );
		}
		else if ( myPlayer.capitalShip != null )
		{
			//TODO: Disable capital camera
		}
	}

	public void OnFighterTypeSelected( FIGHTER_TYPE _fighterType )
	{
		GamePlayerManager.instance.myPlayer.fighterType = _fighterType;
		PlayerInstantiator.instance.CreatePlayerObject( GamePlayerManager.instance.myPlayer, false );
	}

	private void OnGUI()
	{
		GUI.Label( new Rect( Screen.width - 50, 25, 50, 25 ), "Player " + Common.MyNetworkID() );  
	}

	// Unity Callback
	protected override void OnDisconnectedFromServer( NetworkDisconnection _info )
	{
		DebugConsole.Log( "Disconnected from server" );
		Application.LoadLevel( "MenuScene" );
	}

	// Unity Callback
	protected override void OnPlayerDisconnected( NetworkPlayer _player )
	{
		DebugConsole.Log( _player.ipAddress + " has disconnected" );

		int playerID = Common.NetworkID( _player );
		this.GetComponent<NetworkView>().RPC( "OnDisconnectPlayerRPC", RPCMode.All, playerID );
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

		if ( player.playerType == PLAYER_TYPE.COMMANDER1 || player.playerType == PLAYER_TYPE.COMMANDER2 )
		{
			PlayerInstantiator.instance.CreatePlayerObject( player, false );
		}
		this.gameState = GAME_STATE.PLAYING;
	}

	/*public void SendShootBulletMessage( WEAPON_TYPE _weaponType, int _index, Vector3 _pos, Quaternion _rot )
	{
		this.networkView.RPC( "OnShootBulletRPC", RPCMode.Others, Common.MyNetworkID(), (float)Network.time, (int)_weaponType, _index, _pos, _rot );// _forward );
	}

	[RPC]
	private void OnShootBulletRPC( int _ownerID, float _creationTime, int _weaponType,
	                              int _index,
	                              Vector3 _pos, Quaternion _rot )
	{
		if ( !System.Enum.IsDefined( typeof(WEAPON_TYPE), _weaponType ) )
		{
			DebugConsole.Error( "Bullet type " + _weaponType + " not defined" );
		}

		BulletManager.instance.CreateBulletRPC( _ownerID, _creationTime, (WEAPON_TYPE)_weaponType, _index, _pos, _rot );
	}*/

	public void SendWeaponFireMessage( NetworkViewID _weaponManagerViewID, int _weaponIndex, WeaponFirePoint _firePoint )
	{
		this.GetComponent<NetworkView>().RPC( "OnWeaponFireRPC", RPCMode.Others, _weaponManagerViewID, _weaponIndex, (float)Network.time, _firePoint.transform.rotation );
	}

	[RPC]
	private void OnWeaponFireRPC( NetworkViewID _weaponManagerViewID, int _weaponIndex, float _timeSent, Quaternion _firePointRotation )
	{
		NetworkView view = NetworkView.Find( _weaponManagerViewID );
		BaseWeaponManager weaponManager = view.GetComponent<BaseWeaponManager>();
		weaponManager.weapons[_weaponIndex].Fire( false, _timeSent, _firePointRotation );
	}

	public void SendDestroySmartBulletMessage( NetworkViewID _viewID )
	{
		this.GetComponent<NetworkView>().RPC( "OnDestroySmartBulletRPC", RPCMode.Others, _viewID );
	}

	[RPC]
	private void OnDestroySmartBulletRPC( NetworkViewID _viewID )
	{
		BulletManager.instance.DestroySmartBulletRPC( _viewID );
	}

	public void SendDestroyDumbBulletMessage( WEAPON_TYPE _weaponType, int _index, Vector3 _bulletPosition )
	{
		this.GetComponent<NetworkView>().RPC( "OnDestroyDumbBulletRPC", RPCMode.Others, (int)_weaponType, _index, _bulletPosition );
	}

	[RPC]
	private void OnDestroyDumbBulletRPC( int _weaponType, int _index, Vector3 _bulletPosition )
	{
		//TODO: Enum check
		if ( System.Enum.IsDefined( typeof(WEAPON_TYPE), _weaponType ) == false )
		{
			DebugConsole.Error( "Undefined bullet type " + _weaponType );
			return;
		}
		BulletManager.instance.OnDisableDumbBulletMessage( (WEAPON_TYPE)_weaponType, _index, _bulletPosition );
	}

	public void SendOutOfControlFigherMessage( int _playerID )
	{
		this.GetComponent<NetworkView>().RPC( "OnOutOfControlFighterRPC", RPCMode.Others, _playerID );
	}

	[RPC]
	private void OnOutOfControlFighterRPC( int _playerID )
	{
		GamePlayerManager.instance.GetPlayerWithID( _playerID ).fighter.OnOutOfControlNetworkMessage();
	}


	public void SendDeadFighterMessage( int _playerID )
	{
		this.GetComponent<NetworkView>().RPC( "OnDeadFighterRPC", RPCMode.Others, _playerID );
	}
	[RPC]
	private void OnDeadFighterRPC( int _playerID )
	{
		GamePlayerManager.instance.GetPlayerWithID( _playerID ).fighter.OnDestroyFighterNetworkMessage();
	}


	public void SendDeadShieldMessage( NetworkViewID _id)
	{
		this.GetComponent<NetworkView>().RPC( "OnDeadShieldRPC", RPCMode.Others, _id );
	}
	[RPC]
	private void OnDeadShieldRPC( NetworkViewID _id )
	{
		TargetManager.instance.GetTargetWithID( _id ).GetComponent<ShieldGeneratorHealth>().ShieldDestroyedNetwork();
	}


	public void SendRespawnedFighterMessage( NetworkViewID _id )
	{
		this.GetComponent<NetworkView>().RPC( "OnRespawnedFighterRPC", RPCMode.Others, _id );
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
		fighterHealth.masterScript.OnRespawnFighterNetworkMessage();
	}



	public void SendDealDamageMessage( NetworkViewID _id, float _damage, GamePlayer _sourcePlayer )
	{
		int playerID = _sourcePlayer.id;
		this.GetComponent<NetworkView>().RPC( "OnDealDamageRPC", RPCMode.Others, _id, _damage, playerID );
	}

	[RPC]
	private void OnDealDamageRPC( NetworkViewID _id, float _damage, int _playerID )
	{
		GamePlayer sourcePlayer = GamePlayerManager.instance.GetPlayerWithID( _playerID );
		TargetManager.instance.OnNetworkDamageMessage( _id, _damage, sourcePlayer );
	}


	public void SendSetHealthMessage( NetworkViewID _id, float _health, float _shield )
	{
		this.GetComponent<NetworkView>().RPC( "OnSetHealthRPC", RPCMode.Others, _id, _health, _shield );
	}

	[RPC]
	private void OnSetHealthRPC( NetworkViewID _id, float _health, float _shield )
	{
		TargetManager.instance.OnSetHealthMessage( _id, _health, _shield );
	}


	public void SendDockedMessage( NetworkViewID _id, TEAM _team, int landedSlot )
	{
		this.GetComponent<NetworkView>().RPC( "OnFighterDockedRPC", RPCMode.Others, _id, (int)_team, landedSlot );
	}

	[RPC]
	private void OnFighterDockedRPC( NetworkViewID _id, int _teamID, int landedSlotID )
	{
		TEAM team = (TEAM)_teamID;
		CapitalShipMaster capitalShip = GamePlayerManager.instance.GetCommander( team ).capitalShip;
		NetworkView dockingFighterView = NetworkView.Find( _id );

		GameObject dockingFighter = dockingFighterView.gameObject;

		capitalShip.dockingBay.DockFighter( dockingFighter );
	}

	public void SendUndockedMessage( NetworkViewID _id, TEAM _team, int landedSlot )
	{
		this.GetComponent<NetworkView>().RPC ( "OnFighterUndockedRPC", RPCMode.Others, _id, (int)_team, landedSlot );
	}

	[RPC]
	private void OnFighterUndockedRPC( NetworkViewID _id, int _teamID, int landedSlotID )
	{
		NetworkView dockingFighterView = NetworkView.Find( _id );

		GameObject dockingFighter = dockingFighterView.gameObject;
		dockingFighter.GetComponent<NetworkPositionControl>().enabled = true;
		dockingFighter.transform.parent = null;
		dockingFighter.transform.localScale = Vector3.one;

		dockingFighter.GetComponent<NetworkPositionControl>().SetUpdatePosition( true );

		CapitalShipMaster capitalShip = GamePlayerManager.instance.GetCommander( (TEAM)_teamID ).capitalShip;
		capitalShip.dockingBay.slots[landedSlotID].landedFighter = null;
	}

	public void SendUpgradedMessage( int _id, float _newSpeed, float _newDefense, float _newEnergy)
	{
		this.GetComponent<NetworkView>().RPC ( "OnSendUpgradedRPC", RPCMode.Others, _id, _newSpeed, _newDefense, _newEnergy);
	}

	[RPC]
	private void OnSendUpgradedRPC( int _id, float _newSpeed, float _newDefense, float _newEnergy)
	{
		GamePlayer toUpgrade = GamePlayerManager.instance.GetPlayerWithID(_id);
		toUpgrade.defenseMultiplier = _newDefense;
		toUpgrade.energyMultiplier = _newEnergy;
		toUpgrade.speedMultiplier = _newSpeed;

		if(toUpgrade.fighter != null)
		{
			//speed - currently increases acceleration and maximum speed
			toUpgrade.fighter.movement.acceleration = toUpgrade.fighter.movement.baseAcceleration * toUpgrade.speedMultiplier;
			toUpgrade.fighter.movement.maxSpeed = toUpgrade.fighter.movement.baseMaxSpeed * toUpgrade.speedMultiplier;
			//energy - currently this increases regen rate and maximum energy
			toUpgrade.fighter.energySystem.maximumEnergy = toUpgrade.fighter.energySystem.baseMaxEnergy * toUpgrade.energyMultiplier;
			toUpgrade.fighter.energySystem.rechargePerSecond = toUpgrade.fighter.energySystem.baseRechargePerSec * toUpgrade.energyMultiplier;
			//defense - currently increases hull and shields
			toUpgrade.fighter.health.maxShield = toUpgrade.fighter.health.baseShield * toUpgrade.defenseMultiplier;
			toUpgrade.fighter.health.maxHealth = toUpgrade.fighter.health.baseHealth * toUpgrade.defenseMultiplier;
		}
	}

#if UNITY_EDITOR
	private void LocalGameStart()
	{
		DebugConsole.Log( "Local game start" );

		GamePlayerManager.instance.AddPlayerOfType( -1, this.defaultPlayerType );

		if ( GamePlayerManager.instance.myPlayer.playerType == PLAYER_TYPE.COMMANDER1
		  || GamePlayerManager.instance.myPlayer.playerType == PLAYER_TYPE.COMMANDER2 )
		{
			PlayerInstantiator.instance.CreatePlayerObject( GamePlayerManager.instance.myPlayer, false );
		}

		FIGHTER_TYPE[] fighterTypes = { FIGHTER_TYPE.AGILE, FIGHTER_TYPE.HEAVY, FIGHTER_TYPE.SPEED, };

		if ( this.createDummies )
		{
			for ( int i = 0; i < this.dummiesToCreate.Length; ++i )
			{
				PLAYER_TYPE playerType = this.dummiesToCreate[i];
				if ( GamePlayerManager.instance.myPlayer.playerType == playerType
				  && ( playerType == PLAYER_TYPE.COMMANDER1 || playerType == PLAYER_TYPE.COMMANDER2 ) )
					continue;

				DebugConsole.Log( "Creating dummy #" + i + " " + playerType );

				// -1 is reserved for the controlled player, start going below that
				int playerID = -2 - i;
				GamePlayer dummyPlayer = GamePlayerManager.instance.AddPlayerOfType( playerID, playerType );
				this.lastCreatedDummy = dummyPlayer;
				if ( playerType == PLAYER_TYPE.FIGHTER1 || playerType == PLAYER_TYPE.FIGHTER2 )
				{
					dummyPlayer.fighterType = fighterTypes[ i % 3];
				}
				PlayerInstantiator.instance.CreatePlayerObject( dummyPlayer, true );
			}
		}

		if ( GamePlayerManager.instance.commander2 != null )
		{
			foreach ( Transform pos in turretPositions )
			{
				GameObject turretObj = GameObject.Instantiate( this.turretPrefab, pos.position, pos.rotation ) as GameObject;
				turretObj.GetComponent<TurretBehavior>().health.Owner = GamePlayerManager.instance.commander2;
			}
		}
	}
#endif

	public void SendSetViewIDMessage( int _ownerID, NetworkViewID _id )
	{
		this.GetComponent<NetworkView>().RPC( "OnSetViewIDRPC", RPCMode.Others, _ownerID, _id );
	}

	[RPC]
	private void OnSetViewIDRPC( int _ownerID, NetworkViewID _id )
	{
		NetworkOwnerManager.instance.ReceiveSetViewID( _ownerID, _id );
	}

	public void SendSmartBulletInfoRPC( NetworkViewID _viewID, int _playerID, NetworkViewID _targetID )
	{
		this.GetComponent<NetworkView>().RPC( "OnSmartBulletInfoRPC", RPCMode.Others, _viewID, (int)_playerID, _targetID );
	}

	[RPC]
	private void OnSmartBulletInfoRPC( NetworkViewID _viewID, int _playerID, NetworkViewID _targetID )
	{
		if ( BulletManager.instance.smartBulletMap.ContainsKey( _viewID ) )
		{
			SmartBullet bullet = BulletManager.instance.smartBulletMap[_viewID];


			DebugConsole.Log( "Changing smart bullet " + _viewID + " owner to " + _playerID, bullet  );
			bullet.health.Owner = GamePlayerManager.instance.GetPlayerWithID( _playerID );


			SeekingBullet seekingBullet = bullet as SeekingBullet;
			if ( seekingBullet != null
			  && _targetID != NetworkViewID.unassigned )
			{
				seekingBullet.target = TargetManager.instance.GetTargetWithID( _targetID );
				DebugConsole.Log( "Setting smart bullet target to " + seekingBullet.target.gameObject.name, seekingBullet.target );
			}
		}
		else
		{
			DebugConsole.Warning( "Could not find smart bullet with view ID " + _viewID );
		}

	}

	public void SendTractorStartMessage( NetworkViewID _viewID, TractorBeam.TractorFunction _tractorDirection, NetworkViewID _targetID)
	{
		this.GetComponent<NetworkView>().RPC( "OnTractorStartRPC", RPCMode.Others, _viewID, (int)_tractorDirection, _targetID );
	}

	[RPC]
	private void OnTractorStartRPC( NetworkViewID _viewID, int _tractorDirection, NetworkViewID _targetID )
	{
		NetworkView tractorSource = NetworkView.Find( _viewID );
		NetworkView targetSource = NetworkView.Find( _targetID );

		TractorBeam.TractorFunction tractorFuntion = (TractorBeam.TractorFunction)_tractorDirection;
		tractorSource.GetComponent<TractorBeam>().FireAtTarget( targetSource.gameObject, tractorFuntion );
	}

	public void SendTractorStopMessage(NetworkViewID _viewID)
	{
		this.GetComponent<NetworkView>().RPC( "OnTractorStopRPC", RPCMode.Others, _viewID );
	}

	[RPC]
	private void OnTractorStopRPC(NetworkViewID _viewID)
	{
		NetworkView TractorSource = NetworkView.Find (_viewID);

		TractorSource.GetComponent<TractorBeam>().RefreshTarget();	
	}
	

	public void SendAddScoreMessage( SCORE_TYPE _scoreType, GamePlayer _player )
	{
		this.GetComponent<NetworkView>().RPC( "OnAddScoreRPC", RPCMode.Others, (int)_scoreType, _player.id );
	}

	[RPC]
	private void OnAddScoreRPC( int _scoreType, int _playerID )
	{
		//TODO: Score type enum check
		GamePlayer player = GamePlayerManager.instance.GetPlayerWithID( _playerID );
		ScoreManager.instance.AddScore( (SCORE_TYPE)_scoreType, player, false );
	}

	public void QuitGame()
	{
		Network.Disconnect( this.disconnectionTimeoutMS );

		// Go back to the menu
		Application.LoadLevel( 0 );
	}

	public void SendRemoveTargetMessage( NetworkViewID _targetID )
	{
		this.GetComponent<NetworkView>().RPC( "OnRemoveTargetRPC", RPCMode.Others, _targetID );
	}

	[RPC]
	private void OnRemoveTargetRPC( NetworkViewID _targetID )
	{
		TargetManager.instance.RemoveTargetByID( _targetID );
	}

	public void SendAddKillMessage( int _playerID )
	{
		this.GetComponent<NetworkView>().RPC( "OnAddKillRPC", RPCMode.Others, _playerID );
	}

	[RPC]
	private void OnAddKillRPC( int _playerID )
	{
		GamePlayerManager.instance.AddKill( _playerID, false );
	}

	public void SendAddDeathMessage( int _playerID )
	{
		this.GetComponent<NetworkView>().RPC( "OnAddDeathRPC", RPCMode.Others, _playerID );
	}

	[RPC]
	private void OnAddDeathRPC( int _playerID )
	{
		GamePlayerManager.instance.AddDeath( _playerID, false );
	}
}
