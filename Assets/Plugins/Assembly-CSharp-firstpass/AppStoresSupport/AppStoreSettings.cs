using System;
using UnityEngine;
using UnityEngine.Store;

namespace AppStoresSupport
{
	[Serializable]
	public class AppStoreSettings : ScriptableObject
	{
		public string UnityClientID = string.Empty;

		public string UnityClientKey = string.Empty;

		public string UnityClientRSAPublicKey = string.Empty;

		public AppStoreSetting XiaomiAppStoreSetting = new AppStoreSetting();

		public AppInfo getAppInfo()
		{
			AppInfo appInfo = new AppInfo();
			appInfo.clientId = UnityClientID;
			appInfo.clientKey = UnityClientKey;
			appInfo.appId = XiaomiAppStoreSetting.AppID;
			appInfo.appKey = XiaomiAppStoreSetting.AppKey;
			appInfo.debug = XiaomiAppStoreSetting.IsTestMode;
			return appInfo;
		}
	}
}
