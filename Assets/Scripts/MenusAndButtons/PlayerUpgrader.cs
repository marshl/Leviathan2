using UnityEngine;
using System.Collections;

public class PlayerUpgrader : MonoBehaviour {

	public float[] speedMultipliers = {1.0f, 1.25f, 1.5f, 1.75f};
	public float[] defenseMultipliers = {1.0f, 1.25f, 1.5f, 1.75f};
	public float[] energyMultipliers = {1.0f, 1.25f, 1.5f, 1.75f};
	public int[] speedCosts = {0, 150, 500, 1750};
	public int[] defenseCosts = {0, 150, 500, 1750};
	public int[] energyCosts = {0, 150, 500, 1750};

	// The allocated levels. This is LOCAL - real data is in the player class.
	public int speedLevel = 0;
	public int defenseLevel = 0;
	public int energyLevel = 0;

	public GameObject speedHull;
	public GameObject defenseHull;
	public GameObject weaponHull;

	public static PlayerUpgrader instance;

	void Awake()
	{
		instance = this;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetSpeedUpgrade(GamePlayer _player, int _level)
	{

		if(ScoreManager.instance.CanAllocatePoints(speedCosts[_level], _player))
		{
			speedLevel = _level;
			UpdateCosts(_player);
		}
		else
		{
			//NOT ENOUGH PYLONS
		}
	}
	public void SetDefenseUpgrade(GamePlayer _player, int _level)
	{

		if(ScoreManager.instance.CanAllocatePoints(defenseCosts[_level], _player))
		{
			defenseLevel = _level;
			UpdateCosts(_player);
		}
	}

	public void SetEnergyUpgrade(GamePlayer _player, int _level)
	{
		if(ScoreManager.instance.CanAllocatePoints(energyCosts[_level], _player))
		{
			energyLevel = _level;
			UpdateCosts(_player);
		}
	}

	public void UpdateLevels(GamePlayer _player) //We'll call this from the interface when done
	{
		_player.energyMultiplier = energyMultipliers[energyLevel];
		_player.defenseMultiplier = defenseMultipliers[defenseLevel];
		_player.speedMultiplier = speedMultipliers[speedLevel];

		if(_player.fighter != null)
		{
			//speed - currently increases acceleration and maximum speed
			_player.fighter.movement.acceleration = _player.fighter.movement.baseAcceleration * speedMultipliers[speedLevel];
			_player.fighter.movement.maxSpeed = _player.fighter.movement.baseMaxSpeed * speedMultipliers[speedLevel];
			//energy - currently this increases regen rate and maximum energy
			_player.fighter.energySystem.maximumEnergy = _player.fighter.energySystem.baseMaxEnergy * energyMultipliers[energyLevel];
			_player.fighter.energySystem.rechargePerSecond = _player.fighter.energySystem.baseRechargePerSec * energyMultipliers[energyLevel];
			//defense - currently increases hull and shields
			_player.fighter.health.maxShield = _player.fighter.health.baseShield * defenseMultipliers[defenseLevel];
			_player.fighter.health.maxHealth = _player.fighter.health.baseHealth * defenseMultipliers[defenseLevel];
		}

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendUpgradedMessage (_player.id, _player.speedMultiplier, _player.defenseMultiplier, _player.energyMultiplier);
		}

	}

	

	public void UpdateCosts(GamePlayer _player)
	{
		ScoreManager.instance.GetTeamScore(_player.team).allocated = GetTotalCost();
		//UpdateLevels(_player);
	}

	public int GetTotalCost()
	{
		return speedCosts[speedLevel] + defenseCosts[defenseLevel] + energyCosts[energyLevel];
	}
}
