using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class SaveManager
{
	private static bool loaded;

	private static bool loadedSecure;

	private static bool loadedStats;

	private static bool loadedAnnounce;

	private static string lastPlayerName;

	private static byte sfxVolume = byte.MaxValue;

	private static byte musicVolume = byte.MaxValue;

	private static bool showMinPlayerWarning = true;

	private static bool showOnlineHelp = true;

	private static bool sendDataScreen = false;

	private static byte showAdsScreen = 0;

	private static bool sendName = true;

	private static bool sendTelemetry = true;

	private static uint timesImpostor;

	private static uint gamesStarted;

	private static int touchConfig;

	private static float joyStickSize = 1f;

	private static uint colorConfig;

	private static uint lastHat;

	private static GameOptionsData optionsData;

	private static HashSet<string> purchases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static Announcement lastAnnounce;

	public static Announcement LastAnnouncement
	{
		get
		{
			LoadAnnouncement();
			return lastAnnounce;
		}
		set
		{
			lastAnnounce = value;
			SaveAnnouncement();
		}
	}

	public static bool BoughtNoAds
	{
		get
		{
			LoadSecureData();
			return purchases.Contains("bought_ads");
		}
	}

	public static ShowAdsState ShowAdsScreen
	{
		get
		{
			LoadPlayerPrefs();
			return (ShowAdsState)showAdsScreen;
		}
		set
		{
			showAdsScreen = (byte)value;
			SavePlayerPrefs();
		}
	}

	public static bool SendDataScreen
	{
		get
		{
			LoadPlayerPrefs();
			return sendDataScreen;
		}
		set
		{
			sendDataScreen = value;
			SavePlayerPrefs();
		}
	}

	public static bool SendName
	{
		get
		{
			LoadPlayerPrefs();
			return sendName && SendTelemetry;
		}
		set
		{
			sendName = value;
			SavePlayerPrefs();
		}
	}

	public static bool SendTelemetry
	{
		get
		{
			LoadPlayerPrefs();
			return sendTelemetry;
		}
		set
		{
			sendTelemetry = value;
			SavePlayerPrefs();
		}
	}

	public static bool ShowMinPlayerWarning
	{
		get
		{
			LoadPlayerPrefs();
			return showMinPlayerWarning;
		}
		set
		{
			showMinPlayerWarning = value;
			SavePlayerPrefs();
		}
	}

	public static bool ShowOnlineHelp
	{
		get
		{
			LoadPlayerPrefs();
			return showOnlineHelp;
		}
		set
		{
			showOnlineHelp = value;
			SavePlayerPrefs();
		}
	}

	public static float SfxVolume
	{
		get
		{
			LoadPlayerPrefs();
			return (float)(int)sfxVolume / 255f;
		}
		set
		{
			sfxVolume = (byte)(value * 255f);
			SavePlayerPrefs();
		}
	}

	public static float MusicVolume
	{
		get
		{
			LoadPlayerPrefs();
			return (float)(int)musicVolume / 255f;
		}
		set
		{
			musicVolume = (byte)(value * 255f);
			SavePlayerPrefs();
		}
	}

	public static int TouchConfig
	{
		get
		{
			LoadPlayerPrefs();
			return touchConfig;
		}
		set
		{
			touchConfig = value;
			SavePlayerPrefs();
		}
	}

	public static float JoystickSize
	{
		get
		{
			LoadPlayerPrefs();
			return joyStickSize;
		}
		set
		{
			joyStickSize = value;
			SavePlayerPrefs();
		}
	}

	public static string PlayerName
	{
		get
		{
			LoadPlayerPrefs();
			return lastPlayerName ?? "Enter Name";
		}
		set
		{
			lastPlayerName = value;
			SavePlayerPrefs();
		}
	}

	public static uint GamesStarted
	{
		get
		{
			LoadStats();
			return gamesStarted;
		}
		set
		{
			gamesStarted = value;
			SaveStats();
		}
	}

	public static uint TimesImpostor
	{
		get
		{
			LoadStats();
			return timesImpostor;
		}
		set
		{
			timesImpostor = value;
			SaveStats();
		}
	}

	public static uint LastHat
	{
		get
		{
			LoadPlayerPrefs();
			return lastHat;
		}
		set
		{
			lastHat = value;
			SavePlayerPrefs();
		}
	}

	public static byte BodyColor
	{
		get
		{
			LoadPlayerPrefs();
			return (byte)(colorConfig & 0xFFu);
		}
		set
		{
			colorConfig = (colorConfig & 0xFFFF00u) | (value & 0xFFu);
			SavePlayerPrefs();
		}
	}

	public static GameOptionsData GameOptions
	{
		get
		{
			LoadGameOptions();
			return optionsData;
		}
		set
		{
			optionsData = value;
			SaveGameOptions();
		}
	}

	public static bool GetPurchase(string key)
	{
		LoadSecureData();
		return purchases.Contains(key);
	}

	public static void SetPurchased(string key)
	{
		purchases.Add(key);
		if (key == "bought_ads")
		{
			ShowAdsScreen = ShowAdsState.Purchased;
		}
		SaveSecureData();
	}

	private static void LoadGameOptions()
	{
		if (optionsData != null)
		{
			return;
		}
		optionsData = new GameOptionsData();
		string path = Path.Combine(Application.persistentDataPath, "gameOptions");
		if (!File.Exists(path))
		{
			return;
		}
		using (FileStream input = File.OpenRead(path))
		{
			using (BinaryReader reader = new BinaryReader(input))
			{
				optionsData.Deserialize(reader);
			}
		}
	}

	private static void SaveGameOptions()
	{
		string path = Path.Combine(Application.persistentDataPath, "gameOptions");
		using (FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write))
		{
			using (BinaryWriter writer = new BinaryWriter(output))
			{
				optionsData.Serialize(writer);
			}
		}
	}

	private static void LoadAnnouncement()
	{
		if (loadedAnnounce)
		{
			return;
		}
		loadedAnnounce = true;
		string path = Path.Combine(Application.persistentDataPath, "announcement");
		if (File.Exists(path))
		{
			string text = File.ReadAllText(path);
			string[] array = text.Split(default(char));
			if (array.Length == 3)
			{
				Announcement announcement = default(Announcement);
				TryGetUint(array, 0, out announcement.Id);
				announcement.AnnounceText = array[1];
				TryGetInt(array, 2, out announcement.DateFetched);
				lastAnnounce = announcement;
			}
			else
			{
				lastAnnounce = default(Announcement);
			}
		}
	}

	public static void SaveAnnouncement()
	{
		string path = Path.Combine(Application.persistentDataPath, "announcement");
		File.WriteAllText(path, string.Join("\0", lastAnnounce.Id, lastAnnounce.AnnounceText, lastAnnounce.DateFetched));
	}

	private static void LoadStats()
	{
		if (!loadedStats)
		{
			loadedStats = true;
			string path = Path.Combine(Application.persistentDataPath, "playerStats");
			if (File.Exists(path))
			{
				string text = File.ReadAllText(path);
				string[] parts = text.Split(',');
				TryGetUint(parts, 0, out timesImpostor);
				TryGetUint(parts, 1, out gamesStarted);
			}
		}
	}

	public static void SaveStats()
	{
		LoadStats();
		string path = Path.Combine(Application.persistentDataPath, "playerStats");
		File.WriteAllText(path, string.Join(",", timesImpostor, gamesStarted));
	}

	private static void LoadPlayerPrefs()
	{
		if (loaded)
		{
			return;
		}
		loaded = true;
		string path = Path.Combine(Application.persistentDataPath, "playerPrefs");
		if (File.Exists(path))
		{
			string text = File.ReadAllText(path);
			string[] array = text.Split(',');
			lastPlayerName = array[0];
			if (array.Length > 1)
			{
				int.TryParse(array[1], out touchConfig);
			}
			if (array.Length <= 2 || !uint.TryParse(array[2], out colorConfig))
			{
				colorConfig = (uint)((byte)(Palette.PlayerColors.RandomIdx() << 16) | (byte)(Palette.PlayerColors.RandomIdx() << 8) | (byte)Palette.PlayerColors.RandomIdx());
			}
			TryGetBool(array, 4, out sendName);
			TryGetBool(array, 5, out sendTelemetry);
			TryGetBool(array, 6, out sendDataScreen);
			TryGetByte(array, 7, out showAdsScreen);
			TryGetBool(array, 8, out showMinPlayerWarning);
			TryGetBool(array, 9, out showOnlineHelp);
			TryGetUint(array, 10, out lastHat);
			TryGetByte(array, 11, out sfxVolume);
			TryGetByte(array, 12, out musicVolume);
			TryGetFloat(array, 13, out joyStickSize, 1f);
		}
	}

	private static void SavePlayerPrefs()
	{
		LoadPlayerPrefs();
		string path = Path.Combine(Application.persistentDataPath, "playerPrefs");
		File.WriteAllText(path, string.Join(",", lastPlayerName, touchConfig, colorConfig, (byte)1, sendName, sendTelemetry, sendDataScreen, showAdsScreen, showMinPlayerWarning, showOnlineHelp, lastHat, sfxVolume, musicVolume, joyStickSize.ToString(CultureInfo.InvariantCulture)));
	}

	private static void LoadSecureData()
	{
		if (loadedSecure)
		{
			return;
		}
		loadedSecure = true;
		string text = Path.Combine(Application.persistentDataPath, "secureNew");
		Debug.Log("Loading secure: " + text);
		if (!File.Exists(text))
		{
			return;
		}
		byte[] array = File.ReadAllBytes(text);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] ^= (byte)(i % 212);
		}
		try
		{
			bool flag = true;
			using (MemoryStream memoryStream = new MemoryStream(array))
			{
				using (BinaryReader binaryReader = new BinaryReader(memoryStream))
				{
					if (flag & binaryReader.ReadString().Equals(SystemInfo.deviceUniqueIdentifier))
					{
						while (memoryStream.Position < memoryStream.Length)
						{
							purchases.Add(binaryReader.ReadString());
						}
					}
				}
			}
		}
		catch
		{
			Debug.Log("Deleted corrupt secure file");
			File.Delete(text);
		}
	}

	private static void SaveSecureData()
	{
		LoadSecureData();
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(SystemInfo.deviceUniqueIdentifier);
				foreach (string purchase in purchases)
				{
					binaryWriter.Write(purchase);
				}
				binaryWriter.Flush();
				memoryStream.Position = 0L;
				array = memoryStream.ToArray();
			}
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i] ^= (byte)(i % 212);
		}
		string path = Path.Combine(Application.persistentDataPath, "secureNew");
		File.WriteAllBytes(path, array);
	}

	private static void TryGetBool(string[] parts, int index, out bool value)
	{
		value = false;
		if (parts.Length > index)
		{
			bool.TryParse(parts[index], out value);
		}
	}

	private static void TryGetByte(string[] parts, int index, out byte value)
	{
		value = 0;
		if (parts.Length > index)
		{
			byte.TryParse(parts[index], out value);
		}
	}

	private static void TryGetFloat(string[] parts, int index, out float value, float @default = 0f)
	{
		value = @default;
		if (parts.Length > index)
		{
			float.TryParse(parts[index], NumberStyles.Number, CultureInfo.InvariantCulture, out value);
		}
	}

	private static void TryGetInt(string[] parts, int index, out int value)
	{
		value = 0;
		if (parts.Length > index)
		{
			int.TryParse(parts[index], out value);
		}
	}

	private static void TryGetUint(string[] parts, int index, out uint value)
	{
		value = 0u;
		if (parts.Length > index)
		{
			uint.TryParse(parts[index], out value);
		}
	}
}
