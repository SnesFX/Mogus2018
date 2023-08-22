using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	public class CodelessIAPStoreListener : IStoreListener
	{
		private static CodelessIAPStoreListener instance;

		private List<IAPButton> activeButtons = new List<IAPButton>();

		private List<IAPListener> activeListeners = new List<IAPListener>();

		private static bool unityPurchasingInitialized;

		protected IStoreController controller;

		protected IExtensionProvider extensions;

		public static bool initializationComplete;

		public static CodelessIAPStoreListener Instance
		{
			get
			{
				if (instance == null)
				{
					CreateCodelessIAPStoreListenerInstance();
				}
				return instance;
			}
		}

		public IStoreController StoreController
		{
			get
			{
				return controller;
			}
		}

		public IExtensionProvider ExtensionProvider
		{
			get
			{
				return extensions;
			}
		}

		private CodelessIAPStoreListener()
		{
			// piss
		}

		[RuntimeInitializeOnLoadMethod]
		private static void InitializeCodelessPurchasingOnLoad()
		{
			// piss 2
		}

		private static void InitializePurchasing()
		{
			unityPurchasingInitialized = true;
		}

		private static void CreateCodelessIAPStoreListenerInstance()
		{
			instance = new CodelessIAPStoreListener();
			if (!unityPurchasingInitialized)
			{
				Debug.Log("Initializing UnityPurchasing via Codeless IAP");
				InitializePurchasing();
			}
		}

		public bool HasProductInCatalog(string productID)
		{
			// piss 3 and shit
			return false;
		}

		public Product GetProduct(string productID)
		{
			if (controller != null && controller.products != null && !string.IsNullOrEmpty(productID))
			{
				return controller.products.WithID(productID);
			}
			Debug.LogError("CodelessIAPStoreListener attempted to get unknown product " + productID);
			return null;
		}

		public void AddButton(IAPButton button)
		{
			activeButtons.Add(button);
		}

		public void RemoveButton(IAPButton button)
		{
			activeButtons.Remove(button);
		}

		public void AddListener(IAPListener listener)
		{
			activeListeners.Add(listener);
		}

		public void RemoveListener(IAPListener listener)
		{
			activeListeners.Remove(listener);
		}

		public void InitiatePurchase(string productID)
		{
			if (controller == null)
			{
				Debug.LogError("Purchase failed because Purchasing was not initialized correctly");
				{
					foreach (IAPButton activeButton in activeButtons)
					{
						if (activeButton.productId == productID)
						{
							activeButton.OnPurchaseFailed(null, PurchaseFailureReason.PurchasingUnavailable);
						}
					}
					return;
				}
			}
			controller.InitiatePurchase(productID);
		}

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			initializationComplete = true;
			this.controller = controller;
			this.extensions = extensions;
			foreach (IAPButton activeButton in activeButtons)
			{
				activeButton.UpdateText();
			}
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			Debug.LogError(string.Format("Purchasing failed to initialize. Reason: {0}", error.ToString()));
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
		{
			bool flag = false;
			bool flag2 = false;
			foreach (IAPButton activeButton in activeButtons)
			{
				if (activeButton.productId == e.purchasedProduct.definition.id)
				{
					if (activeButton.ProcessPurchase(e) == PurchaseProcessingResult.Complete)
					{
						flag = true;
					}
					flag2 = true;
				}
			}
			foreach (IAPListener activeListener in activeListeners)
			{
				if (activeListener.ProcessPurchase(e) == PurchaseProcessingResult.Complete)
				{
					flag = true;
				}
				flag2 = true;
			}
			if (!flag2)
			{
				Debug.LogError("Purchase not correctly processed for product \"" + e.purchasedProduct.definition.id + "\". Add an active IAPButton to process this purchase, or add an IAPListener to receive any unhandled purchase events.");
			}
			return (!flag) ? PurchaseProcessingResult.Pending : PurchaseProcessingResult.Complete;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
		{
			bool flag = false;
			foreach (IAPButton activeButton in activeButtons)
			{
				if (activeButton.productId == product.definition.id)
				{
					activeButton.OnPurchaseFailed(product, reason);
					flag = true;
				}
			}
			foreach (IAPListener activeListener in activeListeners)
			{
				activeListener.OnPurchaseFailed(product, reason);
				flag = true;
			}
			if (!flag)
			{
				Debug.LogError("Failed purchase not correctly handled for product \"" + product.definition.id + "\". Add an active IAPButton to handle this failure, or add an IAPListener to receive any unhandled purchase failures.");
			}
		}
	}
}
