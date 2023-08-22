using System;
using System.Collections.Generic;
using UDPEditor;
using UnityEngine;
using UnityEngine.UDP;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public class InitListener : IInitListener
	{
		public void OnInitialized(UserInfo userInfo)
		{
			Debug.Log("[Game]On Initialized suceeded");
			Show("Initialize succeeded");
			_initialized = true;
		}

		public void OnInitializeFailed(string message)
		{
			Debug.Log("[Game]OnInitializeFailed: " + message);
			Show("Initialize Failed: " + message);
		}
	}

	public class PurchaseListener : IPurchaseListener
	{
		public void OnPurchase(PurchaseInfo purchaseInfo)
		{
			string message = string.Format("[Game] Purchase Succeeded, productId: {0}, cpOrderId: {1}, developerPayload: {2}, storeJson: {3}", purchaseInfo.ProductId, purchaseInfo.GameOrderId, purchaseInfo.DeveloperPayload, purchaseInfo.StorePurchaseJsonString);
			Debug.Log(message);
			Show(message);
			if (m_consumeOnPurchase)
			{
				Debug.Log("Consuming");
				StoreService.ConsumePurchase(purchaseInfo, this);
			}
		}

		public void OnPurchaseFailed(string message, PurchaseInfo purchaseInfo)
		{
			Debug.Log("Purchase Failed: " + message);
			Show("Purchase Failed: " + message);
		}

		public void OnPurchaseRepeated(string productCode)
		{
			throw new NotImplementedException();
		}

		public void OnPurchaseConsume(PurchaseInfo purchaseInfo)
		{
			Show("Consume success for " + purchaseInfo.ProductId, true);
			Debug.Log("Consume success: " + purchaseInfo.ProductId);
		}

		public void OnMultiPurchaseConsume(List<bool> successful, List<PurchaseInfo> purchaseInfos, List<string> messages)
		{
			int count = successful.Count;
			for (int i = 0; i < count; i++)
			{
				if (successful[i])
				{
					string message = string.Format("Consuming succeeded for {0}\n", purchaseInfos[i].ProductId);
					Show(message, true);
					Debug.Log(message);
				}
				else
				{
					string message = string.Format("Consuming failed for {0}, reason: {1}", purchaseInfos[i].ProductId, messages[i]);
					Show(message, true);
					Debug.Log(message);
				}
			}
		}

		public void OnPurchaseConsumeFailed(string message, PurchaseInfo purchaseInfo)
		{
			Debug.Log("Consume Failed: " + message);
			Show("Consume Failed: " + message);
		}

		public void OnQueryInventory(Inventory inventory)
		{
			Debug.Log("OnQueryInventory");
			Debug.Log("[Game] Product List: ");
			string text = "Product List: \n";
			foreach (KeyValuePair<string, ProductInfo> item in inventory.GetProductDictionary())
			{
				Debug.Log("[Game] Returned product: " + item.Key + " " + item.Value.ProductId);
				text += string.Format("{0}:\n\tTitle: {1}\n\tDescription: {2}\n\tConsumable: {3}\n\tPrice: {4}\n\tCurrency: {5}\n\tPriceAmountMicros: {6}\n\tItemType: {7}\n", item.Key, item.Value.Title, item.Value.Description, item.Value.Consumable, item.Value.Price, item.Value.Currency, item.Value.PriceAmountMicros, item.Value.ItemType);
			}
			text += "\nPurchase List: \n";
			foreach (KeyValuePair<string, PurchaseInfo> item2 in inventory.GetPurchaseDictionary())
			{
				Debug.Log("[Game] Returned purchase: " + item2.Key);
				text += string.Format("{0}\n", item2.Value.ProductId);
			}
			Show(text);
			if (_consumeOnQuery)
			{
				StoreService.ConsumePurchase(inventory.GetPurchaseList(), this);
			}
		}

		public void OnQueryInventoryFailed(string message)
		{
			Debug.Log("OnQueryInventory Failed: " + message);
			Show("QueryInventory Failed: " + message);
		}
	}

	public string Product1;

	public string Product2;

	private static bool m_consumeOnPurchase;

	private static bool _consumeOnQuery;

	private Dropdown _dropdown;

	private List<Dropdown.OptionData> options;

	private static Text _textField;

	private static bool _initialized;

	private PurchaseListener purchaseListener;

	private InitListener initListener;

	private AppInfo appInfo;

	private void Start()
	{
		purchaseListener = new PurchaseListener();
		initListener = new InitListener();
		appInfo = new AppInfo();
		AppStoreSettings appStoreSettings = Resources.Load<AppStoreSettings>("GameSettings");
		appInfo.AppSlug = appStoreSettings.AppSlug;
		appInfo.ClientId = appStoreSettings.UnityClientID;
		appInfo.ClientKey = appStoreSettings.UnityClientKey;
		appInfo.RSAPublicKey = appStoreSettings.UnityClientRSAPublicKey;
		Debug.Log("App Name: " + appStoreSettings.AppName);
		GameObject gameObject = GameObject.Find("Information");
		_textField = gameObject.GetComponent<Text>();
		_textField.text = "Please Click Init to Start";
		gameObject = GameObject.Find("Dropdown");
		_dropdown = gameObject.GetComponent<Dropdown>();
		_dropdown.ClearOptions();
		_dropdown.options.Add(new Dropdown.OptionData(Product1));
		_dropdown.options.Add(new Dropdown.OptionData(Product2));
		_dropdown.RefreshShownValue();
		InitUI();
	}

	private static void Show(string message, bool append = false)
	{
		_textField.text = ((!append) ? message : string.Format("{0}\n{1}", _textField.text, message));
	}

	private void InitUI()
	{
		GetButton("InitButton").onClick.AddListener(delegate
		{
			_initialized = false;
			Debug.Log("Init button is clicked.");
			Show("Initializing");
			StoreService.Initialize(initListener);
		});
		GetButton("BuyButton").onClick.AddListener(delegate
		{
			if (!_initialized)
			{
				Show("Please Initialize first");
			}
			else
			{
				string text2 = _dropdown.options[_dropdown.value].text;
				Debug.Log("Buy button is clicked.");
				Show("Buying Product: " + text2);
				m_consumeOnPurchase = false;
				Debug.Log(_dropdown.options[_dropdown.value].text + " will be bought");
				StoreService.Purchase(text2, null, "{\"AnyKeyYouWant:\" : \"AnyValueYouWant\"}", purchaseListener);
			}
		});
		GetButton("BuyConsumeButton").onClick.AddListener(delegate
		{
			if (!_initialized)
			{
				Show("Please Initialize first");
			}
			else
			{
				string text = _dropdown.options[_dropdown.value].text;
				Show("Buying Product: " + text);
				Debug.Log("Buy&Consume button is clicked.");
				m_consumeOnPurchase = true;
				StoreService.Purchase(text, null, "buy and consume developer payload", purchaseListener);
			}
		});
		List<string> productIds = new List<string> { Product1, Product2 };
		GetButton("QueryButton").onClick.AddListener(delegate
		{
			if (!_initialized)
			{
				Show("Please Initialize first");
			}
			else
			{
				_consumeOnQuery = false;
				Debug.Log("Query button is clicked.");
				Show("Querying Inventory");
				StoreService.QueryInventory(productIds, purchaseListener);
			}
		});
		GetButton("QueryConsumeButton").onClick.AddListener(delegate
		{
			if (!_initialized)
			{
				Show("Please Initialize first");
			}
			else
			{
				_consumeOnQuery = true;
				Show("Querying Inventory");
				Debug.Log("QueryConsume button is clicked.");
				StoreService.QueryInventory(productIds, purchaseListener);
			}
		});
	}

	private Button GetButton(string buttonName)
	{
		GameObject gameObject = GameObject.Find(buttonName);
		if (gameObject != null)
		{
			return gameObject.GetComponent<Button>();
		}
		return null;
	}
}
