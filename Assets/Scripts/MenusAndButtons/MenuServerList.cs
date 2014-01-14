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
		this.serverRows[0] = this.firstServerRow;
		for ( int i = 1; i < this.serverTableLength; ++i )
		{
			GameObject rowObj = GameObject.Instantiate( this.firstServerRow.gameObject ) as GameObject;
			this.serverRows[i] = rowObj.GetComponent<MenuServerRow>();
			rowObj.transform.Translate( 0.0f, -(float)i * this.serverRowHeight, 0.0f, Space.Self );
			rowObj.name = "ConnectionRow" + i;
			rowObj.transform.parent = this.firstServerRow.transform.parent;
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
		Debug.Log( "Refreshing server list." );
		MenuNetworking.instance.UpdateHostList();
		for ( int i = 0; i < this.serverTableLength; ++i )
		{
			this.serverRows[i].hostData = MenuNetworking.instance.GetHostData( i );
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
}
