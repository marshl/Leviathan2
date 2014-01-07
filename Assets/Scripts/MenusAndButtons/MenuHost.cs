using UnityEngine;
using System.Collections;

public class MenuHost : MonoBehaviour
{
	// Instance
	public static MenuHost instance;

	// Public Variables
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

