using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour 
{
	//private FighterMaster fighterMaster;
	//private CapitalShipMaster capitalShipMaster;
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

	//public Rect CapitalShipHealth

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
			break;
		}
		case GUI_MODE.FIGHTER_RESPAWNING:
		{
			break;
		}
		case GUI_MODE.CAPITAL:
		{
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
		float healthRatio = _ship.health.currentHealth / _ship.health.maxHealth;
		float shieldRatio = _ship.health.currentShield / _ship.health.maxShield;
		if ( healthRatio > 0.0f )
		{
			if ( _ship.health.team == TEAM.TEAM_1 )
			{
				GUI.DrawTexture( new Rect( 5, 5, healthRatio * Screen.width * 0.5f, 10 ), this.healthBarTexture ); 
			}
			else
			{
				GUI.DrawTexture( new Rect( Screen.width * 0.5f, 5, healthRatio * Screen.width * 0.5f - 5, 10 ), this.healthBarTexture ); 
			}
		}
		if ( shieldRatio > 0.0f )
		{
			if ( _ship.health.team == TEAM.TEAM_1 )
			{
				GUI.DrawTexture( new Rect( 5, 5, shieldRatio * Screen.width * 0.5f, 10 ), this.shieldBarTexture ); 
			}
			else
			{
				GUI.DrawTexture( new Rect( Screen.width * 0.5f+5, 5, shieldRatio * Screen.width * 0.5f, 10 ), this.shieldBarTexture ); 
			}
		}
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
