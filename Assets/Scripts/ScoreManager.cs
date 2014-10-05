using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamScore
{
	public int score;
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
	
	public Dictionary<TEAM, TeamScore> teamScores;

	public void Awake()
	{
		instance = this;

		this.teamScores = new Dictionary<TEAM, TeamScore>();
		this.teamScores.Add( TEAM.TEAM_1, new TeamScore() );
		this.teamScores.Add ( TEAM.TEAM_2, new TeamScore() );
	}

	public void AddScore( SCORE_TYPE _scoreType, GamePlayer _player, bool _broadcast )
	{
		TEAM team = _player.team;
		int points = this.GetScoreTypeAmount( _scoreType );

		if ( this.teamScores.ContainsKey( team ) )
		{
			this.teamScores[team].score += points;
		}

		_player.personalScore += points;

		DebugConsole.Log( "Player " + _player.id + " earned " + points + " points for " + _scoreType );

		if ( _broadcast && Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendAddScoreMessage( _scoreType, _player );
		}
	}

	public int GetTeamScore( TEAM _team )
	{
		return this.teamScores[_team].score;
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
			throw new System.ArgumentException( "Unknown score type \"" + _scoreType + "\"" );
		}
	}
}
