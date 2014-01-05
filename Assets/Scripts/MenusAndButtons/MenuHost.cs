using UnityEngine;
using System.Collections;

public class MenuHost : MonoBehaviour
{
	public static MenuHost instance;

	public int numConnections;
	public int portNumber;
	public bool useNat;

	private void Awake()
	{
		MenuHost.instance = this;
	}

	public void InitialiseServer()
	{
		NetworkConnectionError result = Network.InitializeServer( this.numConnections, this.portNumber, this.useNat );
		Debug.Log( result.ToString() );
	}

	public void OnGUI()
	{
	}
}

