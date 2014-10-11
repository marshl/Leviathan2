using UnityEngine;
using System.Collections;

[System.Obsolete]
public class MenuServerRow : MonoBehaviour
{
	// Editor Variables
	public GUIText gameNameText;
	public GUIText ipText;
	public GUIText portText;
	public GUIText pingText;

	// Public Variables
	//public HostData hostData;
	public int hostDataIndex;
	public int latency = 0;
	public Ping ping;

	private void Update()
	{
		if ( this.hostDataIndex != -1 )
		{
			// Constantly create a new ping and send it off when the old one is returned
			if ( this.ping != null )
			{
				if ( this.ping.isDone )
				{
					this.latency = this.ping.time;
					this.ping = null;
					this.UpdateGUI();
				}
			}
			else
			{
				HostData hostData = MenuNetworking.instance.GetHostData( this.hostDataIndex );
				if ( hostData == null )
				{
					this.hostDataIndex = -1;
				}
				else
				{
					this.ping = new Ping( string.Concat( hostData.ip ) );
				}
				//this.ping = new Ping( string.Concat( this.hostData.ip ) );
			}
		}
	}

	public void UpdateGUI()
	{
		//if ( this.hostData == null )
		if ( this.hostDataIndex == -1 )
		{
			this.gameNameText.text = "---";
			this.ipText.text = "---";
			this.portText.text = "---";
			this.pingText.text = "---";
			return;
		}

		HostData hostData = MenuNetworking.instance.GetHostData( this.hostDataIndex );

		if ( hostData == null )
		{
			this.hostDataIndex = -1;
			return;
		}

		this.gameNameText.text = hostData.gameName;
		this.ipText.text = string.Join( ".", hostData.ip );
		this.portText.text = hostData.port.ToString();

		this.pingText.text = this.latency.ToString() + "ms";
	}
}
