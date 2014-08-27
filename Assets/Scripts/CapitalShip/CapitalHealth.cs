using UnityEngine;
using System.Collections;

public class CapitalHealth : BaseHealth 
{
	public int shieldGenerators;
	public float baseShields; //Our base shields, the capital ship gets this * shieldGenerators final max shield
	public bool overrideShieldGenerators = false;

	public void Awake()
	{
		if(!this.overrideShieldGenerators)
		{
			ShieldGeneratorHealth[] generators = this.GetComponentsInChildren<ShieldGeneratorHealth>();

			shieldGenerators = generators.GetLength(0);

			DebugConsole.Log ("Capital shield generator total is " + shieldGenerators.ToString());
		}

		this.RecalculateMaxShields();
	}

	public void RecalculateMaxShields()
	{
		this.maxShield = this.baseShields * shieldGenerators;

		if(this.currentShield > maxShield)
		{
			this.currentShield = maxShield;
		}
	}
	
	private void OnGUI()
	{
		if ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected )
		{
			GUI.Label( new Rect(0, 50, 150, 50), "Shields: " + this.currentShield + " / " + this.maxShield );
			GUI.Label( new Rect(0, 0, 150, 50), "Hull: " + this.currentHealth + " / " + this.maxHealth );
		}
	} 
}
