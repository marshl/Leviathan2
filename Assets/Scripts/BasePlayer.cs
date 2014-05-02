using UnityEngine;
using System.Collections;

/*public enum PLAYER_TYPE : int
{
	COMMANDER1,
	COMMANDER2,
	FIGHTER1,
	FIGHTER2,
};
*/

[System.Obsolete]
public class BasePlayer
{
	public int id;

	public string name = "DEFAULT";
	public PLAYER_TYPE playerType;
}
