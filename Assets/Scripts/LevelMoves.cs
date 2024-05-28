using UnityEngine;
using System.Collections;

public class LevelMoves : Level {

    //限定步数和目标分数
	public int numMoves;
	public int targetScore;
    //当前步数
	private int movesUsed = 0;

	//初始化关卡
	void Start () {
		type = LevelType.MOVES;

		hud.SetLevelType (type);
		hud.SetScore (currentScore);
		hud.SetTarget (targetScore);
		hud.SetRemaining (numMoves);
	}
	
    //增加当前步数,减少剩余步数,走完时判断胜利还是失败
	public override void OnMove ()
	{
		movesUsed++;

		hud.SetRemaining (numMoves - movesUsed);

		if (numMoves - movesUsed == 0) {
			if (currentScore >= targetScore) {
				GameWin ();
			} else {
				GameLose ();
			}
		}
	}
}
