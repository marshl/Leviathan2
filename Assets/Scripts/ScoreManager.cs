using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TeamScore
{
	public int score;
	public int allocated;
}

public enum SCORE_TYPE
{
	FIGHTER_KILL,
	TURRET_KILL,
	MISSILE_KILL,
	DAMAGE,
};

public class ScoreManager : MonoBehaviour
{
	public static ScoreManager instance;

	public int fighterKillPoints;
	public int turretKillPoints;
	public int missileKillPoints;
	public int targetDamagePoints;
	
	public TeamScore team1Score;
	public TeamScore team2Score;

	public void Awake()
	{
		instance = this;

		team1Score = new TeamScore();
		team2Score = new TeamScore();
	}

	public void AddScore( SCORE_TYPE _scoreType, GamePlayer _player, bool _broadcast )
	{
		TEAM team = _player.team;
		int points = this.GetScoreTypeAmount( _scoreType );


		this.GetTeamScore( team ).score += points;

		_player.personalScore += points;

		DebugConsole.Log( "Player " + _player.id + " earned " + points + " points for " + _scoreType );

		if ( _broadcast && Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendAddScoreMessage( _scoreType, _player );
		}
	}

	public bool CanAllocatePoints(int _amount, GamePlayer _player)
	{
		TeamScore score = this.GetTeamScore (_player.team);

		if(score.allocated + _amount > score.score)
		{
			return false;
		}
		else
		{
			//score.allocated += _amount;
			return true;
		}
	}

	public bool CanDeallocatePoints(int _amount, GamePlayer _player)
	{
		TeamScore score = this.GetTeamScore (_player.team);
		
		if(score.allocated - _amount < 0)
		{
			return false;
		}
		else
		{
			//score.allocated -= _amount;
			return true;
		}
	}

	public TeamScore GetTeamScore( TEAM _team )
	{
		switch ( _team )
		{
		case TEAM.TEAM_1:
			return this.team1Score;
		case TEAM.TEAM_2:
			return this.team2Score;
		default:
			DebugConsole.Error( "Unknown team " + _team );
			return this.team1Score;
		}
	}

	private int GetScoreTypeAmount( SCORE_TYPE _scoreType )
	{
		switch ( _scoreType )
		{
		case SCORE_TYPE.FIGHTER_KILL:
			return this.fighterKillPoints;
		case SCORE_TYPE.MISSILE_KILL:
			return this.missileKillPoints;
		case SCORE_TYPE.TURRET_KILL:
			return this.turretKillPoints;
		case SCORE_TYPE.DAMAGE:
			return this.targetDamagePoints;
		default:
			DebugConsole.Error( "Unknown score type \"" + _scoreType + "\"" );
			return 0;
		}
	}
}
