using UnityEngine;
using System.Collections;

public class MenuPlayerRow : MonoBehaviour
{
	// Editor Variables
	public GUIText nameText;
	public GUIText ipText;
	public GUIText latencyText;

	// Public Variables
	public int playerIndex;

	public void UpdateGUI()
	{
		if ( this.playerIndex >= Network.connections.Length )
		{
			return;
		}
		
		NetworkPlayer player = Network.connections[playerIndex];

		this.ipText.text = player.ipAddress;
		this.nameText.text = "Player " + player.ToString();//+ this.playerIndex;
	}

	public void SetDefaults()
	{
		this.nameText.text = "---";
		this.ipText.text = "---";
		this.latencyText.text = "---";
	}
}
