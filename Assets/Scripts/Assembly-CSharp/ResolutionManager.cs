using System;
using UnityEngine;

public static class ResolutionManager
{
	public static event Action<float> ResolutionChanged;

	public static void SetResolution(int width, int height, bool fullscreen)
	{
		if (ResolutionManager.ResolutionChanged != null)
		{
			ResolutionManager.ResolutionChanged((float)width / (float)height);
		}
		Screen.SetResolution(width, height, fullscreen);
	}

	public static void ToggleFullscreen()
	{
		bool flag = !Screen.fullScreen;
		int width;
		int height;
		if (flag)
		{
			Resolution[] resolutions = Screen.resolutions;
			Resolution resolution = resolutions[0];
			for (int i = 0; i < resolutions.Length; i++)
			{
				if (resolution.height < resolutions[i].height)
				{
					resolution = resolutions[i];
				}
			}
			width = resolution.width;
			height = resolution.height;
		}
		else
		{
			width = 711;
			height = 400;
		}
		SetResolution(width, height, flag);
	}
}
