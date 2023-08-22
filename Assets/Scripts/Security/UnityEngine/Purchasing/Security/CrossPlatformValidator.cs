using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Purchasing.Security
{
	public class CrossPlatformValidator
	{
		private GooglePlayValidator google;

		private UnityChannelValidator unityChannel;

		private AppleValidator apple;

		private string googleBundleId;

		private string appleBundleId;

		public CrossPlatformValidator(byte[] googlePublicKey, byte[] appleRootCert, string appBundleId)
			: this(googlePublicKey, appleRootCert, null, appBundleId, appBundleId, null)
		{
		}

		public CrossPlatformValidator(byte[] googlePublicKey, byte[] appleRootCert, byte[] unityChannelPublicKey, string appBundleId)
			: this(googlePublicKey, appleRootCert, unityChannelPublicKey, appBundleId, appBundleId, appBundleId)
		{
		}

		public CrossPlatformValidator(byte[] googlePublicKey, byte[] appleRootCert, string googleBundleId, string appleBundleId)
			: this(googlePublicKey, appleRootCert, null, googleBundleId, appleBundleId, null)
		{
		}

		public CrossPlatformValidator(byte[] googlePublicKey, byte[] appleRootCert, byte[] unityChannelPublicKey, string googleBundleId, string appleBundleId, string xiaomiBundleId_not_used)
		{
			try
			{
				if (googlePublicKey != null)
				{
					google = new GooglePlayValidator(googlePublicKey);
				}
				if (unityChannelPublicKey != null)
				{
					unityChannel = new UnityChannelValidator(unityChannelPublicKey);
				}
				if (appleRootCert != null)
				{
					apple = new AppleValidator(appleRootCert);
				}
			}
			catch (Exception ex)
			{
				throw new InvalidPublicKeyException("Cannot instantiate self with an invalid public key. (" + ex.ToString() + ")");
			}
			this.googleBundleId = googleBundleId;
			this.appleBundleId = appleBundleId;
		}

		public IPurchaseReceipt[] Validate(string unityIAPReceipt)
		{
			try
			{
				Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(unityIAPReceipt);
				if (dictionary == null)
				{
					throw new InvalidReceiptDataException();
				}
				string text = (string)dictionary["Store"];
				string text2 = (string)dictionary["Payload"];
				switch (text)
				{
				case "GooglePlay":
				{
					if (google == null)
					{
						throw new MissingStoreSecretException("Cannot validate a Google Play receipt without a Google Play public key.");
					}
					Dictionary<string, object> dictionary3 = (Dictionary<string, object>)MiniJson.JsonDecode(text2);
					string receipt2 = (string)dictionary3["json"];
					string signature2 = (string)dictionary3["signature"];
					GooglePlayReceipt googlePlayReceipt = google.Validate(receipt2, signature2);
					if (!googleBundleId.Equals(googlePlayReceipt.packageName))
					{
						throw new InvalidBundleIdException();
					}
					return new IPurchaseReceipt[1] { googlePlayReceipt };
				}
				case "XiaomiMiPay":
				{
					if (unityChannel == null)
					{
						throw new MissingStoreSecretException("Cannot validate a UnityChannel receipt without a UnityChannel public key.");
					}
					Dictionary<string, object> dictionary2 = (Dictionary<string, object>)MiniJson.JsonDecode(text2);
					string receipt = (string)dictionary2["json"];
					string signature = (string)dictionary2["signature"];
					UnityChannelReceipt unityChannelReceipt = unityChannel.Validate(receipt, signature);
					return new IPurchaseReceipt[1] { unityChannelReceipt };
				}
				case "AppleAppStore":
				case "MacAppStore":
				{
					if (apple == null)
					{
						throw new MissingStoreSecretException("Cannot validate an Apple receipt without supplying an Apple root certificate");
					}
					AppleReceipt appleReceipt = apple.Validate(Convert.FromBase64String(text2));
					if (!appleBundleId.Equals(appleReceipt.bundleID))
					{
						throw new InvalidBundleIdException();
					}
					return appleReceipt.inAppPurchaseReceipts.ToArray();
				}
				default:
					throw new StoreNotSupportedException("Store not supported: " + text);
				}
			}
			catch (IAPSecurityException ex)
			{
				throw ex;
			}
			catch (Exception ex2)
			{
				throw new GenericValidationException(string.Concat("Cannot validate due to unhandled exception. (", ex2, ")"));
			}
		}
	}
}
