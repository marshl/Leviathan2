﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GUIPlaceholder
{
	public Rect rect;
	public string text = "";

	public GUIPlaceholder(){}

	public GUIPlaceholder( Rect _rect, string _text )
	{
		this.rect = _rect;
		this.text = _text;
	}
	
	public void Label()
	{
		GUI.Label( this.rect, this.text );
	}

    public bool Button()
    {
        return GUI.Button( this.rect, this.text );
    }
};

[System.Serializable]
public class PlayerDisplayRow
{
	public GamePlayer player;
	public Ping ping;
	public int lastPing;

	public GUIPlaceholder nameGUI;
	public GUIPlaceholder ipGUI;
	public GUIPlaceholder pingGUI;

	public void Display()
	{
		this.nameGUI.Label();
		this.ipGUI.Label();
		this.pingGUI.Label();
	}
};

[System.Serializable]
public class ServerDisplayRow
{
	public GUIPlaceholder nameLabel;
	public GUIPlaceholder ipLabel;
	public GUIPlaceholder pingLabel;
	public GUIPlaceholder commentLabel;
	public GUIPlaceholder connectButton;
};

public class MenuGUI : MonoBehaviour
{
	public static MenuGUI instance;

	public enum STATE
	{
		MAIN,
		LOBBY,
		CREATE_GAME,
		SERVER_LIST,
		CONNECTING,
	};

	public STATE state = STATE.MAIN;

	public Vector2 buttonScale;

	/**
	 * MAIN
	 */
	public Vector2 mainButtonOffset;
	public Vector2 mainButtonGap;

	private GUIPlaceholder mainCreateGameButton;
	private GUIPlaceholder mainServerListButton;
	private GUIPlaceholder mainQuitButton;

	/*
	 * CREATE GAME
	 */
	public string defaultGameName;
	public string defaultGamePort;
	public string defaultGameDescription;

	public int gameNameLengthLimit;

	public Vector2 createGameLabelScale;
	public Vector2 createGameTextFieldScale;
	public Vector2 createGameCornerOffset;
	public Vector2 createGameTextFieldGap;

	private GUIPlaceholder createGameNameLabel;
	private GUIPlaceholder createGameDescriptionLabel;
	private GUIPlaceholder createGamePortLabel;
	private GUIPlaceholder createGamePasswordLabel;

	private GUIPlaceholder createGameNameField;
	private GUIPlaceholder createGameDescriptionField;
	private GUIPlaceholder createGamePortField;
	private GUIPlaceholder createGamePasswordField;

	public Vector2 createGameButtonOffset;
	public Vector2 createGameButtonGap;

	public GUIPlaceholder createGameCreateButton;
	public GUIPlaceholder createGameBackButton;

	/*
	 * LOBBY
	 */

	public Vector2 playerRowOffset;
	public float lobbyRowHeight;
	public float lobbyNameWidth;
	public float lobbyIPWidth;
	public float lobbyPingWidth;
	public float lobbyTeamGap;

	private GUIPlaceholder lobbyGameNameLabel;
	public Rect lobbyGameNameRect;
	private GUIPlaceholder lobbyGameCommentLabel;
	public Rect lobbyGameCommentRect;

	private GUIPlaceholder lobbyBackButton;
	private GUIPlaceholder lobbyStartButton;

	private PlayerDisplayRow commander1Row;
	private List<PlayerDisplayRow> fighters1;
	private PlayerDisplayRow commander2Row;
	private List<PlayerDisplayRow> fighters2;

	public Vector2 forceTypeButtonOffset;
	public Vector2 forceTypeButtonGap;

	private GUIPlaceholder forceFighter1Button;
	private GUIPlaceholder forceFighter2Button;
	private GUIPlaceholder forceCommander1Button;
	private GUIPlaceholder forceCommander2Button;

	public Vector2 lobbyControlButtonOffset;
	public float lobbyControlButtonGap;

	public Vector2 chatOffset;
	public float chatEnterWidth;
	public float chatEnterHeight;
	public float chatDisplayHeight;

