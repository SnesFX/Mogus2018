using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FontCache : MonoBehaviour
{
	private static FontCache _instance;

	private Dictionary<string, FontData> cache = new Dictionary<string, FontData>();

	public List<FontExtensionData> extraData = new List<FontExtensionData>();

	public static FontCache Instance
	{
		get
		{
			if (!_instance)
			{
				_instance = UnityEngine.Object.FindObjectOfType<FontCache>();
				UnityEngine.Object.DontDestroyOnLoad(_instance.gameObject);
			}
			return _instance;
		}
	}

	public void Start()
	{
		if (!_instance)
		{
			_instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else if (_instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public FontData LoadFont(TextAsset dataSrc)
	{
		if (cache == null)
		{
			cache = new Dictionary<string, FontData>();
		}
		if (cache.ContainsKey(dataSrc.name))
		{
			return cache[dataSrc.name];
		}
		int num = extraData.FindIndex((FontExtensionData ed) => ed.FontName.Equals(dataSrc.name, StringComparison.OrdinalIgnoreCase));
		FontExtensionData eData = null;
		if (num >= 0)
		{
			eData = extraData[num];
		}
		FontData fontData = LoadFontUncached(dataSrc, eData);
		cache[dataSrc.name] = fontData;
		return fontData;
	}

	public static FontData LoadFontUncached(TextAsset dataSrc, FontExtensionData eData = null)
	{
		FontData fontData = new FontData();
		fontData.charMap = new Dictionary<int, int>();
		if (eData != null)
		{
			eData.Prepare(fontData.kernings);
		}
		using (StringReader stringReader = new StringReader(dataSrc.text))
		{
			for (string text = stringReader.ReadLine(); text != null; text = stringReader.ReadLine())
			{
				if (text.StartsWith("common "))
				{
					fontData.LineHeight = ReadNumber(text, 18);
					fontData.TextureSize = new Vector2(ReadNumber(text, 36), ReadNumber(text, 47));
				}
				else if (text.StartsWith("char "))
				{
					string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					int key = int.Parse(array[1].Split('=')[1]);
					int num = int.Parse(array[2].Split('=')[1]);
					int num2 = int.Parse(array[3].Split('=')[1]);
					int num3 = int.Parse(array[4].Split('=')[1]);
					int num4 = int.Parse(array[5].Split('=')[1]);
					int num5 = int.Parse(array[6].Split('=')[1]);
					int num6 = int.Parse(array[7].Split('=')[1]);
					int num7 = int.Parse(array[8].Split('=')[1]);
					fontData.charMap.Add(key, fontData.bounds.Count);
					fontData.bounds.Add(new Vector4(num, num2, num3, num4));
					fontData.offsets.Add(new Vector3(num5, num6, num7));
				}
				else if (text.StartsWith("kerning "))
				{
					int num8 = ReadNumber(text, 14);
					int num9 = ReadNumber(text, 25);
					int num10 = ReadNumber(text, 36);
					Dictionary<int, int> value;
					int value2;
					if (eData != null && eData.fastKern.TryGetValue(num8, out value) && value.TryGetValue(num9, out value2))
					{
						Debug.LogFormat("Extra kern for {0}+{1} = {2}", num8, num9, value2);
						num10 += value2;
					}
					if (!fontData.kernings.ContainsKey(num8))
					{
						fontData.kernings.Add(num8, new Dictionary<int, float>());
					}
					fontData.kernings[num8].Add(num9, num10);
				}
			}
			return fontData;
		}
	}

	private static int ReadNumber(string str, int start)
	{
		bool flag = false;
		int num = 0;
		for (int i = start; i < str.Length; i++)
		{
			int num2 = str[i];
			if (num == 0 && num2 == 45)
			{
				flag = true;
				continue;
			}
			num2 -= 48;
			if (num2 < 0 || num2 > 9)
			{
				break;
			}
			num = num * 10 + num2;
		}
		return (!flag) ? num : (-num);
	}
}
