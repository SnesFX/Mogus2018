using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FontExtensionData : ScriptableObject
{
	public string FontName;

	public List<KerningPair> kernings = new List<KerningPair>();

	public Dictionary<int, Dictionary<int, int>> fastKern;

	public void Prepare(Dictionary<int, Dictionary<int, float>> outside)
	{
		if (fastKern != null)
		{
			return;
		}
		fastKern = new Dictionary<int, Dictionary<int, int>>();
		for (int i = 0; i < kernings.Count; i++)
		{
			KerningPair kerningPair = kernings[i];
			Dictionary<int, int> value;
			if (!fastKern.TryGetValue(kerningPair.First, out value))
			{
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				fastKern[kerningPair.First] = dictionary;
				value = dictionary;
			}
			value[kerningPair.Second] = kerningPair.Pixels;
			Dictionary<int, float> value2;
			if (!outside.TryGetValue(kerningPair.First, out value2))
			{
				Dictionary<int, float> dictionary2 = new Dictionary<int, float>();
				outside[kerningPair.First] = dictionary2;
				value2 = dictionary2;
			}
			value2[kerningPair.Second] = kerningPair.Pixels;
		}
	}
}