	private GUIPlaceholder chatEnterField;
	private GUIPlaceholder chatDisplayArea;
	private GUIPlaceholder chatSendButton;

	/*
	 * SERVER LIST
	 */
	
	private List<ServerDisplayRow> serverRows;
    public Vector2 serverRowListOffset;
    public float serverRowHeight;
    public float serverRowYGap;
    public float serverRowNameWidth;
    public float serverRowIPWidth;
    public float serverRowPingWidth;
    public float serverRowXGap;
	public float serverRowButtonWidth;

	private GUIPlaceholder serverListBackButton;
	public Vector2 serverListButtonOffset;

	private HostData targetHost;

	/*
	 * POPUP
	 */

	public enum POPUP_STATE
	{
		NONE,
		MESSAGE,
		SERVER_PASSWORD,
	};
	private POPUP_STATE popupState = POPUP_STATE.NONE;
	
	private string popupLabelText;
	private string popupButtonText;
	private string popupHeaderText;

	public Vector2 popupWindowSize;

	public Vector2 popupTextFieldScale;
	public Vector2 popupButtonOffset;
	public Vector2 popupTextFieldOffset;
	private string popupPassword = "";




	public void Start()
	{
		instance = this;

		this.UpdateMainGUI( true );
		this.UpdateCreateGameGUI( true );
		this.UpdateLobbyGUI( true );
		this.UpdateServerListGUI( true );

		this.serverRows = new List<ServerDisplayRow>();
	}

	private void Update()
	{
		switch ( this.state )
		{
		case STATE.MAIN:
		{
			this.UpdateMainGUI( false );
			break;
		}
		case STATE.LOBBY:
		{
			this.UpdateLobbyGUI( false );
			break;
		}
		case STATE.CREATE_GAME:
		{
			this.UpdateCreateGameGUI( false );
			break;
		}
		case STATE.SERVER_LIST:
		{
			this.UpdateServerListGUI( false );
			break;
		}
		case STATE.CONNECTING:
		{
			this.UpdateConnectingGUI( false );
			break;
		}
		default:
		{
			DebugConsole.Error( "Uncaught menu state \"" + this.state + "\"", this );
			break;
		}
		}
	}

	private void OnGUI()
	{
		if ( this.popupState != POPUP_STATE.NONE )
		{
			GUI.enabled = false;
		}

		switch ( this.state )
		{
		case STATE.MAIN:
		{
			this.RenderMainGUI();
			break;
		}
		case STATE.LOBBY:
		{
			this.RenderLobbyGUI();
			break;
		}
		case STATE.CREATE_GAME:
		{
			this.RenderCreateGameGUI();
			break;
		}
		case STATE.SERVER_LIST:
		{
			this.RenderServerListGUI();
			break;
		}
		case STATE.CONNECTING:
		{
			this.RenderConnectingGUI();
			break;
		}
		default:
		{
			DebugConsole.Error( "Uncaught menu state \"" + this.state + "\"", this );
			break;
		}
		}

		/*if ( this.popupState != POPUP_STATE.NONE )
		{
			bool done = this.RenderPopUpWindow();
			if ( done )
			{
				this.popupEnabled = false;
			}
		}*/

		if ( this.popupState != POPUP_STATE.NONE )
		{
			int result = this.RenderPopUpWindow();

			if ( result == 1 )
			{
				this.popupState = POPUP_STATE.NONE;
			}
		}

		GUI.enabled = true;
	}

	private void UpdateMainGUI( bool _firstPass )
	{
		if ( _firstPass || Application.isEditor )
		{
			this.mainCreateGameButton = new GUIPlaceholder(
				this.GetTiledAreaRect( this.mainButtonOffset, this.buttonScale, this.mainButtonGap, new Vector2(0,0) ),
				"Create Game" );

			this.mainServerListButton = new GUIPlaceholder(
				this.GetTiledAreaRect( this.mainButtonOffset, this.buttonScale, this.mainButtonGap, new Vector2(0,1) ),
				"Join Game" );

			this.mainQuitButton = new GUIPlaceholder(
				this.GetTiledAreaRect( this.mainButtonOffset, this.buttonScale, this.mainButtonGap, new Vector2(0,2) ),
				"Quit Game" );
		}
	}
	
