using UnityEngine;
using System.Collections;

public class GamePiece : MonoBehaviour {

    //销毁时的得分
	public int score;

    //自己所处的位置和类型和网格类
	private int x;
	private int y;
    private Grid.PieceType type;
    private Grid grid;
    public int X
	{
		get { return x; }
		set {
			if (IsMovable ()) {
				x = value;
			}
		}
	}
	public int Y
	{
		get { return y; }
		set {
			if (IsMovable ()) {
				y = value;
			}
		}
	}
	public Grid.PieceType Type
	{
		get { return type; }
	}
	public Grid GridRef
	{
		get { return grid; }
	}

    //移动控件
	private MovablePiece movableComponent;
	public MovablePiece MovableComponent
	{
		get { return movableComponent; }
	}
    //是否可移动
    public bool IsMovable()
    {
        return movableComponent != null;
    }

    //颜色控件
    private ColorPiece colorComponent;
	public ColorPiece ColorComponent
	{
		get { return colorComponent; }
	}
    //是否有颜色
    public bool IsColored()
    {
        return colorComponent != null;
    }

    //消除控件
    private ClearablePiece clearableComponent;
	public ClearablePiece ClearableComponent {
		get { return clearableComponent; }
	}
    //是否可消除
    public bool IsClearable()
    {
        return clearableComponent != null;
    }

    void Awake()
	{
		movableComponent = GetComponent<MovablePiece> ();
		colorComponent = GetComponent<ColorPiece> ();
		clearableComponent = GetComponent<ClearablePiece> ();
	}

    //初始化
	public void Init(int _x, int _y, Grid _grid, Grid.PieceType _type)
	{
		x = _x;
		y = _y;
		grid = _grid;
		type = _type;
	}

    //按下时
    void OnMouseDown()
    {
        grid.PressPiece(this);
    }
    //滑动时
    void OnMouseEnter()
	{
		grid.EnterPiece (this);
	}
    //抬起时
	void OnMouseUp()
	{
		grid.ReleasePiece ();
	}
}
