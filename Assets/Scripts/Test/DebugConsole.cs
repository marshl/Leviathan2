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
	public static bool newLine = false;
	public static GameObject pickedObject;

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
		newLine = true;
	}

	public static void ProcessInput()
	{
		input = input.ToLower();
		AddLine( "> " + input );
		string[] chunks = input.Split( ' ' );

		if ( chunks.Length == 0 )
		{
			return;
		}

		string commandName = chunks[0];

		bool foundMethod = false;
		System.Type type = typeof( DebugConsole );
		foreach ( System.Reflection.MethodInfo methodInfo in type.GetMethods() )
		{
			System.Object[] attrs = methodInfo.GetCustomAttributes( typeof(CommandAttribute), false );

			if ( attrs.Length == 0 )
			{ // Not a command method, move on
				continue;
			}

			CommandAttribute commandAttr = attrs[0] as CommandAttribute;

			if ( commandName == commandAttr.command )
			{
				typeof( DebugConsole ).InvokeMember( methodInfo.Name,
				                                    System.Reflection.BindingFlags.InvokeMethod,
				                                    System.Type.DefaultBinder,
				                                    null, new object[]{chunks,} );
				foundMethod = true;
			}
		}

		if ( !foundMethod )
		{
			AddLine( "Command \"" + commandName + "\" not found. Use \"help\" for a list of commands" );
		}

		inputLines.Add( input );
		currentInputLine = inputLines.Count;
		input = "";
	}

	public class CommandAttribute : System.Attribute
	{
		public string command;
		public string helpText;

		public CommandAttribute( string _command, string _helpText )
		{
			this.command = _command;
			this.helpText = _helpText;
		}
	}

	public static int CommandAttributeCompare( CommandAttribute _cmd1, CommandAttribute _cmd2 )
	{
		return _cmd1.command.CompareTo( _cmd2.command );
	}

	[Command("help", "Displays a list of commands")]
	public static void OnHelp( string[] _chunks )
	{
		AddLine( "Available commands:" );

		List<CommandAttribute> commandList = new List<CommandAttribute>();

		System.Type type = typeof( DebugConsole );
		foreach ( System.Reflection.MethodInfo methodInfo in type.GetMethods() )
		{
			System.Object[] attrs = methodInfo.GetCustomAttributes( typeof(CommandAttribute), false );
			
			if ( attrs.Length == 0 )
			{ // Not a command method, move on
				continue;
			}
			
			CommandAttribute commandAttr = attrs[0] as CommandAttribute;

			commandList.Add( commandAttr );
		}

		commandList.Sort( CommandAttributeCompare );

		foreach ( CommandAttribute command in commandList )
		{
			AddLine( command.command + " - " + command.helpText );
		}
	}

	[Command("playerlist", "Displays a list of the players")]
	public static void OnPlayerList( string[] _chunks )
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

	[Command("tgm", "Toggles god mode")]
	public static void OnTGM( string[] chunks )
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

	[Command("sethealth", "Sets the health of a given player ID")]
	public static void OnSetHealth( string[] _chunks )
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

	[Command("die", "Kills the current player")]
	public static void OnDie( string[] _chunks )
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

	[Command("gethealth", "Returns the health of the currently selected object")]
	public static void OnGetHealth( string[] _chunks )
	{
		if ( _chunks.Length != 1 )
		{
			AddLine( "No parameters needed for gethealth" );
		}

		if ( pickedObject == null )
		{
			AddLine( "No object selected" );
			return;
		}

		BaseHealth health = pickedObject.GetComponent<BaseHealth>();

		if ( health == null )
		{
			AddLine( "Object has no health attached" );
			return;
		}

		AddLine( pickedObject.name + " HP:" + health.currentHealth + "/" + health.maxHealth + " SP:" + health.currentShield + "/" + health.maxShield );
	}

	[Command("kill", "Kills the currently selected object")]
	public static void OnKill( string[] _chunks )
	{
		if ( _chunks.Length != 1 )
		{
			AddLine( "No parameters needed for kill" );
		}
		
		if ( pickedObject == null )
		{
			AddLine( "No object selected" );
			return;
		}
		
		BaseHealth health = pickedObject.GetComponent<BaseHealth>();
		
		if ( health == null )
		{
			AddLine( "Object has no health attached" );
			return;
		}
		
		health.currentHealth = 0;
	}

	[Command("tractortest", "Moves the capital ship to a good position to test out the tractor beam")]
	public static void OnTractorTest( string[] chunks )
	{
		CapitalShipMaster capitalShip = GamePlayerManager.instance.myPlayer.capitalShip;

		if ( capitalShip == null )
		{
			DebugConsole.Error( "Could not find capital ship on player" );
			return;
		}

		capitalShip.transform.position = new Vector3( 1000.0f, 0.0f, 0.0f );
		capitalShip.movement.currentMovementSpeed = 0.0f;
		capitalShip.movement.minMoveSpeed = 0.0f;
	}

}
