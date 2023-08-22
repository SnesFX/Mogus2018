using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FontData
{
	public Vector2 TextureSize = new Vector2(256f, 256f);

	public List<Vector4> bounds = new List<Vector4>();

	public List<Vector3> offsets = new List<Vector3>();

	public Dictionary<int, int> charMap = new Dictionary<int, int>();

	public float LineHeight;

	public Dictionary<int, Dictionary<int, float>> kernings = new Dictionary<int, Dictionary<int, float>>();

	public float GetKerning(int last, int cur)
	{
		Dictionary<int, float> value;
		float value2;
		if (kernings.TryGetValue(last, out value) && value.TryGetValue(cur, out value2))
		{
			return value2;
		}
		return 0f;
	}
}
