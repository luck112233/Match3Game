using UnityEngine;
using System.Collections;

public class GameOver : MonoBehaviour {

    //结算面板
	public GameObject screenParent;
    //分数面板
	public GameObject scoreParent;
	public UnityEngine.UI.Text scoreText;
    //失败提示
    public UnityEngine.UI.Text loseText;
    //胜利时的星星数
    public UnityEngine.UI.Image[] stars;

	//先将小结算面板和星星数隐藏
	void Start () {
		screenParent.SetActive (false);

		for (int i = 0; i < stars.Length; i++) {
			stars [i].enabled = false;
		}
	}

    //失败时仅仅播放动画
	public void ShowLose()
	{
		screenParent.SetActive (true);
		scoreParent.SetActive (false);

        loseText.enabled = true;

        Animator animator = GetComponent<Animator> ();
		if (animator) {
			animator.Play ("GameOverShow");
		}
	}

    //胜利时播放动画显示星星和得分
	public void ShowWin(int score, int starCount)
	{
		screenParent.SetActive (true);
        scoreParent.SetActive(true);

        scoreText.enabled = false;
        scoreText.text = score.ToString();

        loseText.enabled = false;
        
		
		Animator animator = GetComponent<Animator> ();
		if (animator) {
			animator.Play ("GameOverShow");
		}
		StartCoroutine (ShowWinCoroutine (starCount));
	}

    //缓慢显示星星数和得分
	private IEnumerator ShowWinCoroutine(int starCount)
	{
		yield return new WaitForSeconds (0.5f);

		if (starCount < stars.Length) {
			for (int i = 0; i <= starCount; i++) {
				stars [i].enabled = true;

				if (i > 0) {
					stars [i - 1].enabled = false;
				}

				yield return new WaitForSeconds (0.5f);
			}
		}

		scoreText.enabled = true;
	}

    //点击再来一次,从新加载本场景
	public void OnReplayClicked()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name);
	}

    //点击下一步进入关卡选择场景
	public void OnDoneClicked()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene ("levelSelect");
	}
}
