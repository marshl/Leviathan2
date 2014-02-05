using UnityEngine;
using System.Collections;

public class GameNetworkManager : MonoBehaviour
{
	public static GameNetworkManager instance;
	
	public GameObject playerPrefab;
	
	// Methods
	private void Awake()
	{
		GameNetworkManager.instance = this;

		Network.Instantiate
		(
			this.playerPrefab, Common.RandomVector3( -10.0f, 10.0f ), Quaternion.identity, 0
		);
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
