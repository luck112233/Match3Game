using UnityEngine;
using System.Collections;

public class ClearablePiece : MonoBehaviour {

    //外部传入的清除动画
	public AnimationClip clearAnimation;

    //是否被清除
	private bool isBeingCleared = false;
	public bool IsBeingCleared {
		get { return isBeingCleared; }
	}

    //获取对象类
	protected GamePiece piece;

	void Awake() {
		piece = GetComponent<GamePiece> ();
	}

    //执行清除
	public virtual void Clear()
	{
		piece.GridRef.level.OnPieceCleared (piece);
		isBeingCleared = true;
		StartCoroutine (ClearCoroutine ());
	}
    //执行清除动画
	private IEnumerator ClearCoroutine()
	{
		Animator animator = GetComponent<Animator> ();
		if (animator) {
			animator.Play (clearAnimation.name);
			yield return new WaitForSeconds (clearAnimation.length);
			Destroy (gameObject);
		}
	}
}
