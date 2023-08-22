using System;
using System.Reflection;
using GoogleMobileAds.Common;

namespace GoogleMobileAds.Api
{
	public class MobileAds
	{
		private static readonly IMobileAdsClient client = GetMobileAdsClient();

		public static void Initialize(string appId)
		{
			client.Initialize(appId);
			MobileAdsEventExecutor.Initialize();
		}

		public static void SetApplicationMuted(bool muted)
		{
			client.SetApplicationMuted(muted);
		}

		public static void SetApplicationVolume(float volume)
		{
			client.SetApplicationVolume(volume);
		}

		public static void SetiOSAppPauseOnBackground(bool pause)
		{
			client.SetiOSAppPauseOnBackground(pause);
		}

		private static IMobileAdsClient GetMobileAdsClient()
		{
			Type type = Type.GetType("GoogleMobileAds.GoogleMobileAdsClientFactory,Assembly-CSharp");
			MethodInfo method = type.GetMethod("MobileAdsInstance", BindingFlags.Static | BindingFlags.Public);
			return (IMobileAdsClient)method.Invoke(null, null);
		}
	}
}
