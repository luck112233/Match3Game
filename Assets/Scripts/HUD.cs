using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

    //外部传人关卡类和游戏结束类
	public Level level;
	public GameOver gameOver;
    //剩余步数或时间
	public UnityEngine.UI.Text remainingText;
	public UnityEngine.UI.Text remainingSubtext;
    //目标障碍物或分数
	public UnityEngine.UI.Text targetText;
	public UnityEngine.UI.Text targetSubtext;
    //当前分数
	public UnityEngine.UI.Text scoreText;
    //所得星星
	public UnityEngine.UI.Image[] stars;
    //默认当前星星
	private int starIdx = 0;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < stars.Length; i++) {
			if (i == starIdx) {
				stars [i].enabled = true;
			} else {
				stars [i].enabled = false;
			}
		}
	}
	
    //设置面板分数顺便设置星星数
	public void SetScore(int score)
	{
		scoreText.text = score.ToString ();

		int visibleStar = 0;

		if (score >= level.score1Star && score < level.score2Star) {
			visibleStar = 1;
		} else if (score >= level.score2Star && score < level.score3Star) {
			visibleStar = 2;
		} else if (score >= level.score3Star) {
			visibleStar = 3;
		}

		for (int i = 0; i < stars.Length; i++) {
			if (i == visibleStar) {
				stars [i].enabled = true;
			} else {
				stars [i].enabled = false;
			}
		}

		starIdx = visibleStar;
	}

    //设置目标分数
	public void SetTarget(int target)
	{
		targetText.text = target.ToString ();
	}
    //设置剩余步数或时间
	public void SetRemaining(int remaining)
	{
		remainingText.text = remaining.ToString ();
	}
    //设置剩余步数或时间
    public void SetRemaining(string remaining)
	{
		remainingText.text = remaining;
	}
    //设置游戏类型
	public void SetLevelType(Level.LevelType type)
	{
		if (type == Level.LevelType.MOVES) {
			remainingSubtext.text = "moves remaining";
			targetSubtext.text = "target score";
		} else if (type == Level.LevelType.OBSTACLE) {
			remainingSubtext.text = "moves remaining";
			targetSubtext.text = "bubbles remaining";
		} else if (type == Level.LevelType.TIMER) {
			remainingSubtext.text = "time remaining";
			targetSubtext.text = "target score";
		}
	}

    //游戏结束:胜利,并记下最高星星数
	public void OnGameWin(int score)
	{
		gameOver.ShowWin (score, starIdx);
		if (starIdx > PlayerPrefs.GetInt (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name, 0)) {
			PlayerPrefs.SetInt (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name, starIdx);
		}
	}

    //游戏结束:失败
	public void OnGameLose()
	{
		gameOver.ShowLose ();
	}
}
