using System;
using System.IO;

namespace LipingShare.LCLib.Asn1Processor
{
	internal class Asn1Parser
	{
		private byte[] rawData;

		private Asn1Node rootNode = new Asn1Node();

		private bool ParseEncapsulatedData
		{
			get
			{
				return rootNode.ParseEncapsulatedData;
			}
			set
			{
				rootNode.ParseEncapsulatedData = value;
			}
		}

		public byte[] RawData
		{
			get
			{
				return rawData;
			}
		}

		public Asn1Node RootNode
		{
			get
			{
				return rootNode;
			}
		}

		public void LoadData(string fileName)
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Open);
			rawData = new byte[fileStream.Length];
			fileStream.Read(rawData, 0, (int)fileStream.Length);
			fileStream.Close();
			MemoryStream stream = new MemoryStream(rawData);
			LoadData(stream);
		}

		public void LoadPemData(string fileName)
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Open);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			fileStream.Close();
			string pemStr = Asn1Util.BytesToString(array);
			if (Asn1Util.IsPemFormated(pemStr))
			{
				Stream stream = Asn1Util.PemToStream(pemStr);
				stream.Position = 0L;
				LoadData(stream);
				return;
			}
			throw new Exception("It is a invalid PEM file: " + fileName);
		}

		public void LoadData(Stream stream)
		{
			stream.Position = 0L;
			if (!rootNode.LoadData(stream))
			{
				throw new ArgumentException("Failed to load data.");
			}
			rawData = new byte[stream.Length];
			stream.Position = 0L;
			stream.Read(rawData, 0, rawData.Length);
		}

		public void SaveData(string fileName)
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Create);
			rootNode.SaveData(fileStream);
			fileStream.Close();
		}

		public Asn1Node GetNodeByPath(string nodePath)
		{
			return rootNode.GetDescendantNodeByPath(nodePath);
		}

		public Asn1Node GetNodeByOid(string oid)
		{
			return Asn1Node.GetDecendantNodeByOid(oid, rootNode);
		}

		public static string GetNodeTextHeader(int lineLen)
		{
			string text = string.Format("Offset| Len  |LenByte|\r\n");
			return text + "======+======+=======+" + Asn1Util.GenStr(lineLen + 10, '=') + "\r\n";
		}

		public override string ToString()
		{
			return GetNodeText(rootNode, 100);
		}

		public static string GetNodeText(Asn1Node node, int lineLen)
		{
			string nodeTextHeader = GetNodeTextHeader(lineLen);
			return nodeTextHeader + node.GetText(node, lineLen);
		}
	}
}
