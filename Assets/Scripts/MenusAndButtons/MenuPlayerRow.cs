using UnityEngine;
using System.Collections;

public class MenuPlayerRow : MonoBehaviour
{
	// Editor Variables
	public GUIText nameText;
	public GUIText ipText;
	public GUIText latencyText;

	private Ping ping;

	public void UpdateGUI( GamePlayer _player )
	{
		NetworkPlayer player = Network.player;
		for ( int i = 0; i < Network.connections.Length; ++i )
		{
			if ( Common.NetworkID( Network.connections[i] ) == _player.id )
			{
				player = Network.connections[i];
			}
		}

		this.ipText.text = player.ipAddress;
		this.nameText.text = "Player " + _player.id;

		if ( this.ping == null )
		{
			this.ping = new Ping( player.ipAddress );
		}
		else if ( this.ping.isDone )
		{
			this.latencyText.text = this.ping.time + "ms";
			this.ping = null;
		}
	}

	public void SetDefaults()
	{
		this.nameText.text = "---";
		this.ipText.text = "---";
		this.latencyText.text = "---";
	}
}
