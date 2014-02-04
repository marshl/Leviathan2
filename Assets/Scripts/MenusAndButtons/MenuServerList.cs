using UnityEngine;
using System.Collections;

public class MenuServerList : MonoBehaviour
{
	public static MenuServerList instance;

	public MenuServerRow firstServerRow;
	public int serverTableLength;
	public float serverRowHeight;
	private MenuServerRow[] serverRows; 

	public float hostRefreshRate = 1.0f;
	private float hostRefreshTimer = 0.0f;

	private void Awake()
	{
		MenuServerList.instance = this;

		this.serverRows = new MenuServerRow[this.serverTableLength];
		for ( int i = 0; i < this.serverTableLength; ++i )
		{
			GameObject rowObj;
			if ( i == 0 )
			{
				rowObj = this.firstServerRow.gameObject;
			}
			else
			{
				rowObj = GameObject.Instantiate( this.firstServerRow.gameObject ) as GameObject;
			}

			this.serverRows[i] = rowObj.GetComponent<MenuServerRow>();
			rowObj.transform.Translate( 0.0f, -(float)i * this.serverRowHeight, 0.0f, Space.Self );
			rowObj.name = "ConnectionRow" + i;
			rowObj.transform.parent = this.firstServerRow.transform.parent;

			GUIEventMessage messageHandler = rowObj.GetComponent<GUIEventMessage>();
			messageHandler.value = i.ToString();
		}
	}

	private void Update()
	{
		this.hostRefreshTimer += Time.deltaTime;

		if ( this.hostRefreshTimer >= this.hostRefreshRate )
		{
			this.hostRefreshTimer = 0.0f;
			this.RefreshServerList();
		}
	}

	private void RefreshServerList()
	{
		//Debug.Log( "Refreshing server list." );
		MenuNetworking.instance.UpdateHostList();
		for ( int i = 0; i < this.serverTableLength; ++i )
		{
			//this.serverRows[i].hostData = MenuNetworking.instance.GetHostData( i );
			if ( MenuNetworking.instance.GetHostData(i) != null )
			{
				this.serverRows[i].hostDataIndex = i;
			}
			else
			{
				this.serverRows[i].hostDataIndex = -1;
			}
		}
		this.RefreshGUI();
	}

	public void StartJoinMenu()
	{
		this.RefreshServerList();
		this.RefreshGUI();
	}

	private void RefreshGUI()
	{
		foreach ( MenuServerRow serverRow in this.serverRows )
		{
			serverRow.UpdateGUI();
		}
	}

	private void OnServerRowDown( string _arg )
	{
		int index = 0;
		if ( int.TryParse( _arg, out index ) == false )
		{
			Debug.LogError( "Cannot convert argument into int \"" + _arg + "\"", this );
			return;
		}
	
		if ( index < 0 || index >= this.serverRows.Length )
		{
			Debug.LogError( "Server index out of range \"" + index + "\"", this );
			return;
		}
		//MenuServerRow serverRow = this.serverRows[ _index ];

		if ( MenuNetworking.instance.IsValidHostIndex( index ) == false )
		{
			return;
		}

		MenuNetworking.instance.ConnectToHostIndex( index );
		MainMenuButtons.instance.OpenConnectingWindow();
	}
}
