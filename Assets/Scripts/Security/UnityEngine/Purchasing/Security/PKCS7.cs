using System;
using System.Collections.Generic;
using System.IO;
using LipingShare.LCLib.Asn1Processor;

namespace UnityEngine.Purchasing.Security
{
	internal class PKCS7
	{
		private Asn1Node root;

		private bool validStructure;

		public Asn1Node data { get; private set; }

		public List<SignerInfo> sinfos { get; private set; }

		public List<X509Cert> certChain { get; private set; }

		public static PKCS7 Load(byte[] data)
		{
			using (MemoryStream stream = new MemoryStream(data))
			{
				Asn1Parser asn1Parser = new Asn1Parser();
				asn1Parser.LoadData(stream);
				return new PKCS7(asn1Parser.RootNode);
			}
		}

		public PKCS7(Asn1Node node)
		{
			root = node;
			CheckStructure();
		}

		public bool Verify(X509Cert cert, DateTime certificateCreationTime)
		{
			if (validStructure)
			{
				bool flag = true;
				foreach (SignerInfo sinfo in sinfos)
				{
					X509Cert x509Cert = null;
					foreach (X509Cert item in certChain)
					{
						if (item.SerialNumber == sinfo.IssuerSerialNumber)
						{
							x509Cert = item;
							break;
						}
					}
					if (x509Cert != null && x509Cert.PubKey != null)
					{
						flag = flag && x509Cert.CheckCertTime(certificateCreationTime);
						flag = flag && x509Cert.PubKey.Verify(data.Data, sinfo.EncryptedDigest);
						flag = flag && ValidateChain(cert, x509Cert, certificateCreationTime);
					}
				}
				return flag && sinfos.Count > 0;
			}
			return false;
		}

		private bool ValidateChain(X509Cert root, X509Cert cert, DateTime certificateCreationTime)
		{
			if (cert.Issuer.Equals(root.Subject))
			{
				return cert.CheckSignature(root);
			}
			foreach (X509Cert item in certChain)
			{
				if (item != cert && item.Subject.Equals(cert.Issuer) && item.CheckCertTime(certificateCreationTime))
				{
					if (item.Issuer.Equals(root.Subject) && item.SerialNumber == root.SerialNumber)
					{
						return item.CheckSignature(root);
					}
					if (cert.CheckSignature(item))
					{
						return ValidateChain(root, item, certificateCreationTime);
					}
				}
			}
			return false;
		}

		private void CheckStructure()
		{
			validStructure = false;
			if ((root.Tag & 0x1F) != 16 || root.ChildNodeCount != 2)
			{
				return;
			}
			Asn1Node childNode = root.GetChildNode(0);
			if ((childNode.Tag & 0x1F) != 6 || childNode.GetDataStr(false) != "1.2.840.113549.1.7.2")
			{
				throw new InvalidPKCS7Data();
			}
			childNode = root.GetChildNode(1);
			if (childNode.ChildNodeCount != 1)
			{
				throw new InvalidPKCS7Data();
			}
			int num = 0;
			childNode = childNode.GetChildNode(num++);
			if (childNode.ChildNodeCount < 4 || (childNode.Tag & 0x1F) != 16)
			{
				throw new InvalidPKCS7Data();
			}
			Asn1Node childNode2 = childNode.GetChildNode(0);
			if ((childNode2.Tag & 0x1F) != 2)
			{
				throw new InvalidPKCS7Data();
			}
			childNode2 = childNode.GetChildNode(num++);
			if ((childNode2.Tag & 0x1F) != 17)
			{
				throw new InvalidPKCS7Data();
			}
			childNode2 = childNode.GetChildNode(num++);
			if ((childNode2.Tag & 0x1F) != 16 && childNode2.ChildNodeCount != 2)
			{
				throw new InvalidPKCS7Data();
			}
			data = childNode2.GetChildNode(1).GetChildNode(0);
			if (childNode.ChildNodeCount == 5)
			{
				certChain = new List<X509Cert>();
				childNode2 = childNode.GetChildNode(num++);
				if (childNode2.ChildNodeCount == 0)
				{
					throw new InvalidPKCS7Data();
				}
				for (int i = 0; i < childNode2.ChildNodeCount; i++)
				{
					certChain.Add(new X509Cert(childNode2.GetChildNode(i)));
				}
			}
			childNode2 = childNode.GetChildNode(num++);
			if ((childNode2.Tag & 0x1F) != 17 || childNode2.ChildNodeCount == 0)
			{
				throw new InvalidPKCS7Data();
			}
			sinfos = new List<SignerInfo>();
			for (int j = 0; j < childNode2.ChildNodeCount; j++)
			{
				sinfos.Add(new SignerInfo(childNode2.GetChildNode(j)));
			}
			validStructure = true;
		}
	}
}
