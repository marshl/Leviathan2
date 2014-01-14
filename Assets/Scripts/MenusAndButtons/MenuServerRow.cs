using UnityEngine;
using System.Collections;

public class MenuServerRow : MonoBehaviour
{
	// Editor Variables
	public GUIText gameNameText;
	public GUIText ipText;
	public GUIText portText;
	public GUIText pingText;

	// Public Variables
	public HostData hostData;
	public int latency = 0;
	public Ping ping;

	private void Update()
	{
		if ( this.hostData != null )
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
				this.ping = new Ping( string.Concat( this.hostData.ip ) );
			}
		}
	}

	public void UpdateGUI()
	{
		if ( this.hostData == null )
		{
			this.gameNameText.text = "---";
			this.ipText.text = "---";
			this.portText.text = "---";
			this.pingText.text = "---";
			return;
		}

		this.gameNameText.text = this.hostData.gameName;
		this.ipText.text = string.Join( " ", this.hostData.ip );
		this.portText.text = this.hostData.port.ToString();

		this.pingText.text = this.latency.ToString() + "ms";
	}
}