	private void RenderMainGUI()
	{
		if ( GUI.Button( this.mainCreateGameButton.rect, this.mainCreateGameButton.text ) )
		{
			this.state = STATE.CREATE_GAME;
		}

		if ( GUI.Button( this.mainServerListButton.rect, this.mainServerListButton.text ) ) 
		{
			this.state = STATE.SERVER_LIST;
		}

		if ( GUI.Button( this.mainQuitButton.rect, this.mainQuitButton.text ) ) 
		{
			Application.Quit();
		}
	}

	private void UpdateLobbyGUI( bool _firstPass )
	{
		int fighterCount = MenuNetworking.instance.connectionLimit/2 - 1;

		if ( _firstPass )
		{
			this.commander1Row = this.CreatePlayerRow( TEAM.TEAM_1, 0, fighterCount );
			this.commander2Row = this.CreatePlayerRow( TEAM.TEAM_2, 0, fighterCount );

			this.fighters1 = new List<PlayerDisplayRow>();
			this.fighters2 = new List<PlayerDisplayRow>();

			for ( int i = 0; i < fighterCount; ++i )
			{
				this.fighters1.Add( this.CreatePlayerRow( TEAM.TEAM_1, i+1, fighterCount ) );
				this.fighters2.Add( this.CreatePlayerRow( TEAM.TEAM_2, i+1, fighterCount ) );
			}

			this.forceFighter1Button = new GUIPlaceholder();
			this.forceFighter1Button.rect = new Rect( this.forceTypeButtonOffset.x, this.forceTypeButtonOffset.y,
			                                         this.buttonScale.x, this.buttonScale.y );
			this.forceFighter1Button.text = "Fighter 1";


			this.forceFighter2Button = new GUIPlaceholder();
			this.forceFighter2Button.rect = new Rect( this.forceTypeButtonOffset.x + this.forceTypeButtonGap.x, this.forceTypeButtonOffset.y,
			                                         this.buttonScale.x, this.buttonScale.y );
			this.forceFighter2Button.text = "Fighter 2";


			this.forceCommander1Button = new GUIPlaceholder();
			this.forceCommander1Button.rect = new Rect( this.forceTypeButtonOffset.x, this.forceTypeButtonOffset.y + this.forceTypeButtonGap.y,
			                                         this.buttonScale.x, this.buttonScale.y );
			this.forceCommander1Button.text = "Commander 1";


			this.forceCommander2Button = new GUIPlaceholder();
			this.forceCommander2Button.rect = new Rect( this.forceTypeButtonOffset.x + this.forceTypeButtonGap.x, this.forceTypeButtonOffset.y + this.forceTypeButtonGap.y,
			                                         this.buttonScale.x, this.buttonScale.y );
			this.forceCommander2Button.text = "Commander 2";



			this.lobbyBackButton = new GUIPlaceholder();
			this.lobbyBackButton.rect = new Rect( this.lobbyControlButtonOffset.x, this.lobbyControlButtonOffset.y,
			                                     this.buttonScale.x, this.buttonScale.y );
			this.lobbyBackButton.text = "Back";


			this.lobbyStartButton = new GUIPlaceholder();
			this.lobbyStartButton.rect = new Rect( this.lobbyControlButtonOffset.x + this.buttonScale.x + this.lobbyControlButtonGap,
			                                      this.lobbyControlButtonOffset.y, this.buttonScale.x, this.buttonScale.y );
			this.lobbyStartButton.text = "Start";

			this.chatDisplayArea = new GUIPlaceholder();
			this.chatDisplayArea.rect = new Rect( this.chatOffset.x, this.chatOffset.y,
			                                      this.chatEnterWidth, this.chatDisplayHeight );
			this.chatDisplayArea.text = "";

			this.chatEnterField = new GUIPlaceholder();
			this.chatEnterField.rect = new Rect( this.chatOffset.x, this.chatOffset.y + this.chatDisplayHeight,
			                                     this.chatEnterWidth, this.chatEnterHeight );

			this.chatSendButton = new GUIPlaceholder();
			this.chatSendButton.rect = new Rect( this.chatOffset.x + this.chatEnterWidth,
			                                    this.chatOffset.y + this.chatDisplayHeight,
			                                    this.buttonScale.x, this.buttonScale.y );
			this.chatSendButton.text = "Send";

			this.lobbyGameNameLabel = new GUIPlaceholder();
			this.lobbyGameNameLabel.text = MenuNetworking.instance.gameName;
			this.lobbyGameNameLabel.rect = this.lobbyGameNameRect;

			this.lobbyGameCommentLabel = new GUIPlaceholder();
			this.lobbyGameCommentLabel.text = MenuNetworking.instance.gameComment;
			this.lobbyGameCommentLabel.rect = this.lobbyGameCommentRect;
		}

		UpdatePlayerRow( this.commander1Row, GamePlayerManager.instance.commander1 );
		UpdatePlayerRow( this.commander2Row, GamePlayerManager.instance.commander2 );

		for ( int i = 0; i < this.fighters1.Count; ++i )
		{
			GamePlayer player = i < GamePlayerManager.instance.fighters1.Count
				? GamePlayerManager.instance.fighters1[i] : null;

			UpdatePlayerRow( this.fighters1[i], player );

			player = i < GamePlayerManager.instance.fighters2.Count
				? GamePlayerManager.instance.fighters2[i] : null;

			UpdatePlayerRow( this.fighters2[i], player );
		}

		string chat = "";
		for ( int i = MessageManager.instance.messages.Count - 1; i >= 0; --i )
		{
			chat += MessageManager.instance.GetFormattedMessage( i );
		}
		this.chatDisplayArea.text = chat;
	}

