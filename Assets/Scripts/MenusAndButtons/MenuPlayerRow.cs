using UnityEngine;
using System.Collections;

public class MenuPlayerRow : MonoBehaviour
{
	// Editor Variables
	public GUIText nameText;
	public GUIText ipText;
	public GUIText latencyText;
	public GUIText typeText;

	private Ping ping;

	public void UpdateGUI( GamePlayer _player )
	{
		NetworkPlayer player = _player.networkPlayer;//Network.player;

		this.ipText.text = player.ipAddress;
		this.nameText.text = "Player " + _player.id;

		this.typeText.text = _player.playerType.ToString();

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
