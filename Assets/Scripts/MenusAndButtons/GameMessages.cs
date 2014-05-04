using UnityEngine;
using System.Collections;

public class GameMessages : MonoBehaviour
{
	public bool showingMessage = false;
	public string message = "";

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Return ) )
		{
			if ( this.showingMessage )
			{
				if ( this.message != "" )
				{
					//MessageManager.instance.AddMessage( Common.MyNetworkID(), this.message );
					GameNetworkManager.instance.SendLobbyMessage( this.message );
				}
				this.message = "";
			}

			this.showingMessage = !this.showingMessage;
		} 

		foreach ( char c in Input.inputString )
		{
			if ( c == '\b' )
			{
				if ( this.message.Length > 0 )
				{
					this.message = this.message.Substring( 0, this.message.Length - 1 );
				}
			}
			else      
			{
				this.message += c;
			}
		}
	}

	private void OnGUI()
	{
		if ( this.showingMessage )
		{
			GUI.Label( new Rect( 0, Screen.height - 25,200,25 ), this.message );
		}

		int index = 1;
		for ( int i = MessageManager.instance.messages.Count - 1; i >= 0; --i )
		{
			GUI.Label( new Rect( 0, Screen.height - 25 - index * 25, 500, 25 ), MessageManager.instance.GetFormattedMessage( i ) );
			++ index;
		}
	}
}
