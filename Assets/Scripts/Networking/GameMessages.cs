using UnityEngine;
using System.Collections;

public class GameMessages : MonoBehaviour
{
	public static GameMessages instance;

	public bool showingMessage = false;
	public bool typing = false;
	public string message = "";

	private void Awake()
	{
		GameMessages.instance = this;
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Return ) )
		{
			if ( typing )
			{
				if ( this.message != "" )
				{
					GameNetworkManager.instance.SendLobbyMessage( this.message, -1 );
				}
				this.message = "";

				this.typing = false;
			}
			else
			{
				this.showingMessage = true;
				this.typing = true;
			}
		} 

		if( Input.GetKeyDown( KeyCode.Y ) )
		{
			if ( !this.typing )
			{
				this.showingMessage = !this.showingMessage;
			}
		}

		if ( this.typing )
		{
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


	}

	private void OnGUI()
	{
		if ( this.showingMessage )
		{

			int index = 1;
			for ( int i = MessageManager.instance.messages.Count - 1; i >= 0; --i )
			{
				GUI.Label( new Rect( 0, Screen.height - 25 - index * 25, 500, 25 ), MessageManager.instance.GetFormattedMessage( i ) );
				++ index;
			}

			if(typing)
			{
				GUI.Label( new Rect( 0, Screen.height - 25,200,25 ), this.message );
			}
		}


	}
}
