using UnityEngine;
using System.Collections;

public class Level : MonoBehaviour {

    //关卡类型
	public enum LevelType
	{
		TIMER,
		OBSTACLE,
		MOVES,
	};

    //外部传入网格和游戏面板
	public Grid grid;
	public HUD hud;

    //游戏完成度对应需要多少分数
	public int score1Star;
	public int score2Star;
	public int score3Star;

    //当前关卡类型
	protected LevelType type;
	public LevelType Type {
		get { return type; }
	}

    //当前分数
	protected int currentScore;

    //当前关卡是否被完成
	protected bool didWin;

	// Use this for initialization
	void Start () {
		hud.SetScore (currentScore);
	}

    //游戏加分
    public virtual void OnPieceCleared(GamePiece piece)
    {
        currentScore += piece.score;
        hud.SetScore(currentScore);
    }

    //游戏步数
    public virtual void OnMove()
    {

    }

    //游戏胜利
    public virtual void GameWin()
	{
		grid.GameOver ();
		didWin = true;
		StartCoroutine (WaitForGridFill ());
	}
    //游戏失败
	public virtual void GameLose()
	{
		grid.GameOver ();
		didWin = false;
		StartCoroutine (WaitForGridFill ());
	}
    protected virtual IEnumerator WaitForGridFill()
    {
        while (grid.IsFilling)
        {
            yield return 0;
        }

        if (didWin)
        {
            hud.OnGameWin(currentScore);
        }
        else
        {
            hud.OnGameLose();
        }
    }
}
