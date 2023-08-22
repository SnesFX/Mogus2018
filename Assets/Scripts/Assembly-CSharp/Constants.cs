using System;
using UnityEngine;

public static class Constants
{
	public const string LocalNetAddress = "127.0.0.1";

	public const string OnlineNetAddress = "18.218.20.65";

	public const int AnnouncementPort = 22024;

	public const string InfinitySymbol = "∞";

	public const int GhostLayer = 12;

	public static readonly int ShipOnlyMask = LayerMask.GetMask("Ship");

	public static readonly int ShipAndObjectsMask = LayerMask.GetMask("Ship") | LayerMask.GetMask("Objects");

	public static readonly int ShipAndAllObjectsMask = LayerMask.GetMask("Ship") | LayerMask.GetMask("Objects") | LayerMask.GetMask("ShortObjects");

	public static readonly int NotShipMask = ~LayerMask.GetMask("Ship");

	public static readonly int Usables = ~LayerMask.GetMask("Ship", "UI");

	public static readonly int PlayersOnlyMask = LayerMask.GetMask("Players");

	public static readonly int ShadowMask = LayerMask.GetMask("Shadow") | LayerMask.GetMask("Objects") | LayerMask.GetMask("IlluminatedBlocking");

	public const int Year = 2018;

	public const int Month = 11;

	public const int Day = 14;

	public const int Revision = 0;

	internal static int GetBroadcastVersion()
	{
		return 36947700;
	}

	internal static byte[] GetBroadcastVersionBytes()
	{
		return BitConverter.GetBytes(GetBroadcastVersion());
	}

	public static bool ShouldPlaySfx()
	{
		return !AmongUsClient.Instance || AmongUsClient.Instance.GameMode != 0 || DetectHeadset.Detect();
	}
}
