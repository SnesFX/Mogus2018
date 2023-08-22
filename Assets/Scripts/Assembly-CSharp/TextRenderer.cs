using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TextRenderer : MonoBehaviour
{
	public TextAsset FontData;

	public float scale = 1f;

	public bool Centered;

	public bool RightAligned;

	public TextLink textLinkPrefab;

	[HideInInspector]
	private FontData data;

	[HideInInspector]
	private Mesh mesh;

	[HideInInspector]
	private MeshRenderer render;

	[Multiline]
	public string Text;

	private string lastText;

	public Color Color = Color.white;

	private Color lastColor = Color.white;

	public Color OutlineColor = Color.black;

	private Color lastOutlineColor = Color.white;

	public float maxWidth = -1f;

	private Vector2 cursorLocation;

	public float Width { get; private set; }

	public float Height { get; private set; }

	public Vector3 CursorPos
	{
		get
		{
			return new Vector3(cursorLocation.x / 100f * scale, cursorLocation.y / 100f * scale, -0.001f);
		}
	}

	public void Start()
	{
		if (data == null)
		{
			data = FontCache.Instance.LoadFont(FontData);
			render = GetComponent<MeshRenderer>();
			MeshFilter component = GetComponent<MeshFilter>();
			if (!component.mesh)
			{
				mesh = new Mesh();
				mesh.name = "Text" + base.name;
				component.mesh = mesh;
			}
			else
			{
				mesh = component.mesh;
			}
		}
	}

	[ContextMenu("Generate Mesh")]
	public void GenerateMesh()
	{
		data = FontCache.LoadFontUncached(FontData);
		render = GetComponent<MeshRenderer>();
		MeshFilter component = GetComponent<MeshFilter>();
		if (!component.sharedMesh)
		{
			mesh = new Mesh();
			mesh.name = "Text" + base.name;
			component.mesh = mesh;
		}
		else
		{
			mesh = component.sharedMesh;
		}
		lastText = null;
		lastOutlineColor = OutlineColor;
		Update();
	}

	private void Update()
	{
		if (lastOutlineColor != OutlineColor)
		{
			lastOutlineColor = OutlineColor;
			render.material.SetColor("_OutlineColor", OutlineColor);
		}
		if (lastText != Text || lastColor != Color)
		{
			RefreshMesh();
		}
	}

	public void RefreshMesh()
	{
		if (data == null)
		{
			Start();
		}
		lastText = Text;
		lastColor = Color;
		if (maxWidth > 0f)
		{
			lastText = (Text = WrapText(data, lastText, maxWidth));
		}
		List<Vector3> list = new List<Vector3>(lastText.Length * 4);
		List<Vector2> list2 = new List<Vector2>(lastText.Length * 4);
		List<Color> list3 = new List<Color>(lastText.Length * 4);
		int[] array = new int[lastText.Length * 6];
		Width = 0f;
		cursorLocation.x = (cursorLocation.y = 0f);
		int num = -1;
		Vector2 from = default(Vector2);
		string text = null;
		int lineStart = 0;
		int num2 = 0;
		Color item = Color;
		int? num3 = null;
		for (int i = 0; i < lastText.Length; i++)
		{
			int num4 = lastText[i];
			if (num4 == 91)
			{
				num3 = 0;
				num = num4;
				continue;
			}
			if (num3.HasValue)
			{
				switch (num4)
				{
				case 93:
					if (num != 91)
					{
						item = new Color32((byte)((num3 >> 24) & 0xFF).Value, (byte)((num3 >> 16) & 0xFF).Value, (byte)((num3 >> 8) & 0xFF).Value, (byte)(num3 & 0xFF).Value);
						item.a *= Color.a;
					}
					else
					{
						item = Color;
					}
					num = -1;
					num3 = null;
					if (text != null)
					{
						TextLink textLink = Object.Instantiate(textLinkPrefab, base.transform);
						textLink.transform.localScale = Vector3.one;
						Vector3 vector = list.Last();
						textLink.Set(from, vector, text);
						text = null;
					}
					break;
				case 104:
				{
					int num6 = lastText.IndexOf(']', i);
					text = lastText.Substring(i, num6 - i);
					from = list[list.Count - 2];
					item = new Color(0.5f, 0.5f, 1f);
					num = -1;
					num3 = null;
					i = num6;
					break;
				}
				default:
				{
					int? num5 = num3 << 4;
					num3 = ((!num5.HasValue) ? null : new int?(num5.GetValueOrDefault() | CharToInt(num4)));
					break;
				}
				}
				num = num4;
				continue;
			}
			switch (num4)
			{
			case 10:
				if (Centered)
				{
					CenterVerts(list, cursorLocation.x, lineStart);
				}
				else if (RightAligned)
				{
					RightAlignVerts(list, cursorLocation.x, lineStart);
				}
				cursorLocation.x = 0f;
				cursorLocation.y -= data.LineHeight;
				lineStart = list.Count;
				continue;
			case 13:
				continue;
			}
			int value;
			if (!data.charMap.TryGetValue(num4, out value))
			{
				Debug.Log("Missing char :" + num4);
				num4 = -1;
				value = data.charMap[-1];
			}
			Vector4 vector2 = data.bounds[value];
			Vector2 textureSize = data.TextureSize;
			Vector3 vector3 = data.offsets[value];
			float kerning = data.GetKerning(num, num4);
			float num7 = cursorLocation.x + vector3.x + kerning;
			float num8 = cursorLocation.y - vector3.y;
			list.Add(new Vector3(num7, num8 - vector2.w) / 100f * scale);
			list.Add(new Vector3(num7, num8) / 100f * scale);
			list.Add(new Vector3(num7 + vector2.z, num8) / 100f * scale);
			list.Add(new Vector3(num7 + vector2.z, num8 - vector2.w) / 100f * scale);
			list3.Add(item);
			list3.Add(item);
			list3.Add(item);
			list3.Add(item);
			list2.Add(new Vector2(vector2.x / textureSize.x, 1f - (vector2.y + vector2.w) / textureSize.y));
			list2.Add(new Vector2(vector2.x / textureSize.x, 1f - vector2.y / textureSize.y));
			list2.Add(new Vector2((vector2.x + vector2.z) / textureSize.x, 1f - vector2.y / textureSize.y));
			list2.Add(new Vector2((vector2.x + vector2.z) / textureSize.x, 1f - (vector2.y + vector2.w) / textureSize.y));
			array[num2 * 6] = num2 * 4;
			array[num2 * 6 + 1] = num2 * 4 + 1;
			array[num2 * 6 + 2] = num2 * 4 + 2;
			array[num2 * 6 + 3] = num2 * 4;
			array[num2 * 6 + 4] = num2 * 4 + 2;
			array[num2 * 6 + 5] = num2 * 4 + 3;
			cursorLocation.x += vector3.z + kerning;
			float num9 = cursorLocation.x / 100f * scale;
			if (Width < num9)
			{
				Width = num9;
			}
			num = num4;
			num2++;
		}
		if (Centered)
		{
			CenterVerts(list, cursorLocation.x, lineStart);
			cursorLocation.x /= 2f;
			Width /= 2f;
		}
		else if (RightAligned)
		{
			RightAlignVerts(list, cursorLocation.x, lineStart);
		}
		Height = (0f - (cursorLocation.y - data.LineHeight)) / 100f * scale;
		mesh.Clear();
		mesh.SetVertices(list);
		mesh.SetColors(list3);
		mesh.SetUVs(0, list2);
		mesh.SetIndices(array, MeshTopology.Triangles, 0);
	}

	private void RightAlignVerts(List<Vector3> verts, float baseX, int lineStart)
	{
		for (int i = lineStart; i < verts.Count; i++)
		{
			Vector3 value = verts[i];
			value.x -= baseX / 100f * scale;
			verts[i] = value;
		}
	}

	private void CenterVerts(List<Vector3> verts, float baseX, int lineStart)
	{
		for (int i = lineStart; i < verts.Count; i++)
		{
			Vector3 value = verts[i];
			value.x -= baseX / 200f * scale;
			verts[i] = value;
		}
	}

	private int CharToInt(int c)
	{
		if (c < 65)
		{
			return c - 48;
		}
		if (c < 97)
		{
			return 10 + (c - 65);
		}
		return 10 + (c - 97);
	}

	public static string WrapText(FontData data, string displayTxt, float maxWidth)
	{
		float num = 0f;
		int num2 = -1;
		int last = -1;
		bool flag = false;
		int num3 = 0;
		for (int i = 0; i < displayTxt.Length && num3++ <= 1000; i++)
		{
			int num4 = displayTxt[i];
			switch (num4)
			{
			case 91:
				flag = true;
				break;
			case 93:
				flag = false;
				continue;
			}
			if (flag)
			{
				continue;
			}
			switch (num4)
			{
			case 10:
				num2 = -1;
				last = -1;
				num = 0f;
				continue;
			case 13:
				continue;
			}
			int value;
			if (!data.charMap.TryGetValue(num4, out value))
			{
				Debug.Log("Missing char :" + num4);
				num4 = -1;
				value = data.charMap[-1];
			}
			if (num4 == 32)
			{
				num2 = i;
			}
			num += data.offsets[value].z + data.GetKerning(last, num4);
			if (num > maxWidth * 100f)
			{
				if (num2 != -1)
				{
					displayTxt = displayTxt.Substring(0, num2) + '\n' + displayTxt.Substring(num2 + 1);
					i = num2;
				}
				else
				{
					displayTxt = displayTxt.Substring(0, i) + '\n' + displayTxt.Substring(i);
				}
				num2 = -1;
				last = -1;
				num = 0f;
			}
			last = num4;
		}
		return displayTxt;
	}
}