	private void UpdatePlayerRow( PlayerDisplayRow _row, GamePlayer _player )
	{
		_row.player = _player;

		_row.nameGUI.text = _player != null ? _player.name : "----------";
		_row.ipGUI.text = _player != null ? _player.networkPlayer.ipAddress : "---.---.---.---";

		if ( _player != null )
		{
			if ( _row.ping == null )
			{
				_row.ping = new Ping( _player.networkPlayer.ipAddress );
			}

			if ( _row.ping.isDone )
			{
				_row.pingGUI.text = _row.ping.time + "ms";
				_row.ping = null;
			}
		}
		else
		{
			_row.pingGUI.text = "-----";
		}

	}

	private PlayerDisplayRow CreatePlayerRow( TEAM _team, int _pos, int _fighterCount )
	{
		PlayerDisplayRow row = new PlayerDisplayRow();

		float xPos = this.playerRowOffset.x;
		float yPos = this.playerRowOffset.y + _pos * this.lobbyRowHeight;
		if ( _team == TEAM.TEAM_2 )
		{
			yPos += this.lobbyTeamGap;
		}

		row.nameGUI = new GUIPlaceholder();
		row.nameGUI.rect = new Rect( xPos, yPos, this.lobbyNameWidth, this.lobbyRowHeight );

		xPos += this.lobbyNameWidth;

		row.ipGUI = new GUIPlaceholder();
		row.ipGUI.rect = new Rect( xPos, yPos, this.lobbyIPWidth, this.lobbyRowHeight );

		xPos += this.lobbyIPWidth;

		row.pingGUI = new GUIPlaceholder();
		row.pingGUI.rect = new Rect( xPos, yPos, this.lobbyPingWidth, this.lobbyRowHeight );

		return row;
	}

