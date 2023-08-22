using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LipingShare.LCLib.Asn1Processor;

namespace UnityEngine.Purchasing.Security
{
	public class AppleReceiptParser
	{
		public AppleReceipt Parse(byte[] receiptData)
		{
			PKCS7 receipt;
			return Parse(receiptData, out receipt);
		}

		internal AppleReceipt Parse(byte[] receiptData, out PKCS7 receipt)
		{
			using (MemoryStream stream = new MemoryStream(receiptData))
			{
				Asn1Parser asn1Parser = new Asn1Parser();
				asn1Parser.LoadData(stream);
				receipt = new PKCS7(asn1Parser.RootNode);
				return ParseReceipt(receipt.data);
			}
		}

		private AppleReceipt ParseReceipt(Asn1Node data)
		{
			if (data == null || data.ChildNodeCount != 1)
			{
				throw new InvalidPKCS7Data();
			}
			Asn1Node childNode = data.GetChildNode(0);
			AppleReceipt appleReceipt = new AppleReceipt();
			List<AppleInAppPurchaseReceipt> list = new List<AppleInAppPurchaseReceipt>();
			for (int i = 0; i < childNode.ChildNodeCount; i++)
			{
				Asn1Node childNode2 = childNode.GetChildNode(i);
				if (childNode2.ChildNodeCount != 3)
				{
					continue;
				}
				long num = Asn1Util.BytesToLong(childNode2.GetChildNode(0).Data);
				Asn1Node childNode3 = childNode2.GetChildNode(2);
				long num2 = num;
				if (num2 <= 12)
				{
					long num3 = num2 - 2;
					if ((ulong)num3 <= 3uL)
					{
						switch (num3)
						{
						case 0L:
							appleReceipt.bundleID = Encoding.UTF8.GetString(childNode3.GetChildNode(0).Data);
							continue;
						case 1L:
							appleReceipt.appVersion = Encoding.UTF8.GetString(childNode3.GetChildNode(0).Data);
							continue;
						case 2L:
							appleReceipt.opaque = childNode3.Data;
							continue;
						case 3L:
							appleReceipt.hash = childNode3.Data;
							continue;
						}
					}
					if (num2 == 12)
					{
						string @string = Encoding.UTF8.GetString(childNode3.GetChildNode(0).Data);
						appleReceipt.receiptCreationDate = DateTime.Parse(@string).ToUniversalTime();
					}
				}
				else
				{
					switch (num2)
					{
					case 17L:
						list.Add(ParseInAppReceipt(childNode3.GetChildNode(0)));
						break;
					case 19L:
						appleReceipt.originalApplicationVersion = Encoding.UTF8.GetString(childNode3.GetChildNode(0).Data);
						break;
					}
				}
			}
			appleReceipt.inAppPurchaseReceipts = list.ToArray();
			return appleReceipt;
		}

		private AppleInAppPurchaseReceipt ParseInAppReceipt(Asn1Node inApp)
		{
			AppleInAppPurchaseReceipt appleInAppPurchaseReceipt = new AppleInAppPurchaseReceipt();
			for (int i = 0; i < inApp.ChildNodeCount; i++)
			{
				Asn1Node childNode = inApp.GetChildNode(i);
				if (childNode.ChildNodeCount != 3)
				{
					continue;
				}
				long num = Asn1Util.BytesToLong(childNode.GetChildNode(0).Data);
				Asn1Node childNode2 = childNode.GetChildNode(2);
				long num2 = num;
				long num3 = num2 - 1701;
				if ((ulong)num3 <= 18uL)
				{
					switch (num3)
					{
					case 0L:
						appleInAppPurchaseReceipt.quantity = (int)Asn1Util.BytesToLong(childNode2.GetChildNode(0).Data);
						break;
					case 1L:
						appleInAppPurchaseReceipt.productID = Encoding.UTF8.GetString(childNode2.GetChildNode(0).Data);
						break;
					case 2L:
						appleInAppPurchaseReceipt.transactionID = Encoding.UTF8.GetString(childNode2.GetChildNode(0).Data);
						break;
					case 4L:
						appleInAppPurchaseReceipt.originalTransactionIdentifier = Encoding.UTF8.GetString(childNode2.GetChildNode(0).Data);
						break;
					case 3L:
						appleInAppPurchaseReceipt.purchaseDate = TryParseDateTimeNode(childNode2);
						break;
					case 5L:
						appleInAppPurchaseReceipt.originalPurchaseDate = TryParseDateTimeNode(childNode2);
						break;
					case 7L:
						appleInAppPurchaseReceipt.subscriptionExpirationDate = TryParseDateTimeNode(childNode2);
						break;
					case 11L:
						appleInAppPurchaseReceipt.cancellationDate = TryParseDateTimeNode(childNode2);
						break;
					case 6L:
						appleInAppPurchaseReceipt.productType = (int)Asn1Util.BytesToLong(childNode2.GetChildNode(0).Data);
						break;
					case 12L:
						appleInAppPurchaseReceipt.isFreeTrial = (int)Asn1Util.BytesToLong(childNode2.GetChildNode(0).Data);
						break;
					case 18L:
						appleInAppPurchaseReceipt.isIntroductoryPricePeriod = (int)Asn1Util.BytesToLong(childNode2.GetChildNode(0).Data);
						break;
					}
				}
			}
			return appleInAppPurchaseReceipt;
		}

		private static DateTime TryParseDateTimeNode(Asn1Node node)
		{
			string @string = Encoding.UTF8.GetString(node.GetChildNode(0).Data);
			if (!string.IsNullOrEmpty(@string))
			{
				return DateTime.Parse(@string).ToUniversalTime();
			}
			return DateTime.MinValue;
		}
	}
}
