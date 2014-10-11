using UnityEngine;
using System.Collections;

public class GameMessages : MonoBehaviour
{
	public static GameMessages instance;

	public bool showingMessage = false;
	public string message = "";

	private void Awake()
	{
		GameMessages.instance = this;
	}

	private void OnGUI()
	{
		int index = 1;
		for ( int i = MessageManager.instance.messages.Count - 1; i >= 0; --i )
		{
			GUI.Label( new Rect( 0, Screen.height - 25 - index * 25, 500, 25 ), MessageManager.instance.GetFormattedMessage( i ) );
			++index;
		}

		if ( !this.showingMessage )
		{
			if ( Event.current.keyCode == KeyCode.Return )
			{
				this.showingMessage = true;
			}
		}
		else
		{
			GUI.SetNextControlName( "ChatEnterField" );
			this.message = GUI.TextField( new Rect( 0, Screen.height - 25, 400, 25 ), this.message );

			if ( Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "ChatEnterField" )
			{
				if ( this.message != string.Empty )
				{
					GameNetworkManager.instance.SendLobbyMessage( this.message, true );
					this.message = "";
				}
				this.showingMessage = false;
			}
		}
	}
}