	private void RenderLobbyGUI()
	{
		this.lobbyGameNameLabel.Label();
		this.lobbyGameCommentLabel.Label();

		this.commander1Row.Display();

		foreach ( PlayerDisplayRow row in this.fighters1 )
		{
			row.Display();
		}

		this.commander2Row.Display();

		foreach ( PlayerDisplayRow row in this.fighters2 )
		{
			row.Display();
		}

		if ( this.forceFighter1Button.Button() )
		{
			this.OnForcePlayerTypeButtonDown( PLAYER_TYPE.FIGHTER1 );
		}
		if ( this.forceFighter2Button.Button() )
		{
			this.OnForcePlayerTypeButtonDown( PLAYER_TYPE.FIGHTER2 );
		}
		if ( this.forceCommander1Button.Button() )
		{
			this.OnForcePlayerTypeButtonDown( PLAYER_TYPE.COMMANDER1 );
		}
		if ( this.forceCommander2Button.Button() )
		{
			this.OnForcePlayerTypeButtonDown( PLAYER_TYPE.COMMANDER2 );
		}

		if ( this.lobbyBackButton.Button() )
		{
			this.state = STATE.MAIN;
			MenuNetworking.instance.DisconnectFromLobby();
		}

		if ( Network.isServer )
		{
			if ( this.lobbyStartButton.Button() )
			{
				MenuNetworking.instance.StartGame();
			}
		}

		GUI.SetNextControlName( "ChatEnterField" );

		GUIStyle style = GUI.skin.textField;
		if ( GUI.GetNameOfFocusedControl() != "ChatEnterField" && this.chatEnterField.text == "" )
		{
			style.normal.textColor = Color.grey;
			GUI.TextField( this.chatEnterField.rect, "Type here to chat" );
		}
		else
		{
			this.chatEnterField.text = GUI.TextField( this.chatEnterField.rect, this.chatEnterField.text );
		}

		style = GUI.skin.textArea;
		style.richText = true;
		GUI.TextArea( this.chatDisplayArea.rect, this.chatDisplayArea.text, style );

		if ( this.chatSendButton.Button()
		  || Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "ChatEnterField" )
		{
			if ( this.chatEnterField.text.Trim().Length > 0 )
			{
				MenuNetworking.instance.SendLobbyMessage( this.chatEnterField.text, true );
				this.chatEnterField.text = "";
			}
		}
	}

	private void UpdateCreateGameGUI( bool _firstPass )
	{
		if ( _firstPass || Application.isEditor )
		{
			this.createGameNameLabel = new GUIPlaceholder(
				this.GetTiledAreaRect( this.createGameCornerOffset, this.createGameLabelScale,
			                      this.createGameTextFieldGap, new Vector2( 0, 0 ) ), "Game Name:" );

			this.createGameDescriptionLabel = new GUIPlaceholder(
				this.GetTiledAreaRect( this.createGameCornerOffset, this.createGameLabelScale,
			                      this.createGameTextFieldGap, new Vector2( 0, 1 ) ), "Description:" );

			this.createGamePortLabel = new GUIPlaceholder(
				this.GetTiledAreaRect( this.createGameCornerOffset, this.createGameLabelScale,
			                      this.createGameTextFieldGap, new Vector2( 0, 2 ) ), "Port Number:" );

			this.createGamePasswordLabel = new GUIPlaceholder(
				this.GetTiledAreaRect( this.createGameCornerOffset, this.createGameLabelScale,
			                      this.createGameTextFieldGap, new Vector2( 0, 3 ) ), "Password" );

			if ( _firstPass )
			{
				this.createGameNameField = new GUIPlaceholder();
				this.createGameNameField.text = this.defaultGameName;

				this.createGameDescriptionField = new GUIPlaceholder();
				this.createGameDescriptionField.text = this.defaultGameDescription;

				this.createGamePortField = new GUIPlaceholder();
				this.createGamePortField.text = this.defaultGamePort;

				this.createGamePasswordField = new GUIPlaceholder();
				this.createGamePasswordField.text = "";
			}

			this.createGameNameField.rect = this.GetTiledAreaRect( 
				this.createGameCornerOffset, this.createGameLabelScale,
				this.createGameTextFieldGap, new Vector2( 1, 0 ) );

			this.createGameDescriptionField.rect = this.GetTiledAreaRect(
				this.createGameCornerOffset, this.createGameLabelScale,
				this.createGameTextFieldGap, new Vector2( 1, 1 ) );

			this.createGamePortField.rect = this.GetTiledAreaRect( 
				this.createGameCornerOffset, this.createGameLabelScale,
				this.createGameTextFieldGap, new Vector2( 1, 2 ) );
		

			this.createGamePasswordField.rect = this.GetTiledAreaRect( 
				this.createGameCornerOffset, this.createGameLabelScale,
				this.createGameTextFieldGap, new Vector2( 1, 3 ) );

			this.createGameBackButton = new GUIPlaceholder( 
		      new Rect( this.createGameButtonOffset.x,
			            this.createGameButtonOffset.y,
			            this.buttonScale.x, 
			            this.buttonScale.y ), "Back" );

			this.createGameCreateButton = new GUIPlaceholder( 
				new Rect( this.createGameButtonOffset.x + this.createGameButtonGap.x,
						  this.createGameButtonOffset.y + this.createGameButtonGap.y,
						  this.buttonScale.x, 
						  this.buttonScale.y ), "Create" );
		}
	}

