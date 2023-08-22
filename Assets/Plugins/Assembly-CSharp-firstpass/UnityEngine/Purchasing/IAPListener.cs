using System;
using UnityEngine.Events;

namespace UnityEngine.Purchasing
{
	[AddComponentMenu("Unity IAP/IAP Listener")]
	[HelpURL("https://docs.unity3d.com/Manual/UnityIAP.html")]
	public class IAPListener : MonoBehaviour
	{
		[Serializable]
		public class OnPurchaseCompletedEvent : UnityEvent<Product>
		{
		}

		[Serializable]
		public class OnPurchaseFailedEvent : UnityEvent<Product, PurchaseFailureReason>
		{
		}

		[Tooltip("Consume successful purchases immediately")]
		public bool consumePurchase = true;

		[Tooltip("Preserve this GameObject when a new scene is loaded")]
		public bool dontDestroyOnLoad = true;

		[Tooltip("Event fired after a successful purchase of this product")]
		public OnPurchaseCompletedEvent onPurchaseComplete;

		[Tooltip("Event fired after a failed purchase of this product")]
		public OnPurchaseFailedEvent onPurchaseFailed;

		private void OnEnable()
		{
			if (dontDestroyOnLoad)
			{
				Object.DontDestroyOnLoad(base.gameObject);
			}
			CodelessIAPStoreListener.Instance.AddListener(this);
		}

		private void OnDisable()
		{
			CodelessIAPStoreListener.Instance.RemoveListener(this);
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
		{
			Debug.Log(string.Format("IAPListener.ProcessPurchase(PurchaseEventArgs {0} - {1})", e, e.purchasedProduct.definition.id));
			onPurchaseComplete.Invoke(e.purchasedProduct);
			return (!consumePurchase) ? PurchaseProcessingResult.Pending : PurchaseProcessingResult.Complete;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
		{
			Debug.Log(string.Format("IAPListener.OnPurchaseFailed(Product {0}, PurchaseFailureReason {1})", product, reason));
			onPurchaseFailed.Invoke(product, reason);
		}
	}
}
