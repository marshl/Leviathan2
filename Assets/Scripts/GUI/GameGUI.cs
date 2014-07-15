using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour 
{
	private GamePlayer player;

	public enum GUI_MODE
	{
		NONE,
		FIGHTER,
		FIGHTER_RESPAWNING,
		CAPITAL,
	};

	public GUI_MODE guiMode = GUI_MODE.NONE;

	public Texture healthBarTexture;
	public Texture shieldBarTexture;

	public Rect capitalShipHealthRect1;
	public Rect capitalShipHealthRect2;
	public Rect capitalShipShieldRect1;
	public Rect capitalShipShieldRect2;

	private void Update () 
	{
		if ( this.player == null )
		{
			this.player = GamePlayerManager.instance.GetMe();
		}

		if ( this.player != null )
		{
			if ( this.player.fighter != null )
			{
				this.guiMode = this.player.fighter.state == FighterMaster.FIGHTERSTATE.DEAD
					? GUI_MODE.FIGHTER_RESPAWNING : GUI_MODE.FIGHTER;
			}
			else if ( this.player.capitalShip != null )
			{
				this.guiMode = GUI_MODE.CAPITAL;
			}
			else
			{
				this.guiMode = GUI_MODE.NONE;
			}
		}
		else
		{
			this.guiMode = GUI_MODE.NONE;
		}

		switch ( this.guiMode )
		{
		case GUI_MODE.NONE:
		{
			return;
		}
		case GUI_MODE.FIGHTER:
		{
			this.UpdateCapitalShipDisplay();
			break;
		}
		case GUI_MODE.FIGHTER_RESPAWNING:
		{
			break;
		}
		case GUI_MODE.CAPITAL:
		{
			this.UpdateCapitalShipDisplay();
			break;
		}
		default:
		{
			DebugConsole.Error( "Uncaught GUIMODE state " + this.guiMode );
			break;
		}
		}
	}

	private void UpdateCapitalShipDisplay()
	{
		GamePlayer p1 = GamePlayerManager.instance.commander1;
		if ( p1 != null && p1.capitalShip != null )
		{
			CapitalHealth health = p1.capitalShip.health;
			float healthRatio = health.currentHealth / health.maxHealth;
			float shieldRatio = health.currentShield / health.maxShield;

			healthRatio = healthRatio > 0.0f ? healthRatio : 0.0f;
			shieldRatio = shieldRatio > 0.0f ? shieldRatio : 0.0f;

			this.capitalShipHealthRect1.Set( 5, 5, healthRatio * (Screen.width * 0.5f-10), 5 );
			this.capitalShipShieldRect1.Set( 5, 10, shieldRatio * (Screen.width * 0.5f-10), 5 );
		}
		GamePlayer p2  = GamePlayerManager.instance.commander2;
		if ( p2 != null && p2.capitalShip != null )
		{
			CapitalHealth health = p2.capitalShip.health;
			float healthRatio = health.currentHealth / health.maxHealth;
			float shieldRatio = health.currentShield / health.maxShield;

			healthRatio = healthRatio > 0.0f ? healthRatio : 0.0f;
			shieldRatio = shieldRatio > 0.0f ? shieldRatio : 0.0f;

			this.capitalShipHealthRect2.Set( Screen.width * 0.5f+5, 5, healthRatio * (Screen.width * 0.5f - 10), 5 );
			this.capitalShipShieldRect2.Set( Screen.width * 0.5f+5, 10, shieldRatio * (Screen.width * 0.5f - 10), 5 );
		}
	}

	private void RenderCapitalShipDisplay()
	{
		if ( GamePlayerManager.instance.commander1 != null )
		{
			this.RenderCapitalShipHealth( GamePlayerManager.instance.commander1.capitalShip );
		}
		if ( GamePlayerManager.instance.commander2 != null )
		{
			this.RenderCapitalShipHealth( GamePlayerManager.instance.commander2.capitalShip );
		}
	}

	private void RenderCapitalShipHealth( CapitalShipMaster _ship )
	{
		GUI.DrawTexture( this.capitalShipHealthRect1, this.healthBarTexture ); 
		GUI.DrawTexture( this.capitalShipHealthRect2, this.healthBarTexture ); 

		GUI.DrawTexture( this.capitalShipShieldRect1, this.shieldBarTexture ); 
		GUI.DrawTexture( this.capitalShipShieldRect2, this.shieldBarTexture ); 
	}

	private void RenderFighterTargets()
	{
		foreach ( TargetManager.Target target in this.player.fighter.weapons.targets )
		{
			Vector3 worldPos = target.health.transform.position;

			Vector3 screenPos = Camera.main.WorldToScreenPoint( worldPos );

			GUI.DrawTexture( new Rect( screenPos.x, screenPos.y, 5, 5 ), this.healthBarTexture );
		}
	}

	private void OnGUI()
	{
		switch ( this.guiMode )
		{
		case GUI_MODE.NONE:
		{
			return;
		}
		case GUI_MODE.FIGHTER:
		{
			this.RenderFighterTargets();
			this.RenderCapitalShipDisplay();
			break;
		}
		case GUI_MODE.FIGHTER_RESPAWNING:
		{
			break;
		}
		case GUI_MODE.CAPITAL:
		{
			this.RenderCapitalShipDisplay();
			break;
		}
		default:
		{
			DebugConsole.Error( "Uncaught GUIMODE state " + this.guiMode );
			break;
		}
		}
	}
}
