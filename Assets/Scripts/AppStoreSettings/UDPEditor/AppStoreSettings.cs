using System;
using UnityEngine;

namespace UDPEditor
{
	[Serializable]
	public class AppStoreSettings : ScriptableObject
	{
		public string UnityProjectID = "";

		public string UnityClientID = "";

		public string UnityClientKey = "";

		public string UnityClientRSAPublicKey = "";

		public string AppName = "";

		public string AppSlug = "";

		public string AppItemId = "";

		public string Permission = "";

		public const string appStoreSettingsAssetFolder = "Assets/Plugins/UDP/UdpSupport/Resources";

		public const string appStoreSettingsAssetPath = "Assets/Plugins/UDP/UdpSupport/Resources/GameSettings.asset";

		public const string appStoreSettingsPropFolder = "Assets/Plugins/Android/assets";

		public const string appStoreSettingsPropPath = "Assets/Plugins/Android/assets/GameSettings.prop";
	}
}
