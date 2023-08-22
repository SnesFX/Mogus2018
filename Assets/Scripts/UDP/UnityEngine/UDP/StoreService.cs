using System;
using System.Collections.Generic;
using UDPEditor;

namespace UnityEngine.UDP
{
	public class StoreService
	{
		private static AndroidJavaClass serviceClass = new AndroidJavaClass("com.unity.udp.sdk.ChannelService");

		public static string StoreName
		{
			get
			{
				try
				{
					return serviceClass.CallStatic<string>("getStoreName", new object[0]);
				}
				catch (Exception)
				{
					return "UDP";
				}
			}
		}

		public static void Initialize(IInitListener listener, AppInfo appInfo = null)
		{
			switch (Application.platform)
			{
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.LinuxEditor:
				listener.OnInitialized(null);
				break;
			default:
				throw new InvalidOperationException("StoreService doesn't support current platform!");
			case RuntimePlatform.Android:
			{
				if (GameObject.Find(MainThreadDispatcher.OBJECT_NAME) == null)
				{
					GameObject gameObject = new GameObject(MainThreadDispatcher.OBJECT_NAME);
					Object.DontDestroyOnLoad(gameObject);
					gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					gameObject.AddComponent<MainThreadDispatcher>();
				}
				AndroidJavaObject @static = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
				InitLoginForwardCallback initLoginForwardCallback = new InitLoginForwardCallback(listener);
				AndroidJavaObject androidJavaObject = new AndroidJavaObject("com.unity.udp.sdk.AppInfo");
				if (appInfo == null)
				{
					AppStoreSettings appStoreSettings = Resources.Load<AppStoreSettings>("GameSettings");
					if (appStoreSettings == null)
					{
						throw new InvalidOperationException("StoreService cannot find valid GameSettings.asset file! Initialization failed!");
					}
					androidJavaObject.Set("clientId", appStoreSettings.UnityClientID);
					androidJavaObject.Set("clientSecret", appStoreSettings.UnityClientKey);
					androidJavaObject.Set("appSlug", appStoreSettings.AppSlug);
					androidJavaObject.Set("RSAPublicKey", appStoreSettings.UnityClientRSAPublicKey);
				}
				else
				{
					androidJavaObject.Set("clientId", appInfo.ClientId);
					androidJavaObject.Set("clientSecret", appInfo.ClientKey);
					androidJavaObject.Set("appSlug", appInfo.AppSlug);
					androidJavaObject.Set("RSAPublicKey", appInfo.RSAPublicKey);
				}
				serviceClass.CallStatic("init", @static, androidJavaObject, initLoginForwardCallback);
				break;
			}
			}
		}

		public static void Purchase(string productId, string gameOrderId, string developerPayload, IPurchaseListener listener)
		{
			PurchaseForwardCallback purchaseForwardCallback = new PurchaseForwardCallback(listener);
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("com.unity.udp.sdk.PurchaseInfo");
			androidJavaObject.Set("productId", productId);
			androidJavaObject.Set("gameOrderId", gameOrderId);
			androidJavaObject.Set("developerPayload", developerPayload);
			serviceClass.CallStatic("purchase", androidJavaObject, purchaseForwardCallback);
		}

		public static void QueryInventory(IPurchaseListener listener)
		{
			PurchaseForwardCallback purchaseForwardCallback = new PurchaseForwardCallback(listener);
			serviceClass.CallStatic("queryInventory", null, purchaseForwardCallback);
		}

		public static void QueryInventory(List<string> productIds, IPurchaseListener listener)
		{
			PurchaseForwardCallback purchaseForwardCallback = new PurchaseForwardCallback(listener);
			serviceClass.CallStatic("queryInventory", javaArrayFromCSList(productIds), purchaseForwardCallback);
		}

