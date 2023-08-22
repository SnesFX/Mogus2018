using System;
using System.IO;
using System.Security.Cryptography;
using LipingShare.LCLib.Asn1Processor;

namespace UnityEngine.Purchasing.Security
{
	internal class RSAKey
	{
		public RSACryptoServiceProvider rsa { get; private set; }

		public RSAKey(Asn1Node n)
		{
			rsa = ParseNode(n);
		}

		public RSAKey(byte[] data)
		{
			using (MemoryStream stream = new MemoryStream(data))
			{
				Asn1Parser asn1Parser = new Asn1Parser();
				asn1Parser.LoadData(stream);
				rsa = ParseNode(asn1Parser.RootNode);
			}
		}

		public bool Verify(byte[] message, byte[] signature)
		{
			SHA1Managed sHA1Managed = new SHA1Managed();
			byte[] rgbHash = sHA1Managed.ComputeHash(message);
			return rsa.VerifyHash(rgbHash, null, signature);
		}

		private RSACryptoServiceProvider ParseNode(Asn1Node n)
		{
			if ((n.Tag & 0x1F) == 16 && n.ChildNodeCount == 2 && (n.GetChildNode(0).Tag & 0x1F) == 16 && (n.GetChildNode(0).GetChildNode(0).Tag & 0x1F) == 6 && n.GetChildNode(0).GetChildNode(0).GetDataStr(false) == "1.2.840.113549.1.1.1" && (n.GetChildNode(1).Tag & 0x1F) == 3)
			{
				Asn1Node childNode = n.GetChildNode(1).GetChildNode(0);
				if (childNode.ChildNodeCount == 2)
				{
					byte[] data = childNode.GetChildNode(0).Data;
					byte[] array = new byte[data.Length - 1];
					Array.Copy(data, 1, array, 0, data.Length - 1);
					string modulus = Convert.ToBase64String(array);
					string exponent = Convert.ToBase64String(childNode.GetChildNode(1).Data);
					RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
					rSACryptoServiceProvider.FromXmlString(ToXML(modulus, exponent));
					return rSACryptoServiceProvider;
				}
			}
			throw new InvalidRSAData();
		}

		private string ToXML(string modulus, string exponent)
		{
			return "<RSAKeyValue><Modulus>" + modulus + "</Modulus><Exponent>" + exponent + "</Exponent></RSAKeyValue>";
		}
	}
}
