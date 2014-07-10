using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenuButtons : MonoBehaviour
{
	public static MainMenuButtons instance;

	public GameObject mainPanelObj;
	public GameObject hostPanelObj;
	public GameObject serverPanelObj;
	public GameObject lobbyPanelObj;
	public GameObject connectingPanelObj;

	public GUITextField hostPortField;
	public GUITextField hostNameField;
	public GUITextField hostDescField;

	public enum STATE
	{
		MAIN,
		LOBBY,
		CREATE_GAME,
		SERVER_LIST,
		CONNECTING,
	};

	public STATE currentState;

	private void Awake()
	{
		MainMenuButtons.instance = this;
		this.mainPanelObj.SetActive( true );
		this.currentState = STATE.MAIN;
	}

	private void OnHostButtonDown()
	{
		this.mainPanelObj.SetActive( false );
		this.hostPanelObj.SetActive( true );
		this.currentState = STATE.CREATE_GAME;
	}

	private void OnReturnToMainDown()
	{
		this.mainPanelObj.SetActive( true );
		this.hostPanelObj.SetActive( false );
		this.serverPanelObj.SetActive( false );
		this.currentState = STATE.MAIN;
	}

	private void OnQuitLobbyDown()
	{
		MenuNetworking.instance.QuitLobby();
		this.currentState = STATE.MAIN;
	}

	private void OnJoinButtonDown()
	{
		this.mainPanelObj.SetActive( false );
		this.serverPanelObj.SetActive( true );

		MenuServerList.instance.StartJoinMenu();
		this.currentState = STATE.SERVER_LIST;
	}

	public void OpenGameLobby()
	{
		this.connectingPanelObj.SetActive( false );
		this.lobbyPanelObj.SetActive( true );
		MenuLobby.instance.StartLobby();

		this.currentState = STATE.LOBBY;
	}

	public void ExitLobby()
	{
		this.mainPanelObj.SetActive( true );
		this.lobbyPanelObj.SetActive( false );
		this.currentState = STATE.MAIN;
	}

	public void OpenConnectingWindow()
	{
		this.connectingPanelObj.SetActive( true );
		this.serverPanelObj.SetActive( false );
		this.currentState = STATE.CONNECTING;
	}

	private void OnStartServerDown()
	{
		string gameNameString = this.hostNameField.text;
		string gameDescString = this.hostDescField.text;
		string portString = this.hostPortField.text;

		if ( gameNameString == "" )
		{
			DebugConsole.Error( "No game name provided." );
			return;
		}

		if ( portString == "" )
		{
			DebugConsole.Error( "No port number  provided." );
			return;
		}

		int portNumber;
		if ( int.TryParse( portString, out portNumber ) == false )
		{
			DebugConsole.Error( "Port number \"" + portNumber + "\" is invalid." );
			return;
		}

		MenuNetworking.instance.StartServer( portNumber, gameNameString, gameDescString );

		this.hostPanelObj.SetActive( false );
		this.lobbyPanelObj.SetActive( true );
		MenuLobby.instance.StartLobby();

		this.currentState = STATE.LOBBY;
	}

	public void OnConnectionFailure( NetworkConnectionError _info )
	{
		this.connectingPanelObj.SetActive( false );
		this.mainPanelObj.SetActive( true );

		this.currentState = STATE.MAIN;
	}

	public void OnForceStartCondition( int _playerType )
	{
		if ( !System.Enum.IsDefined( typeof(PLAYER_TYPE), _playerType ) )
		{
			DebugConsole.Error( "Unknown PLAYER_TYPE condicion " + _playerType );
		}
		else
		{
			PLAYER_TYPE type = (PLAYER_TYPE)_playerType;
			//MenuLobby.instance.ChangePlayerType( Common.MyNetworkID(), type );
			GamePlayerManager.instance.ChangePlayerType( Common.MyNetworkID(), type );
		}
	}

	private void OnExitGameDown()
	{
		Application.Quit();
		DebugConsole.Warning( "Quitting application" );
	}
}