	private void RenderCreateGameGUI()
	{
		GUI.Label( this.createGameNameLabel.rect, this.createGameNameLabel.text );
		GUI.Label( this.createGameDescriptionLabel.rect, this.createGameDescriptionLabel.text );
		GUI.Label( this.createGamePortLabel.rect, this.createGamePortLabel.text );
		GUI.Label( this.createGamePasswordLabel.rect, this.createGamePasswordLabel.text );

		this.createGameNameField.text = GUI.TextField( this.createGameNameField.rect, this.createGameNameField.text );
		if ( this.createGameNameField.text.Length > this.gameNameLengthLimit )
		{
			this.createGameNameField.text = this.createGameNameField.text.Remove( this.gameNameLengthLimit );
		}

		this.createGameDescriptionField.text = GUI.TextField( this.createGameDescriptionField.rect, this.createGameDescriptionField.text );
		this.createGamePortField.text = GUI.TextField( this.createGamePortField.rect, this.createGamePortField.text );
		this.createGamePasswordField.text = GUI.TextField( this.createGamePasswordField.rect, this.createGamePasswordField.text );

		if ( GUI.Button( this.createGameBackButton.rect, this.createGameBackButton.text ) )
		{
			this.state = STATE.MAIN;
			return;
		}

		if ( GUI.Button( this.createGameCreateButton.rect, this.createGameCreateButton.text ) )
		{
			this.OnCreateGameButtonDown();
			return;
		}
	}

	private void UpdateConnectingGUI( bool _firstPass )
	{

	}

	private void RenderConnectingGUI()
	{

	}

	private void UpdateServerListGUI( bool _firstPass )
	{
		if ( MenuNetworking.instance.gameHosts != null
		  && MenuNetworking.instance.gameHosts.Length != 0 )
		{
			this.serverRows.Clear();

			for ( int i = 0; i < MenuNetworking.instance.gameHosts.Length; ++i )
			//for ( int i = 0; i < 10; ++i ) //Testing purposes
			{
				HostData hostData = MenuNetworking.instance.gameHosts[0];//[i];
				ServerDisplayRow row = new ServerDisplayRow();
				this.serverRows.Add( row );

				float yPos = this.serverRowListOffset.y + (float)(i) * ( this.serverRowYGap + this.serverRowHeight );
			
				row.nameLabel = new GUIPlaceholder();
				row.nameLabel.text = hostData.gameName;
				row.nameLabel.rect = new Rect( this.serverRowListOffset.x, yPos,
				                            this.serverRowNameWidth, this.serverRowHeight );

				row.ipLabel = new GUIPlaceholder();
				row.ipLabel.text = string.Join( ".", hostData.ip );
				row.ipLabel.rect = new Rect( this.serverRowListOffset.x + this.serverRowNameWidth + this.serverRowXGap, yPos,
				                          this.serverRowIPWidth, this.serverRowHeight );

				row.pingLabel = new GUIPlaceholder( new Rect( this.serverRowListOffset.x + this.serverRowNameWidth + this.serverRowXGap * 2 + this.serverRowIPWidth,
				                                           yPos, this.serverRowPingWidth, this.serverRowHeight ), "PING" );

				row.connectButton = new GUIPlaceholder( new Rect(
					this.serverRowListOffset.x + this.serverRowNameWidth + this.serverRowXGap * 3 + this.serverRowIPWidth + this.serverRowPingWidth,
					yPos, this.serverRowButtonWidth, this.serverRowHeight ), "Connect" );
			
			}
		}

		if ( _firstPass || Application.isEditor )
		{
			this.serverListBackButton = new GUIPlaceholder();
			this.serverListBackButton.text = "Back";
			this.serverListBackButton.rect = new Rect( this.serverListButtonOffset.x, this.serverListButtonOffset.y,
			                                          this.buttonScale.x, this.buttonScale.y );
		}
	}

