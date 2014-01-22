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
	
	private enum STATE
	{
		MAIN,
		SERVERS,
		HOST,
		GAME_LOBBY,
		CONNECTING,
	}

	private MainMenuButtons.STATE currentState = MainMenuButtons.STATE.MAIN;

	private void Awake()
	{
		MainMenuButtons.instance = this;
		this.mainPanelObj.SetActive( true );
	}

	private void Update()
	{
		switch ( this.currentState )
		{
		case STATE.MAIN:
		{
			break;
		}
		case STATE.HOST:
		{
			break;
		}
		case STATE.SERVERS:
		{
			break;
		}
		case STATE.GAME_LOBBY:
		{
			break;
		}
		case STATE.CONNECTING:
		{
			break;
		}
		default:
		{
			Debug.Log( "Uncaught state: " + this.currentState, this );
			break;
		}
		}
	}

	private void OnHostButtonDown()
	{
		this.currentState = STATE.HOST;
		this.mainPanelObj.SetActive( false );
		this.hostPanelObj.SetActive( true );
	}

	private void OnReturnToMainDown()
	{
		this.currentState = STATE.MAIN;
		this.mainPanelObj.SetActive( true );
		this.hostPanelObj.SetActive( false );
		this.serverPanelObj.SetActive( false );
	}

	private void OnQuitLobbyDown()
	{
		MenuNetworking.instance.QuitLobby();
	}

	private void OnJoinButtonDown()
	{
		this.mainPanelObj.SetActive( false );
		this.serverPanelObj.SetActive( true );

		MenuServerList.instance.StartJoinMenu();
	}

	public void OpenGameLobby()
	{
		this.currentState = STATE.GAME_LOBBY;
		this.connectingPanelObj.SetActive( false );
		this.lobbyPanelObj.SetActive( true );
		MenuLobby.instance.StartLobby();
	}

	public void ExitLobby()
	{
		this.mainPanelObj.SetActive( true );
		this.lobbyPanelObj.SetActive( false );
	}

	public void OpenConnectingWindow()
	{
		this.currentState = STATE.CONNECTING;
		this.connectingPanelObj.SetActive( true );
		this.serverPanelObj.SetActive( false );
	}

	private void OnStartServerDown()
	{
		string gameNameString = this.hostNameField.text;
		string gameDescString = this.hostDescField.text;
		string portString = this.hostPortField.text;

		if ( gameNameString == "" )
		{
			Debug.LogError( "No game name provided." );
			return;
		}

		if ( portString == "" )
		{
			Debug.LogError( "No port number  provided." );
			return;
		}

		int portNumber;
		if ( int.TryParse( portString, out portNumber ) == false )
		{
			Debug.LogError( "Port number \"" + portNumber + "\" is invalid." );
			return;
		}

		MenuNetworking.instance.StartServer( portNumber, gameNameString, gameDescString );

		this.hostPanelObj.SetActive( false );
		this.lobbyPanelObj.SetActive( true );
		MenuLobby.instance.StartLobby();
	}
}
