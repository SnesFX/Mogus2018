using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;

public static class AdPlayer
{
	private static InterstitialAd interstitial;

	private const string appId = "unexpected_platform";

	private const string adUnitId = "unexpected_platform";

	public static void ShowInterstitial(MonoBehaviour parent)
	{
		parent.StartCoroutine(CoShowAd());
	}

	private static IEnumerator CoShowAd()
	{
		if (TempData.playAgain)
		{
			DestroyableSingleton<EndGameManager>.Instance.StartCoroutine(DestroyableSingleton<EndGameManager>.Instance.CoJoinGame());
		}
		else
		{
			AmongUsClient.Instance.ExitGame();
		}
		yield break;
	}

	public static void RequestInterstitial()
	{
		MobileAds.Initialize("unexpected_platform");
		if (interstitial == null)
		{
			interstitial = new InterstitialAd("unexpected_platform");
			AdRequest adRequest = new AdRequest.Builder().AddTestDevice("437025455EDB8E2BDE7B6F4837D3D19F").Build();
			if (SaveManager.ShowAdsScreen.HasFlag(ShowAdsState.NonPersonalized))
			{
				adRequest.Extras.Add("npa", "1");
			}
			interstitial.OnAdFailedToLoad += Interstitial_OnAdFailedToLoad;
			interstitial.OnAdClosed += Interstitial_OnAdClosed;
			interstitial.OnAdLeavingApplication += Interstitial_OnAdLeavingApplication;
			interstitial.LoadAd(adRequest);
		}
	}

	private static void Interstitial_OnAdLeavingApplication(object sender, EventArgs e)
	{
		Debug.LogError("Ad leaving app");
		interstitial.Destroy();
		interstitial = null;
	}

	private static void Interstitial_OnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
	{
		Debug.LogError("Couldn't load ad: " + (e.Message ?? "No Message"));
		interstitial.Destroy();
		interstitial = null;
	}

	private static void Interstitial_OnAdClosed(object sender, EventArgs e)
	{
		Debug.LogError("Ad closed");
		interstitial.Destroy();
		interstitial = null;
	}
}
