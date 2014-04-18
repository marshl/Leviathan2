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

		if ( info == null )
		{
			GameObject obj = new GameObject();
			info = obj.AddComponent<MenuToGameInfo>();
			info.UseDefaults();

			Debug.LogWarning( "No menu to game info found. Using default values", info );
		}
		int networkID = Common.NetworkID();
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
	
	public void SendShootBulletMessage( BULLET_TYPE _bulletType, 
	                                   int _index,
	                                   Vector3 _pos, Vector3 _forward )
	{
		this.networkView.RPC( "OnShootBulletRPC", RPCMode.Others, Common.NetworkID(), (float)Network.time, (int)_bulletType, _index, _pos, _forward );
	}

	[RPC]
	private void OnShootBulletRPC( int _ownerID, float _creationTime, int _bulletType,
	                              int _index,
	                              Vector3 _pos, Vector3 _forward )
	{
		BulletManager.instance.CreateBulletRPC( _ownerID, _creationTime, (BULLET_TYPE)_bulletType, _index, _pos, _forward );
	}

	public void SendDestroySmartBulletMessage( int _ownerID, BULLET_TYPE _bulletType, int _index )
	{
		Debug.Log( "Sending message to destroy" );
		this.networkView.RPC( "OnDestroySmartBulletRPC", RPCMode.Others, _ownerID, (int)_bulletType, _index );
	}

	[RPC]
	private void OnDestroySmartBulletRPC( int _ownerID, int _bulletType, int _index )
	{
		Debug.Log( "Received Destroy Message" );
		BulletManager.instance.DestroySmartBulletRPC( _ownerID, (BULLET_TYPE)_bulletType, _index );
	}

	public void SendDestroyDumbBulletMessage( BULLET_TYPE _bulletType, int _index )
	{
		this.networkView.RPC( "OnDestroyDumbBulletRPC", RPCMode.Others, (int)_bulletType, _index );
	}

	[RPC]
	private void OnDestroyDumbBulletRPC( int _bulletType, int _index )
	{
		BulletManager.instance.DestroyDumbBulletRPC( (BULLET_TYPE)_bulletType, _index );
	}

	public void SendDealDamageMessage( NetworkViewID _id, float _damage )
	{
		this.networkView.RPC( "OnDealDamageRPC", RPCMode.Others, _id, _damage );
	}

	[RPC]
	private void OnDealDamageRPC( NetworkViewID _id, float _damage )
	{
		TargetManager.instance.OnDealDamage( _id, _damage );
	}

	public void SendDockedMessage( NetworkViewID _id, int landedSlot )
	{
		this.networkView.RPC ( "OnFighterDockedRPC", RPCMode.Others, _id, landedSlot );
	}

	[RPC]
	private void OnFighterDockedRPC( NetworkViewID _id, int landedSlotID )
	{
		//_id.gameObject.transform.position = landedSlot.landedPosition.transform.position;
		//_id.gameObject.transform.rotation = landedSlot.landedPosition.transform.rotation;

		NetworkView dockingFighterView = NetworkView.Find (_id);

		GameObject dockingFighter = dockingFighterView.gameObject;

		DockingBay.DockingSlot landedSlot = TargetManager.instance.GetDockingSlotByID ( landedSlotID );

		dockingFighter.transform.position = landedSlot.landedPosition.transform.position;
		dockingFighter.transform.parent = landedSlot.landedPosition;

		dockingFighter.transform.rotation = landedSlot.landedPosition.transform.rotation;
		dockingFighter.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
		dockingFighter.GetComponent<NetworkPositionControl>().enabled = false;
		landedSlot.landedFighter = dockingFighter.GetComponent<Fighter>();

		print("Received docked RPC");
	}

	public void SendUndockedMessage( NetworkViewID _id, int landedSlot )
	{
		this.networkView.RPC ( "OnFighterUndockedRPC", RPCMode.Others, _id, landedSlot );
	}
	
	[RPC]
	private void OnFighterUndockedRPC( NetworkViewID _id, int landedSlotID )
	{
		NetworkView dockingFighterView = NetworkView.Find (_id);
		
		GameObject dockingFighter = dockingFighterView.gameObject;

		DockingBay.DockingSlot landedSlot = TargetManager.instance.GetDockingSlotByID ( landedSlotID );

		dockingFighter.transform.parent = null;
		dockingFighter.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
		dockingFighter.GetComponent<NetworkPositionControl>().enabled = true;
		//dockingFighter.GetComponent<NetworkPositionControl>().lerp = true;
		landedSlot.landedFighter = null;
	}

}
