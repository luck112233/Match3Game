using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

    //网格大小
    public int xDim;
	public int yDim;
    //获取网格背景
    public GameObject backgroundPrefab;
    //填充用时
    public float fillTime;
    //关卡了
	public Level level;
    //获取每种类型
	public PiecePrefab[] piecePrefabs;
    //设置特殊元素
	public PiecePosition[] initialPieces;

    //所有头像类型
    public enum PieceType
    {
        EMPTY, //空的
        NORMAL,//元素
        BUBBLE,//障碍物
        ROW_CLEAR,//消除整行
        COLUMN_CLEAR,//消除整列
        RAINBOW,//彩虹类型
        COUNT,
    };
    //对每种头像进行赋值
    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    };
    //设置特殊元素，比如障碍物
    [System.Serializable]
    public struct PiecePosition
    {
        public PieceType type;
        public int x;
        public int y;
    };


    //设置每种元素
    private Dictionary<PieceType, GameObject> piecePrefabDict;
    //所有元素
	private GamePiece[,] pieces;
    //滑动的开始和结束元素
	private GamePiece pressedPiece;
	private GamePiece enteredPiece;
    //是否正在填充
    private bool isFilling = false;
    //填充障碍物周边的方向
    private bool inverse = false;
    //游戏是否结束
    private bool gameOver = false;
    
	public bool IsFilling
	{
		get { return isFilling; }
	}
    //程序入口
	void Awake () {
        //存放元素及特殊元素类型的字典
		piecePrefabDict = new Dictionary<PieceType, GameObject> ();
		for (int i = 0; i < piecePrefabs.Length; i++) {
			if (!piecePrefabDict.ContainsKey (piecePrefabs [i].type)) {
				piecePrefabDict.Add (piecePrefabs [i].type, piecePrefabs [i].prefab);
			}
		}

        //存放面板所有元素
        pieces = new GamePiece[xDim, yDim];

        //产生网格背景
        for (int x = 0; x < xDim; x++) {
			for (int y = 0; y < yDim; y++) {
				GameObject background = (GameObject)Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
				background.transform.parent = transform;
			}
		}

        //产生特殊元素
        for (int i = 0; i < initialPieces.Length; i++) {
			if (initialPieces [i].x >= 0 && initialPieces [i].x < xDim
			    && initialPieces [i].y >= 0 && initialPieces [i].y < yDim) {
                SpawnNewPiece (initialPieces [i].x, initialPieces [i].y, initialPieces [i].type);
			}
		}
       
        //产生空元素
        for (int x = 0; x < xDim; x++) {
			for (int y = 0; y < yDim; y++) {
				if (pieces [x, y] == null) {
					SpawnNewPiece (x, y, PieceType.EMPTY);
				}
			}
		}
        //协程还是填充
		StartCoroutine(Fill ());
	}

    //获取世界坐标
    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - xDim / 2.0f + x,
            transform.position.y + yDim / 2.0f - y);

    }

    //创建元素
    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;
        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x, y, this, type);
        return pieces[x, y];
    }

	public IEnumerator Fill()
	{
        //填充状态
        isFilling = true;

        //需要被注满
        bool needsRefill = true;
        //需要注满时执行
		while (needsRefill) {
			yield return new WaitForSeconds (fillTime);
            //每个元素往下落一步等待一段时间，并确定是否还需要往下落
			while (FillStep ()) {
				inverse = !inverse;
				yield return new WaitForSeconds (fillTime);
			}
            //落满以后执行销毁，如果有销毁就继续往下落，直到落满后不销毁为止
			needsRefill = ClearAllValidMatches ();
		}

		isFilling = false;
	}

    //填充步骤，遍历每个元素往下移动一次
	public bool FillStep()
	{
        //是否有元素移动
		bool movedPiece = false;

        //从下往上
		for (int y = yDim-2; y >= 0; y--)
		{
            //默认从左往右
			for (int loopX = 0; loopX < xDim; loopX++)
			{
                //通过标签决定遍历方向，inverse 为true是从右往左，false为从左往右
				int x = loopX;
				if (inverse) {
					x = xDim - 1 - loopX;
				}

                //针对元素开始移动
				GamePiece piece = pieces [x, y];
				if (piece.IsMovable ())
				{                   
					GamePiece pieceBelow = pieces [x, y + 1];
                    //如果下方为空元素，则移动
					if (pieceBelow.Type == PieceType.EMPTY) {
						Destroy (pieceBelow.gameObject);
						piece.MovableComponent.Move (x, y + 1, fillTime);
						pieces [x, y + 1] = piece;
						SpawnNewPiece (x, y, PieceType.EMPTY);
						movedPiece = true;
					} else {
                        //如果下方是障碍物或元素则不能移动
						for (int diag = -1; diag <= 1; diag++)
						{
                            //-1,1是左右方向
							if (diag != 0)
							{
								int diagX = x + diag;
                                //先看左边元素还是先看右边元素
								if (inverse)
								{
									diagX = x - diag;
								}
                                //遍历当前元素的左右两边，排除左边越界和右边越界
								if (diagX >= 0 && diagX < xDim)
								{
									GamePiece diagonalPiece = pieces [diagX, y + 1];
                                    //如果左下角或右下角为空元素则继续，否则到这里就结束了
									if (diagonalPiece.Type == PieceType.EMPTY)
									{
										bool hasPieceAbove = true;
                                        //寻找左下角或右下角的同一Y轴上是否有障碍物
										for (int aboveY = y; aboveY >= 0; aboveY--)
										{
											GamePiece pieceAbove = pieces [diagX, aboveY];
											if (pieceAbove.IsMovable ())
											{
												break;
											}
											else if(!pieceAbove.IsMovable() && pieceAbove.Type != PieceType.EMPTY)
											{
												hasPieceAbove = false;
												break;
											}
										}
                                        //如果有障碍物，则当前元素移动到左下或右下 如果没有障碍物就不处理，到这里就结束了（因为没有障碍物左下或右下的填充由它正上方的元素填充，轮不到当前元素）
                                        if (!hasPieceAbove)
										{
											Destroy (diagonalPiece.gameObject);
											piece.MovableComponent.Move (diagX, y + 1, fillTime);
											pieces [diagX, y + 1] = piece;
											SpawnNewPiece (x, y, PieceType.EMPTY);
											movedPiece = true;
											break;
										}
									} 
								}
							}
						}
					}
				}
			}
		}
        //填充最上层元素
		for (int x = 0; x < xDim; x++)
		{         
			GamePiece pieceBelow = pieces [x, 0];
            //如果最上层元素为空，则创建不存在的-1层下落到最上层元素
			if (pieceBelow.Type == PieceType.EMPTY)
			{
				Destroy (pieceBelow.gameObject);
				GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);
				newPiece.transform.parent = transform;

				pieces [x, 0] = newPiece.GetComponent<GamePiece> ();
				pieces [x, 0].Init (x, -1, this, PieceType.NORMAL);
                pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors));
                pieces [x, 0].MovableComponent.Move (x, 0, fillTime);
				movedPiece = true;
			}
		}
        //返回有元素移动
		return movedPiece;
	}

    //遍历每个元素是否可以清除
    public bool ClearAllValidMatches()
    {
        //是否需要填充
        bool needsRefill = false;
        //每个元素挨个遍历
        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (pieces[x, y].IsClearable())
                {
                    //获取每个元素匹配队列
                    List<GamePiece> match = GetMatch(pieces[x, y], x, y);
                    //匹配队列存在
                    if (match != null)
                    {
                        //定义特殊元素，默认为队列的随机一个
                        PieceType specialPieceType = PieceType.COUNT;
                        GamePiece randomPiece = match[Random.Range(0, match.Count)];
                        int specialPieceX = randomPiece.X;
                        int specialPieceY = randomPiece.Y;
                        //如果是四个相邻
                        if (match.Count == 4)
                        {
                            //判断是刚开始产生的还是滑动产生的，来确定类型
                            if (pressedPiece == null || enteredPiece == null)
                            {
                                specialPieceType = (PieceType)Random.Range((int)PieceType.ROW_CLEAR, (int)PieceType.COLUMN_CLEAR);
                            }
                            else if (pressedPiece.Y == enteredPiece.Y)
                            {
                                specialPieceType = PieceType.ROW_CLEAR;
                            }
                            else
                            {
                                specialPieceType = PieceType.COLUMN_CLEAR;
                            }
                        }//如果是五个相邻类型为彩虹
                        else if (match.Count >= 5)
                        {
                            specialPieceType = PieceType.RAINBOW;
                        }
                        //在销毁时设置填充为true，并确定特殊元素位置
                        for (int i = 0; i < match.Count; i++)
                        {
                            //销毁成功执行if语句
                            if (ClearPiece(match[i].X, match[i].Y))
                            {
                                needsRefill = true;
                                if (match[i] == pressedPiece || match[i] == enteredPiece)
                                {
                                    specialPieceX = match[i].X;
                                    specialPieceY = match[i].Y;
                                }
                            }
                        }

                        //如果特殊元素存在
                        if (specialPieceType != PieceType.COUNT)
                        {
                            //销毁原来的元素并产生特殊元素类型
                            Destroy(pieces[specialPieceX, specialPieceY]);
                            GamePiece newPiece = SpawnNewPiece(specialPieceX, specialPieceY, specialPieceType);
                            //将元素颜色设置为匹配队列的第一个元素，或者彩虹
                            if ((specialPieceType == PieceType.ROW_CLEAR || specialPieceType == PieceType.COLUMN_CLEAR)
                                && newPiece.IsColored() && match[0].IsColored())
                            {
                                newPiece.ColorComponent.SetColor(match[0].ColorComponent.Color);
                            }
                            else if (specialPieceType == PieceType.RAINBOW && newPiece.IsColored())
                            {
                                newPiece.ColorComponent.SetColor(ColorPiece.ColorType.ANY);
                            }
                        }
                    }
                }
            }
        }
        //返回是否可以填充
        return needsRefill;
    }

    //获取指定元素在指定位置的匹配队列
    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (piece.IsColored())
        {
            ColorPiece.ColorType color = piece.ColorComponent.Color;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();
            // First check horizontal
            //首先检查水平
            horizontalPieces.Add(piece);
            //找到横向的相连的相同颜色的元素
            //0,1代表左右方向
            for (int dir = 0; dir <= 1; dir++)
            {
                //xoffset代表与指定位置的距离
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;
                    if (dir == 0)
                    { // Left
                        x = newX - xOffset;
                    }
                    else
                    { // Right
                        x = newX + xOffset;
                    }
                    if (x < 0 || x >= xDim)
                    {
                        break;
                    }
                    //相邻并且颜色相同进队列，只要找到一个不一样的或者超出网格的就结束
                    if (pieces[x, newY].IsColored() && pieces[x, newY].ColorComponent.Color == color)
                    {
                        horizontalPieces.Add(pieces[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //如果大于三添加到匹配队列
            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }
            // Traverse vertically if we found a match (for L and T shapes)
            //如果我们找到一个匹配（L型和T形）
            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int yOffset = 1; yOffset < yDim; yOffset++)
                        {
                            int y;
                            if (dir == 0)
                            { // Up
                                y = newY - yOffset;
                            }
                            else
                            { // Down
                                y = newY + yOffset;
                            }
                            if (y < 0 || y >= yDim)
                            {
                                break;
                            }
                            if (pieces[horizontalPieces[i].X, y].IsColored() && pieces[horizontalPieces[i].X, y].ColorComponent.Color == color)
                            {
                                verticalPieces.Add(pieces[horizontalPieces[i].X, y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (verticalPieces.Count < 2)
                    {
                        verticalPieces.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < verticalPieces.Count; j++)
                        {
                            matchingPieces.Add(verticalPieces[j]);
                        }
                        break;
                    }
                }
            }
            //匹配队列大于3则返回
            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            // Didn't find anything going horizontally first,
            //没有找到任何水平先行，
            // so now check vertically
            //所以现在检查垂直
            //以下同理不在注释
            horizontalPieces.Clear();
            verticalPieces.Clear();
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { // Up
                        y = newY - yOffset;
                    }
                    else
                    { // Down
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }

                    if (pieces[newX, y].IsColored() && pieces[newX, y].ColorComponent.Color == color)
                    {
                        verticalPieces.Add(pieces[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            // Traverse horizontally if we found a match (for L and T shapes)
            //如果我们发现匹配（对于L和T形状），横向横向运行
            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int xOffset = 1; xOffset < xDim; xOffset++)
                        {
                            int x;

                            if (dir == 0)
                            { // Left
                                x = newX - xOffset;
                            }
                            else
                            { // Right
                                x = newX + xOffset;
                            }

                            if (x < 0 || x >= xDim)
                            {
                                break;
                            }

                            if (pieces[x, verticalPieces[i].Y].IsColored() && pieces[x, verticalPieces[i].Y].ColorComponent.Color == color)
                            {
                                horizontalPieces.Add(pieces[x, verticalPieces[i].Y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (horizontalPieces.Count < 2)
                    {
                        horizontalPieces.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < horizontalPieces.Count; j++)
                        {
                            matchingPieces.Add(horizontalPieces[j]);
                        }

                        break;
                    }
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }

        return null;
    }

    //清除指定坐标元素
    public bool ClearPiece(int x, int y)
    {
        if (pieces[x, y].IsClearable() && !pieces[x, y].ClearableComponent.IsBeingCleared)
        {
            pieces[x, y].ClearableComponent.Clear();
            SpawnNewPiece(x, y, PieceType.EMPTY);
            ClearObstacles(x, y);
            return true;
        }
        return false;
    }

    //清除指定坐标临近障碍物
    public void ClearObstacles(int x, int y)
    {
        //清除左右障碍物
        for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
        {
            if (adjacentX != x && adjacentX >= 0 && adjacentX < xDim)
            {
                if (pieces[adjacentX, y].Type == PieceType.BUBBLE && pieces[adjacentX, y].IsClearable())
                {
                    pieces[adjacentX, y].ClearableComponent.Clear();
                    SpawnNewPiece(adjacentX, y, PieceType.EMPTY);
                }
            }
        }
        //清除上下障碍物
        for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
        {
            if (adjacentY != y && adjacentY >= 0 && adjacentY < yDim)
            {
                if (pieces[x, adjacentY].Type == PieceType.BUBBLE && pieces[x, adjacentY].IsClearable())
                {
                    pieces[x, adjacentY].ClearableComponent.Clear();
                    SpawnNewPiece(x, adjacentY, PieceType.EMPTY);
                }
            }
        }
    }

    //按下时执行
    public void PressPiece(GamePiece piece)
    {
        pressedPiece = piece;
    }

    //滑动时执行
    public void EnterPiece(GamePiece piece)
    {
        enteredPiece = piece;
    }

    //释放时执行
    public void ReleasePiece()
    {
        if (IsAdjacent(pressedPiece, enteredPiece))
        {
            SwapPieces(pressedPiece, enteredPiece);
        }
    }

    //判断两个元素是否相邻
    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
	{
		return (piece1.X == piece2.X && (int)Mathf.Abs (piece1.Y - piece2.Y) == 1)
		|| (piece1.Y == piece2.Y && (int)Mathf.Abs (piece1.X - piece2.X) == 1);
	}

    //两个指定元素交换位置
	public void SwapPieces(GamePiece piece1, GamePiece piece2)
	{
        //游戏结束不执行
		if (gameOver) {
			return;
		}
        //假如都能移动
		if (piece1.IsMovable () && piece2.IsMovable ()) {
            //先将缓存刷新
			pieces [piece1.X, piece1.Y] = piece2;
			pieces [piece2.X, piece2.Y] = piece1;
            //两元素在交换位置之后有匹配队列或者是彩虹，则往下执行
			if (GetMatch (piece1, piece2.X, piece2.Y) != null || GetMatch (piece2, piece1.X, piece1.Y) != null
				|| piece1.Type == PieceType.RAINBOW || piece2.Type == PieceType.RAINBOW) {
                //将两元素位置置换
				int piece1X = piece1.X;
				int piece1Y = piece1.Y;
				piece1.MovableComponent.Move (piece2.X, piece2.Y, fillTime);
				piece2.MovableComponent.Move (piece1X, piece1Y, fillTime);
                //如果是彩虹类型则清除指定颜色元素
				if (piece1.Type == PieceType.RAINBOW && piece1.IsClearable () && piece2.IsColored ()) {
					ClearColorPiece clearColor = piece1.GetComponent<ClearColorPiece> ();
					if (clearColor) {
						clearColor.Color = piece2.ColorComponent.Color;
					}

					ClearPiece (piece1.X, piece1.Y);
				}
                //如果是彩虹类型则清除指定颜色元素
                if (piece2.Type == PieceType.RAINBOW && piece2.IsClearable () && piece1.IsColored ()) {
					ClearColorPiece clearColor = piece2.GetComponent<ClearColorPiece> ();
					if (clearColor) {
						clearColor.Color = piece1.ColorComponent.Color;
					}
					ClearPiece (piece2.X, piece2.Y);
				}                
                //清除行列
				if (piece1.Type == PieceType.ROW_CLEAR || piece1.Type == PieceType.COLUMN_CLEAR) {
					ClearPiece (piece1.X, piece1.Y);
				}
                //清除行列
				if (piece2.Type == PieceType.ROW_CLEAR || piece2.Type == PieceType.COLUMN_CLEAR) {
					ClearPiece (piece2.X, piece2.Y);
				}
                //清除匹配队列元素
                ClearAllValidMatches();
                //清除完以后前后元素置空
                pressedPiece = null;
				enteredPiece = null;
                //开始填充元素
				StartCoroutine (Fill ());      
                //记下移动了一次
				level.OnMove ();
			} else {//如果没有匹配队列还换回来
				pieces [piece1.X, piece1.Y] = piece1;
				pieces [piece2.X, piece2.Y] = piece2;
			}
		}
	}
    
    //清除列
	public void ClearRow(int row)
	{
		for (int x = 0; x < xDim; x++) {
			ClearPiece (x, row);
		}
	}

    //清除行
	public void ClearColumn(int column)
	{
		for (int y = 0; y < yDim; y++) {
			ClearPiece (column, y);
		}
	}

    //清除某一颜色的元素
	public void ClearColor(ColorPiece.ColorType color)
	{
		for (int x = 0; x < xDim; x++) {
			for (int y = 0; y < yDim; y++) {
				if (pieces [x, y].IsColored () && (pieces [x, y].ColorComponent.Color == color
				    || color == ColorPiece.ColorType.ANY)) {
					ClearPiece (x, y);
				}
			}
		}
	}

    //游戏结束时执行
	public void GameOver()
	{
		gameOver = true;
	}

    //返回网格中元素或者其他类型的列表
	public List<GamePiece> GetPiecesOfType(PieceType type)
	{
		List<GamePiece> piecesOfType = new List<GamePiece> ();
		for (int x = 0; x < xDim; x++) {
			for (int y = 0; y < yDim; y++) {
				if (pieces [x, y].Type == type) {
					piecesOfType.Add (pieces [x, y]);
				}
			}
		}
		return piecesOfType;
	}
}
