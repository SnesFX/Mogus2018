using System.Collections.Generic;
using UnityEngine;

public class PlayerTab : MonoBehaviour
{
	public ColorChip ColorTabPrefab;

	public SpriteRenderer DemoImage;

	public SpriteRenderer HatImage;

	public FloatRange XRange = new FloatRange(1.5f, 3f);

	public FloatRange YRange = new FloatRange(-1f, -3f);

	private HashSet<int> AvailableColors = new HashSet<int>();

	private List<ColorChip> ColorChips = new List<ColorChip>();

	public void Start()
	{
		for (int j = 0; j < Palette.PlayerColors.Length; j++)
		{
			float x = XRange.Lerp(j % 2);
			float y = YRange.Lerp(1f - (float)(j / 2) / 4f);
			ColorChip colorChip = Object.Instantiate(ColorTabPrefab);
			colorChip.transform.SetParent(base.transform);
			colorChip.transform.localPosition = new Vector3(x, y, -1f);
			int i = j;
			colorChip.Button.OnClick.AddListener(delegate
			{
				SelectColor(i);
			});
			colorChip.Inner.color = Palette.PlayerColors[j];
			ColorChips.Add(colorChip);
		}
	}

	public void OnEnable()
	{
		PlayerControl.SetPlayerMaterialColors(SaveManager.BodyColor, DemoImage);
		PlayerControl.SetHatImage(SaveManager.LastHat, HatImage);
	}

	public void Update()
	{
		UpdateAvailableColors();
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChips[i].InUseForeground.SetActive(!AvailableColors.Contains(i));
		}
	}

	private void SelectColor(int colorId)
	{
		UpdateAvailableColors();
		if (AvailableColors.Remove(colorId))
		{
			SaveManager.BodyColor = (byte)colorId;
			PlayerControl.SetPlayerMaterialColors(SaveManager.BodyColor, DemoImage);
			if ((bool)PlayerControl.LocalPlayer)
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)colorId);
			}
		}
	}

	public void UpdateAvailableColors()
	{
		for (int i = 0; i < Palette.PlayerColors.Length; i++)
		{
			AvailableColors.Add(i);
		}
		if (!GameData.Instance)
		{
			return;
		}
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		for (int j = 0; j < allPlayers.Count; j++)
		{
			GameData.PlayerInfo playerInfo = allPlayers[j];
			if ((bool)playerInfo.Object)
			{
				AvailableColors.Remove(playerInfo.Object.ColorId);
			}
		}
	}
}
