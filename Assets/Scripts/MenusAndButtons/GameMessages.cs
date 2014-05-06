using UnityEngine;
using System.Collections;

public class GameMessages : MonoBehaviour
{
	public bool showingMessage = false;
	public static bool typing = false;
	public string message = "";

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Return ) )
		{
			if ( typing )
			{
				if ( this.message != "" )
				{
					//MessageManager.instance.AddMessage( Common.MyNetworkID(), this.message );
					GameNetworkManager.instance.SendLobbyMessage( this.message );
				}
				this.message = "";
				//this.showingMessage = false;
				typing = false;
			}
			else
			{
				this.showingMessage = true;
				typing = true;
			}
			 //= !this.showingMessage;
		} 

		if( Input.GetKeyDown( KeyCode.Y ))
		{
			if(!typing)
			{
				this.showingMessage = !this.showingMessage;
			}
		}

		if(typing)
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
