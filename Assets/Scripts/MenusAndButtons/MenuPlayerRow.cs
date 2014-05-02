using UnityEngine;
using System.Collections;

public class MenuPlayerRow : MonoBehaviour
{
	// Editor Variables
	public GUIText nameText;
	public GUIText ipText;
	public GUIText latencyText;

	// Public Variables
	//public int playerIndex;

	public void UpdateGUI( GamePlayer _player )
	{
		/*if ( this.playerIndex >= Network.connections.Length )
		{
			this.SetDefaults();
			return;
		}*/
		
		//NetworkPlayer player = MenuLobby.instance//Network.connections[playerIndex];
		//TODO: have to store NetworkPlayer somewhere (use MenuPlayer class) LM 30/04/14
		/*NetworkPlayer player = Network.player; // TODO: Gotta be a better way to default LM 30/04/14
		for ( int i = 0; i < Network.connections.Length; ++i )
		{
			if ( Common.NetworkID( Network.connections[i] ) == _playerID )
			{
				player = Network.connections[i];
			}
		}*/

		this.ipText.text = "<TODO>";//_player.netPlayer.ipAddress;//player.ipAddress;
		this.nameText.text = "Player " + _player.id;//_playerID;
	}

	public void SetDefaults()
	{
		this.nameText.text = "---";
		this.ipText.text = "---";
		this.latencyText.text = "---";
	}
}
