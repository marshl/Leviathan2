using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DebugConsole
{
	public static bool changed = false;

	public static List<string> outputLines = new List<string>();
	public static List<string> inputLines = new List<string>();
	public static int currentInputLine = -1;
	public static string input = "";
	
	public static void Log( string _str, Object _context  = null )
	{
		if ( _context == null )
		{
			Debug.Log( _str );
		}
		else
		{
			Debug.Log( _str, _context );
		}
		DebugConsole.Write( _str );
	}

	public static void Warning( string _str, Object _context = null )
	{
		if ( _context == null )
		{
			Debug.LogWarning( _str );
		}
		else
		{
			Debug.LogWarning( _str, _context );
		}
		_str = "<color=yellow>" +_str + "</color>";
		DebugConsole.Write( _str );
	}

	public static void Error( string _str, Object _context = null )
	{
		if ( _context == null )
		{
			Debug.LogError( _str );
		}
		else
		{
			Debug.LogError( _str, _context );
		}
		_str = "<color=red>" +_str + "</color>";
		DebugConsole.Write( _str );
	}

	public static void Write( string _str )
	{
		_str += " (" + System.DateTime.Now.ToString( "HH:mm:ss tt" ) + ")";
		AddLine( _str );
	}

	public static void AddLine( string _str )
	{
		outputLines.Add( _str );
	}

	public static void ProcessInput()
	{
		AddLine( "> " + input );

		string[] chunks = input.Split( ' ' );

		if ( chunks.Length == 0 )
		{
			return;
		}
		switch ( chunks[0] )
		{
		case "help":
		{
			OnHelp( chunks );
			break;
		}
		case "playerlist":
		{
			OnPlayerList( chunks );
			break;
		}
		case "tgm":
		{
			OnTGM( chunks );
			break;
		}
		case "sethealth":
		{
			OnSetHealth( chunks );
			break;
		}
		case "die":
		{
			OnDie( chunks );
			break;
		}
		default:
		{
			AddLine( "Unknown command \"" + input + "\". Type \"help\" for a list of commands." );
			break;
		}
		}

		inputLines.Add( input );
		currentInputLine = inputLines.Count;
		input = "";
	}

	private static void OnHelp( string[] _chunks )
	{
		AddLine( "Available commands:" );
		AddLine( "playerlist - Displays a list of all players" );
		AddLine( "tgm - Toggle God Mode" );
		AddLine( "sethealth - Set the health of a player" );
		AddLine( "die - Sets your health to 0" );
	}

	private static void OnPlayerList( string[] _chunks )
	{
		if ( GamePlayerManager.instance == null )
		{
			AddLine( "Cannot find GamePlayerManager instance" );
			return;
		}

		AddLine( GamePlayerManager.instance.playerMap.Count + " Players found" );

		foreach ( KeyValuePair<int, GamePlayer> pair in GamePlayerManager.instance.playerMap )
		{
			GamePlayer player = pair.Value;
			AddLine( "ID: " + pair.Key );

			AddLine( "Name: " + player.name );
			AddLine( "Player Type: " + player.playerType );
			AddLine( "Team: " + player.team );
			AddLine( "Kills: " + player.kills + " Deaths: " + player.deaths + " Ratio: " + player.GetKillDeathRatio() );
		}
	}

	private static void OnTGM( string[] chunks )
	{
		if ( chunks.Length != 2
		  || chunks[1] != "1" && chunks[1] != "0" )
		{
			AddLine( "Toggle God Mode syntax: " );
			AddLine( "tgm [1/0]" );
			return;
		}

		bool tgm = chunks[1] == "1";

		GamePlayer player = GamePlayerManager.instance.GetPlayerWithID( Common.MyNetworkID() );

		if ( player == null )
		{
			AddLine( "Cannot find player object" );
			return;
		}

		if ( player.capitalShip != null )
		{
			player.capitalShip.health.isIndestructible = tgm;
		}
		else if ( player.fighter != null )
		{
			player.fighter.health.isIndestructible = tgm;
		}
		else
		{
			AddLine( "Ccannot find object to apply godmode to" );
			return;
		}

		AddLine( "God mode " + (tgm ? "enabled" : "disabled" ) );
	}

	private static void OnSetHealth( string[] _chunks )
	{
		int playerID = -1;
		int health;
		if ( (_chunks.Length == 2 && int.TryParse( _chunks[1], out health )
		  || _chunks.Length == 3 && int.TryParse( _chunks[1], out health ) && int.TryParse( _chunks[2], out playerID ) )
			== false )
		{
			AddLine( "Set Health syntax:" );
			AddLine( "sethealth [health] [playerid=me]" );
			return;
		}

		if ( playerID == -1 )
		{
			playerID = Common.MyNetworkID();
		}

		GamePlayer player = GamePlayerManager.instance.GetPlayerWithID( playerID );
		if ( player == null )
		{
			AddLine( "No player with ID " + playerID + " found" );
			return;
		}

		if ( player.fighter != null )
		{
			player.fighter.health.currentHealth = health;
		}
		else if ( player.capitalShip != null )
		{
			player.capitalShip.health.currentHealth = health;
		}
		else
		{
			AddLine( "No player object found to modiify" );
			return;
		}

		AddLine( "Player " + playerID + " health set to " + health );
	}

	private static void OnDie( string[] _chunks )
	{
		if ( _chunks.Length != 1 )
		{
			AddLine( "No parameters needed for 'die'" );
		}

		GamePlayer player = GamePlayerManager.instance.myPlayer;
		if ( player.fighter != null )
		{
			player.fighter.health.currentHealth = 0;
		}
		else if ( player.capitalShip != null )
		{
			player.capitalShip.health.currentHealth = 0;
		}
		else
		{
			AddLine( "No player object found to modiify" );
			return;
		}
	}
}
