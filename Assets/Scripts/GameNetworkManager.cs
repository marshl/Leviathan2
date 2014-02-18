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

	public void SendShootBulletMessage( BULLET_TYPE _bulletType, 
	                                   int _index,
	                                   Vector3 _pos, Vector3 _forward )
	{
		this.networkView.RPC( "OnShootBulletRPC", RPCMode.Others, (int)_bulletType, _index, _pos, _forward );
	}

	[RPC]
	private void OnShootBulletRPC( int _bulletType,
	                              int _index,
	                              Vector3 _pos, Vector3 _forward )
	{
		BulletManager.instance.CreateDumbBullet( (BULLET_TYPE)_bulletType, _index, _pos, _forward );
	}

	public void SendDestroyBulletMessage( BULLET_TYPE _bulletType, int _index )
	{
		this.networkView.RPC( "OnDestroyBulletRPC", RPCMode.Others, (int)_bulletType, _index );
	}

	[RPC]
	private void OnDestroyBulletRPC( int _bulletType, int _index )
	{
		BulletManager.instance.DestroyInactiveBullet( (BULLET_TYPE)_bulletType, _index );
	}
}