	private void RenderServerListGUI()
	{
		if ( this.serverListBackButton.Button() )
		{
			this.state = STATE.MAIN;
			return;
		}

		for ( int i = 0; i < this.serverRows.Count; ++i )
		{
			ServerDisplayRow row = this.serverRows[i];
			GUI.Label( row.nameLabel.rect, row.nameLabel.text );
			GUI.Label( row.ipLabel.rect, row.ipLabel.text );
			GUI.Label( row.pingLabel.rect, row.pingLabel.text );

			if ( GUI.Button( row.connectButton.rect, row.connectButton.text ) )
			{
				this.targetHost = MenuNetworking.instance.GetHostData( i );
				if ( this.targetHost.passwordProtected )
				{
					this.StartPasswordPopup();
				}
				else
				{
					this.TryServerConnect( this.targetHost, "" );
				}
			}
		}

		if ( this.popupState == POPUP_STATE.SERVER_PASSWORD )
		{
			int val = this.RenderPopUpWindow();

			if ( val == -1 )
			{
				this.popupState = POPUP_STATE.NONE;
			}
			else if ( val == 1 )
			{
				this.TryServerConnect( this.targetHost, this.popupPassword );
			}
		}
	}

	private void TryServerConnect( HostData _hostData, string _password )
	{
		NetworkConnectionError error = MenuNetworking.instance.Connect(
			this.targetHost.ip[0], this.targetHost.port, this.popupPassword );

		switch ( error )
		{
		case NetworkConnectionError.NoError:
		{
			this.popupState = POPUP_STATE.NONE;
			this.state = STATE.CONNECTING;
			break;
		}
		default:
		{
			this.StartMessagePopup( error.ToString(), "Close" );
			break;
		}
		}

	}

	private Rect GetTiledAreaRect( Vector2 _offset, Vector2 _scale, Vector2 _gap, Vector2 _pos )
	{
		return new Rect( _offset.x + _scale.x * _pos.x + _gap.x * _pos.x,
		                _offset.y + _scale.y * _pos.y + _gap.y * _pos.y, 
		                _scale.x, _scale.y );
	}

	private void OnCreateGameButtonDown()
	{
		if ( this.createGameNameField.text == "" )
		{
			DebugConsole.Error( "No game name provided." );
			return;
		}

		string portString = this.createGamePortField.text;

		int portNumber;
		if ( int.TryParse( portString, out portNumber ) == false )
		{
			this.StartMessagePopup( "Invalid port number", "Close" );
			DebugConsole.Error( "Port number \"" + portNumber + "\" is invalid." );
			return;
		}

		NetworkConnectionError info = MenuNetworking.instance.StartServer(
			portNumber, 
			this.createGameNameField.text,
			this.createGameDescriptionField.text,
			this.createGamePasswordField.text );
	
		switch ( info )
		{
		case NetworkConnectionError.ConnectionFailed:
		{
			DebugConsole.Error( "Connection failure" );
			break;
		}
		case NetworkConnectionError.NoError:
		{
			this.OpenGameLobby();
			break;
		}
		default:
		{
			DebugConsole.Log( info.ToString() );
			break;
		}
		}
	}

