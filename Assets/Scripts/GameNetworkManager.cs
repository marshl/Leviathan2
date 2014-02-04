using UnityEngine;
using System.Collections;

public class GameNetworkManager : MonoBehaviour
{
	// Instance
	public static GameNetworkManager instance;

	// Editor Variables
	public GameObject playerPrefab;
	
	// Methods
	private void Awake()
	{
		GameNetworkManager.instance = this;

		GameObject obj = Network.Instantiate
		(
			this.playerPrefab, Vector3.zero, Quaternion.identity, 0
		) as GameObject;
	}

	private void OnDisconnectedFromServer()
	{
		Debug.Log( "Disconnected from server" );
		Application.LoadLevel( "MenuTest" );
	}
}