		public static void ConsumePurchase(PurchaseInfo purchaseInfo, IPurchaseListener listener)
		{
			PurchaseForwardCallback purchaseForwardCallback = new PurchaseForwardCallback(listener);
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("com.unity.udp.sdk.PurchaseInfo");
			androidJavaObject.Call<AndroidJavaObject>("setItemType", new object[1] { purchaseInfo.ItemType });
			androidJavaObject.Call<AndroidJavaObject>("setProductId", new object[1] { purchaseInfo.ProductId });
			androidJavaObject.Call<AndroidJavaObject>("setDeveloperPayload", new object[1] { purchaseInfo.DeveloperPayload });
			androidJavaObject.Call<AndroidJavaObject>("setGameOrderId", new object[1] { purchaseInfo.GameOrderId });
			androidJavaObject.Call<AndroidJavaObject>("setOrderQueryToken", new object[1] { purchaseInfo.OrderQueryToken });
			androidJavaObject.Call<AndroidJavaObject>("setStorePurchaseJsonString", new object[1] { purchaseInfo.StorePurchaseJsonString });
			serviceClass.CallStatic("consumePurchase", androidJavaObject, purchaseForwardCallback);
		}

		public static void ConsumePurchase(List<PurchaseInfo> purchaseInfos, IPurchaseListener listener)
		{
			PurchaseForwardCallback purchaseForwardCallback = new PurchaseForwardCallback(listener);
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("java.util.ArrayList");
			foreach (PurchaseInfo purchaseInfo in purchaseInfos)
			{
				AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("com.unity.udp.sdk.PurchaseInfo");
				androidJavaObject2.Call<AndroidJavaObject>("setItemType", new object[1] { purchaseInfo.ItemType });
				androidJavaObject2.Call<AndroidJavaObject>("setProductId", new object[1] { purchaseInfo.ProductId });
				androidJavaObject2.Call<AndroidJavaObject>("setDeveloperPayload", new object[1] { purchaseInfo.DeveloperPayload });
				androidJavaObject2.Call<AndroidJavaObject>("setGameOrderId", new object[1] { purchaseInfo.GameOrderId });
				androidJavaObject2.Call<AndroidJavaObject>("setOrderQueryToken", new object[1] { purchaseInfo.OrderQueryToken });
				androidJavaObject2.Call<AndroidJavaObject>("setStorePurchaseJsonString", new object[1] { purchaseInfo.StorePurchaseJsonString });
				androidJavaObject.Call<bool>("add", new object[1] { androidJavaObject2 });
			}
			serviceClass.CallStatic("consumePurchases", androidJavaObject, purchaseForwardCallback);
		}

		public static void EnableDebugLogging(bool enable)
		{
			serviceClass.CallStatic("enableDebugLogging", enable, "");
		}

		public static void EnableDebugLogging(bool enable, string tag)
		{
			serviceClass.CallStatic("enableDebugLogging", enable, (tag == null) ? "" : tag);
		}

		internal static AndroidJavaObject javaArrayFromCS(string[] values)
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("java.lang.reflect.Array");
			AndroidJavaObject androidJavaObject = androidJavaClass.CallStatic<AndroidJavaObject>("newInstance", new object[2]
			{
				new AndroidJavaClass("java.lang.String"),
				values.Length
			});
			for (int i = 0; i < values.Length; i++)
			{
				androidJavaClass.CallStatic("set", androidJavaObject, i, new AndroidJavaObject("java.lang.String", values[i]));
			}
			return androidJavaObject;
		}

		internal static AndroidJavaObject javaArrayFromCSList(List<string> values)
		{
			if (values == null)
			{
				return null;
			}
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("java.lang.reflect.Array");
			AndroidJavaObject androidJavaObject = androidJavaClass.CallStatic<AndroidJavaObject>("newInstance", new object[2]
			{
				new AndroidJavaClass("java.lang.String"),
				values.Count
			});
			for (int i = 0; i < values.Count; i++)
			{
				androidJavaClass.CallStatic("set", androidJavaObject, i, new AndroidJavaObject("java.lang.String", values[i]));
			}
			return androidJavaObject;
		}
	}
}
