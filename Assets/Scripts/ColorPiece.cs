using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColorPiece : MonoBehaviour {

    //通过结构体枚举外部赋值可能的所有类型
	public enum ColorType
	{
		YELLOW,
		PURPLE,
		RED,
		BLUE,
		GREEN,
		PINK,
		ANY,
		COUNT
	};
	[System.Serializable]
	public struct ColorSprite
	{
		public ColorType color;
		public Sprite sprite;
	};
    public ColorSprite[] colorSprites;

    //类型数量封装
    public int NumColors
    {
        get { return colorSprites.Length; }
    }

    //所有图片字典
    private Dictionary<ColorType, Sprite> colorSpriteDict;

    //自己的类型
    private ColorType color;
	public ColorType Color
	{
		get { return color; }
		set { SetColor (value); }
	}
    //自己的图片
    private SpriteRenderer sprite;

	void Awake()
	{
		sprite = transform.Find ("piece").GetComponent<SpriteRenderer> ();

		colorSpriteDict = new Dictionary<ColorType, Sprite> ();

		for (int i = 0; i < colorSprites.Length; i++) {
			if (!colorSpriteDict.ContainsKey (colorSprites [i].color)) {
				colorSpriteDict.Add (colorSprites [i].color, colorSprites [i].sprite);
			}
		}
	}

    //设置颜色
	public void SetColor(ColorType newColor)
	{
		color = newColor;

		if (colorSpriteDict.ContainsKey (newColor)) {
			sprite.sprite = colorSpriteDict [newColor];
		}
	}
}