	public void OpenGameLobby()
	{
		this.state = STATE.LOBBY;

		this.lobbyGameNameLabel.text = MenuNetworking.instance.gameName;
		this.lobbyGameCommentLabel.text = MenuNetworking.instance.gameComment;
	}

	/*public void ExitLobby( NetworkDisconnection _info )
	{
		this.state = STATE.MAIN;
	}*/

	public void OnFailedToConnect( NetworkConnectionError _info )
	{
		this.StartMessagePopup( _info.ToString(), "Close" );
		this.state = STATE.MAIN;
	}

	private void OnForcePlayerTypeButtonDown( PLAYER_TYPE _type )
	{
		if ( _type == GamePlayerManager.instance.myPlayer.playerType )
		{
			return;
		}

		if ( Network.isServer )
		{
			MenuNetworking.instance.SendChangePlayerTypeMessage( Common.MyNetworkID(), _type );
		}
		else
		{
			MenuNetworking.instance.SendValidatePlayerTypeChange( Common.MyNetworkID(), _type );
		}
	}

	public void StartMessagePopup( string _labelText , string _buttonText )
	{
		this.popupState = POPUP_STATE.MESSAGE;

		this.popupLabelText = _labelText;
		this.popupButtonText = _buttonText;
	}

	public void StartPasswordPopup()
	{
		this.popupState = POPUP_STATE.SERVER_PASSWORD;
	}

	private int RenderPopUpWindow()
	{
		GUI.enabled = true;
		GUI.BeginGroup( new Rect( Screen.width/2 - this.popupWindowSize.x/2,
		                         Screen.height/2 - this.popupWindowSize.y/2,
		                         this.popupWindowSize.x, this.popupWindowSize.y ), GUI.skin.box );

		int returnVal = 0;

		GUI.Label( new Rect( this.popupWindowSize.x/2 - 50, 0, 100, 20 ), this.popupLabelText );

		switch ( this.popupState )
		{
		case POPUP_STATE.NONE:
		{
			break;
		}
		case POPUP_STATE.MESSAGE:
		{
			returnVal = GUI.Button( new Rect( this.popupWindowSize.x/2 - this.buttonScale.x/2, 50,
			                                 this.buttonScale.x, this.buttonScale.y ), this.popupButtonText ) ? 1 : 0;
			break;
		}
		case POPUP_STATE.SERVER_PASSWORD:
		{
			this.popupPassword = GUI.TextField( 
		       new Rect( this.popupWindowSize.x/2 - this.popupTextFieldScale.x/2,
			             this.popupTextFieldOffset.y,
			         this.popupTextFieldScale.x, this.popupTextFieldScale.y ),
			                                   this.popupPassword );

			if ( GUI.Button( new Rect( this.popupWindowSize.x/2 - this.popupButtonOffset.x - this.buttonScale.x,
			                          this.popupButtonOffset.y, this.buttonScale.x, this.buttonScale.y ), "Cancel" ) )
			{
				returnVal = -1;
			}
			else if ( GUI.Button( new Rect( this.popupWindowSize.x/2 + this.popupButtonOffset.x,
			                               this.popupButtonOffset.y, this.buttonScale.x, this.buttonScale.y ), "Accept" ) )
			{
				returnVal = 1;
			}
			break;
		}
		default:
		{
			DebugConsole.Error( "Uncaught popup state " + this.popupState );
			break;
		}
		}

		GUI.EndGroup();
		GUI.enabled = false;

		return returnVal;
	}

	public void OnDisconnectedFromServer( NetworkDisconnection _info )
	{
		switch ( _info )
		{
		case NetworkDisconnection.LostConnection:
		{
			this.StartMessagePopup( "Lost connection to server", "Close" );
			break;
		}
		case NetworkDisconnection.Disconnected:
		{
			this.StartMessagePopup( "Disconnected from server", "Close" );
			break;
		}
		default:
		{
			break;
		}
		}
		this.state = STATE.MAIN;
	}
}
