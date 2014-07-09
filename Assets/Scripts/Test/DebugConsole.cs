﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DebugConsole
{
	public static bool changed = false;

	public static List<string> outputLines = new List<string>();
	//public static string output = "";
	public static string input = "";

	public static void Log( string _str )
	{
		DebugConsole.Write( _str );
		Debug.Log( _str );
	}

	public static void Warning( string _str )
	{
		DebugConsole.Write( _str );
		Debug.LogWarning( _str );
	}

	public static void Error( string _str )
	{
		DebugConsole.Write( _str );
		Debug.LogError( _str );
	}

	public static void Write( string _str )
	{
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
		default:
		{
			AddLine( "Unknown command \"" + input + "\". Type \"help\" for a list of commands." );
			break;
		}
		}

		input = "";
	}

	private static void OnHelp( string[] _chunks )
	{
		AddLine( "Available commands:" );
		AddLine( "playerlist - Displays a list of all players" );
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
			AddLine( "tgm 1/0" );
			AddLine( "Where 1 is on, 0 is off" );
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
}