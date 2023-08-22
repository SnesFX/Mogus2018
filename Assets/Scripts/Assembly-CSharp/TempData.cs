using System.Collections.Generic;
using GoogleMobileAds.Api;

public static class TempData
{
	public static GameOverReason EndReason = GameOverReason.HumansByTask;

	public static bool showAd;

	public static bool playAgain;

	public static InterstitialAd interstitial;

	public static bool GameStarting;

	public static List<WinningPlayerData> winners = new List<WinningPlayerData>
	{
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 0,
			IsDead = true
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 1,
			IsDead = true
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 2,
			IsDead = true
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 3
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 4
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 5
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 6
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 7
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 8
		}
	};

	public static bool DidHumansWin(GameOverReason reason)
	{
		return reason == GameOverReason.HumansByTask || reason == GameOverReason.HumansByVote;
	}
}
