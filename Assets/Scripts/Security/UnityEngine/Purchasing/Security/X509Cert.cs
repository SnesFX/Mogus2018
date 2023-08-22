using System;
using System.IO;
using System.Text;
using LipingShare.LCLib.Asn1Processor;

namespace UnityEngine.Purchasing.Security
{
	internal class X509Cert
	{
		private Asn1Node TbsCertificate;

		public byte[] rawTBSCertificate;

		public string SerialNumber { get; private set; }

		public DateTime ValidAfter { get; private set; }

		public DateTime ValidBefore { get; private set; }

		public RSAKey PubKey { get; private set; }

		public bool SelfSigned { get; private set; }

		public DistinguishedName Subject { get; private set; }

		public DistinguishedName Issuer { get; private set; }

		public Asn1Node Signature { get; private set; }

		public X509Cert(Asn1Node n)
		{
			ParseNode(n);
		}

		public X509Cert(byte[] data)
		{
			using (MemoryStream stream = new MemoryStream(data))
			{
				Asn1Parser asn1Parser = new Asn1Parser();
				asn1Parser.LoadData(stream);
				ParseNode(asn1Parser.RootNode);
			}
		}

		public bool CheckCertTime(DateTime time)
		{
			return time.CompareTo(ValidAfter) >= 0 && time.CompareTo(ValidBefore) <= 0;
		}

		public bool CheckSignature(X509Cert signer)
		{
			if (Issuer.Equals(signer.Subject))
			{
				return signer.PubKey.Verify(rawTBSCertificate, Signature.Data);
			}
			return false;
		}

		private void ParseNode(Asn1Node root)
		{
			if ((root.Tag & 0x1F) != 16 || root.ChildNodeCount != 3)
			{
				throw new InvalidX509Data();
			}
			TbsCertificate = root.GetChildNode(0);
			if (TbsCertificate.ChildNodeCount < 7)
			{
				throw new InvalidX509Data();
			}
			rawTBSCertificate = new byte[TbsCertificate.DataLength + 4];
			Array.Copy(root.Data, 0, rawTBSCertificate, 0, rawTBSCertificate.Length);
			Asn1Node childNode = TbsCertificate.GetChildNode(1);
			if ((childNode.Tag & 0x1F) != 2)
			{
				throw new InvalidX509Data();
			}
			SerialNumber = Asn1Util.ToHexString(childNode.Data);
			Issuer = new DistinguishedName(TbsCertificate.GetChildNode(3));
			Subject = new DistinguishedName(TbsCertificate.GetChildNode(5));
			Asn1Node childNode2 = TbsCertificate.GetChildNode(4);
			if ((childNode2.Tag & 0x1F) != 16 || childNode2.ChildNodeCount != 2)
			{
				throw new InvalidX509Data();
			}
			ValidAfter = ParseTime(childNode2.GetChildNode(0));
			ValidBefore = ParseTime(childNode2.GetChildNode(1));
			SelfSigned = Subject.Equals(Issuer);
			PubKey = new RSAKey(TbsCertificate.GetChildNode(6));
			Signature = root.GetChildNode(2);
		}

		private DateTime ParseTime(Asn1Node n)
		{
			string @string = new UTF8Encoding().GetString(n.Data);
			if (@string.Length != 13 && @string.Length != 15)
			{
				throw new InvalidTimeFormat();
			}
			if (@string[@string.Length - 1] != 'Z')
			{
				throw new InvalidTimeFormat();
			}
			int num = 0;
			int num2 = 0;
			if (@string.Length == 13)
			{
				num2 = int.Parse(@string.Substring(0, 2));
				if (num2 >= 50)
				{
					num2 += 1900;
				}
				else if (num2 < 50)
				{
					num2 += 2000;
				}
				num += 2;
			}
			else
			{
				num2 = int.Parse(@string.Substring(0, 4));
				num += 4;
			}
			int month = int.Parse(@string.Substring(num, 2));
			num += 2;
			int day = int.Parse(@string.Substring(num, 2));
			num += 2;
			int hour = int.Parse(@string.Substring(num, 2));
			num += 2;
			int minute = int.Parse(@string.Substring(num, 2));
			num += 2;
			int second = int.Parse(@string.Substring(num, 2));
			num += 2;
			return new DateTime(num2, month, day, hour, minute, second, DateTimeKind.Utc);
		}
	}
}
