using UnityEngine;
using System.Collections;

public class PlayerUpgrader : MonoBehaviour {

	public float[] speedMultipliers = {1.0f, 1.25f, 1.5f, 1.75f};
	public float[] defenseMultipliers = {1.0f, 1.25f, 1.5f, 1.75f};
	public float[] energyMultipliers = {1.0f, 1.25f, 1.5f, 1.75f};
	public int[] speedCosts = {0, 150, 500, 1750};
	public int[] defenseCosts = {0, 150, 500, 1750};
	public int[] energyCosts = {0, 150, 500, 1750};

	// The amount of tech points that have been allocated to each category
	// This is important for when we downgrade
	public int speedLevel = 0;
	public int defenseLevel = 0;
	public int energyLevel = 0;

	public GameObject speedHull;
	public GameObject defenseHull;
	public GameObject weaponHull;

	public PlayerUpgrader instance;

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

		GameNetworkManager.instance.SendUpgradedMessage (_player.id, _player.speedMultiplier, _player.defenseMultiplier, _player.energyMultiplier);
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
