using UnityEngine;
using System.Collections;

public class GameNetworkManager : MonoBehaviour
{
	public static GameNetworkManager instance;
	
	//public GameObject fighterPrefab;
	//public GameObject capitalShipPrefab;
	
	// Methods
	private void Awake()
	{
		GameNetworkManager.instance = this;
	}

	private void Start()
	{
		MenuToGameInfo info = MenuToGameInfo.instance;
		int networkID = Common.NetworkID( Network.player );
		info.Print();
		Debug.Log( "NetworkID: " + networkID );
		PLAYER_TYPE state = info.playerTypeMap[networkID];
		PlayerInstantiator.instance.CreatePlayerObject( state );
	}

	private void OnDisconnectedFromServer()
	{
		Debug.Log( "Disconnected from server" );
		Application.LoadLevel( "MenuTest" );
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Escape ) )
			Debug.Break();
	}
}
