using System.Collections.Generic;
using UnityEngine.UDP.Common;

namespace UnityEngine.UDP
{
	public class PurchaseForwardCallback : AndroidJavaProxy
	{
		private IPurchaseListener purchaseListener;

		public PurchaseForwardCallback(IPurchaseListener purchaseListener)
			: base("com.unity.udp.sdk.PurchaseCallback")
		{
			this.purchaseListener = purchaseListener;
		}

		public void onPurchaseFinished(int resultCode, string message, string purchaseInfoString)
		{
			Debug.Log("Purchased Finished");
			if (purchaseListener == null)
			{
				return;
			}
			PurchaseInfo purchaseInfo = ConvertPurchaseInfo(purchaseInfoString);
			Debug.Log("purchaseInfoString is : " + purchaseInfoString);
			if (resultCode == 0)
			{
				MainThreadDispatcher.RunOnMainThread(delegate
				{
					purchaseListener.OnPurchase(purchaseInfo);
				});
			}
			else
			{
				MainThreadDispatcher.RunOnMainThread(delegate
				{
					purchaseListener.OnPurchaseFailed(message, purchaseInfo);
				});
			}
		}

		public void onConsumeFinished(int resultCode, string message, string purchaseInfoString)
		{
			if (purchaseListener == null)
			{
				return;
			}
			PurchaseInfo purchaseInfo = ConvertPurchaseInfo(purchaseInfoString);
			Debug.Log("onConsumeFinished, purchaseInfoString: " + purchaseInfoString);
			switch (resultCode)
			{
			case 0:
				MainThreadDispatcher.RunOnMainThread(delegate
				{
					purchaseListener.OnPurchaseConsume(purchaseInfo);
				});
				break;
			case -600:
				MainThreadDispatcher.RunOnMainThread(delegate
				{
					purchaseListener.OnPurchaseConsumeFailed(message, purchaseInfo);
				});
				break;
			}
		}

		public void onQueryInventory(int resultCode, string message, string inventoryString)
		{
			Debug.Log("onQueryInventory, inventoryString: " + inventoryString);
			switch (resultCode)
			{
			case 0:
			{
				Inventory inventory = ConvertInventory(inventoryString);
				MainThreadDispatcher.RunOnMainThread(delegate
				{
					purchaseListener.OnQueryInventory(inventory);
				});
				break;
			}
			case -700:
				MainThreadDispatcher.RunOnMainThread(delegate
				{
					purchaseListener.OnQueryInventoryFailed(message);
				});
				break;
			}
		}

		private static PurchaseInfo ConvertPurchaseInfo(string purchaseInfoString)
		{
			if (string.IsNullOrEmpty(purchaseInfoString))
			{
				return null;
			}
			return ConvertPurchaseInfo(MiniJson.JsonDecode(purchaseInfoString));
		}

		private static PurchaseInfo ConvertPurchaseInfo(Dictionary<string, object> purchaseInfoMap)
		{
			return new PurchaseInfo
			{
				GameOrderId = GetValueOfDictionary<string>(purchaseInfoMap, "gameOrderId", null),
				ItemType = GetValueOfDictionary<string>(purchaseInfoMap, "itemType", null),
				OrderQueryToken = GetValueOfDictionary<string>(purchaseInfoMap, "orderQueryToken", null),
				ProductId = GetValueOfDictionary<string>(purchaseInfoMap, "productId", null),
				StorePurchaseJsonString = GetValueOfDictionary<string>(purchaseInfoMap, "storePurchaseJsonString", null),
				DeveloperPayload = GetValueOfDictionary<string>(purchaseInfoMap, "developerPayload", null)
			};
		}

		private static ProductInfo ConvertProductInfo(string productInfoString)
		{
			if (string.IsNullOrEmpty(productInfoString))
			{
				return null;
			}
			return ConvertProductInfo(MiniJson.JsonDecode(productInfoString));
		}

		private static ProductInfo ConvertProductInfo(Dictionary<string, object> productInfoMap)
		{
			return new ProductInfo
			{
				Consumable = GetValueOfDictionary(productInfoMap, "consumable", false),
				Currency = GetValueOfDictionary<string>(productInfoMap, "currency", null),
				Description = GetValueOfDictionary<string>(productInfoMap, "description", null),
				ItemType = GetValueOfDictionary<string>(productInfoMap, "itemType", null),
				Price = GetValueOfDictionary<string>(productInfoMap, "price", null),
				PriceAmountMicros = GetValueOfDictionary(productInfoMap, "priceAmountMicros", 0L),
				ProductId = GetValueOfDictionary<string>(productInfoMap, "productId", null),
				Title = GetValueOfDictionary<string>(productInfoMap, "title", null)
			};
		}

		private static Inventory ConvertInventory(string inventoryString)
		{
			if (string.IsNullOrEmpty(inventoryString))
			{
				return null;
			}
			Inventory inventory = new Inventory();
			Dictionary<string, object> dictionary = MiniJson.JsonDecode(inventoryString);
			List<object> list = (List<object>)dictionary["purchases"];
			List<object> list2 = (List<object>)dictionary["products"];
			foreach (object item in list)
			{
				inventory.AddPurchase(ConvertPurchaseInfo((Dictionary<string, object>)item));
			}
			foreach (object item2 in list2)
			{
				inventory.AddProduct(ConvertProductInfo((Dictionary<string, object>)item2));
			}
			return inventory;
		}

		private static T GetValueOfDictionary<T>(IDictionary<string, object> dictionary, string key, T defaultValue)
		{
			if (dictionary.ContainsKey(key))
			{
				return (T)dictionary[key];
			}
			return defaultValue;
		}
	}
}
