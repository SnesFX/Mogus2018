using System;
using System.Collections;
using System.IO;
using System.Text;

namespace LipingShare.LCLib.Asn1Processor
{
	internal class Asn1Node : IAsn1Node
	{
		public class TagTextMask
		{
			public const uint SHOW_OFFSET = 1u;

			public const uint SHOW_DATA = 2u;

			public const uint USE_HEX_OFFSET = 4u;

			public const uint SHOW_TAG_NUMBER = 8u;

			public const uint SHOW_PATH = 16u;
		}

		private byte tag;

		private long dataOffset;

		private long dataLength;

		private long lengthFieldBytes;

		private byte[] data;

		private ArrayList childNodeList;

		private byte unusedBits;

		private long deepness;

		private string path = "";

		private const int indentStep = 3;

		private Asn1Node parentNode;

		private bool requireRecalculatePar = true;

		private bool isIndefiniteLength = false;

		private bool parseEncapsulatedData = true;

		public const int defaultLineLen = 80;

		public const int minLineLen = 60;

		public const int TagLength = 1;

		public const int BitStringUnusedFiledLength = 1;

		public bool IsIndefiniteLength
		{
			get
			{
				return isIndefiniteLength;
			}
			set
			{
				isIndefiniteLength = value;
			}
		}

		public byte Tag
		{
			get
			{
				return tag;
			}
			set
			{
				tag = value;
			}
		}

		public byte MaskedTag
		{
			get
			{
				return (byte)(tag & 0x1Fu);
			}
		}

		public bool IsEmptyData
		{
			get
			{
				if (data == null)
				{
					return true;
				}
				if (data.Length < 1)
				{
					return true;
				}
				return false;
			}
		}

		public long ChildNodeCount
		{
			get
			{
				return childNodeList.Count;
			}
		}

		public string TagName
		{
			get
			{
				return Asn1Util.GetTagName(tag);
			}
		}

		public Asn1Node ParentNode
		{
			get
			{
				return parentNode;
			}
		}

		public string Path
		{
			get
			{
				return path;
			}
		}

		public long DataLength
		{
			get
			{
				return dataLength;
			}
		}

		public long LengthFieldBytes
		{
			get
			{
				return lengthFieldBytes;
			}
		}

		public byte[] Data
		{
			get
			{
				MemoryStream memoryStream = new MemoryStream();
				long childNodeCount = ChildNodeCount;
				if (childNodeCount == 0)
				{
					if (data != null)
					{
						memoryStream.Write(data, 0, data.Length);
					}
				}
				else
				{
					for (int i = 0; i < childNodeCount; i++)
					{
						Asn1Node childNode = GetChildNode(i);
						childNode.SaveData(memoryStream);
					}
				}
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				return array;
			}
			set
			{
				SetData(value);
			}
		}

		public long Deepness
		{
			get
			{
				return deepness;
			}
		}

		public long DataOffset
		{
			get
			{
				return dataOffset;
			}
		}

		public byte UnusedBits
		{
			get
			{
				return unusedBits;
			}
			set
			{
				unusedBits = value;
			}
		}

		protected bool RequireRecalculatePar
		{
			get
			{
				return requireRecalculatePar;
			}
			set
			{
				requireRecalculatePar = value;
			}
		}

		public bool ParseEncapsulatedData
		{
			get
			{
				return parseEncapsulatedData;
			}
			set
			{
				if (parseEncapsulatedData == value)
				{
					return;
				}
				byte[] buffer = Data;
				parseEncapsulatedData = value;
				ClearAll();
				if ((tag & 0x20u) != 0 || parseEncapsulatedData)
				{
					MemoryStream memoryStream = new MemoryStream(buffer);
					memoryStream.Position = 0L;
					bool flag = true;
					while (memoryStream.Position < memoryStream.Length)
					{
						Asn1Node asn1Node = new Asn1Node();
						asn1Node.ParseEncapsulatedData = parseEncapsulatedData;
						if (!asn1Node.LoadData(memoryStream))
						{
							ClearAll();
							flag = false;
							break;
						}
						AddChild(asn1Node);
					}
					if (!flag)
					{
						Data = buffer;
					}
				}
				else
				{
					Data = buffer;
				}
			}
		}

		private Asn1Node(Asn1Node parentNode, long dataOffset)
		{
			Init();
			deepness = parentNode.Deepness + 1;
			this.parentNode = parentNode;
			this.dataOffset = dataOffset;
		}

		private void Init()
		{
			childNodeList = new ArrayList();
			data = null;
			dataLength = 0L;
			lengthFieldBytes = 0L;
			unusedBits = 0;
			tag = 48;
			childNodeList.Clear();
			deepness = 0L;
			parentNode = null;
		}

		private string GetHexPrintingStr(Asn1Node startNode, string baseLine, string lStr, int lineLen)
		{
			string text = "";
			string indentStr = GetIndentStr(startNode);
			string text2 = Asn1Util.ToHexString(data);
			text = ((text2.Length <= 0) ? (text + baseLine) : ((baseLine.Length + text2.Length >= lineLen) ? (text + baseLine + FormatLineHexString(lStr, indentStr.Length, lineLen, text2)) : (text + baseLine + "'" + text2 + "'")));
			return text + "\r\n";
		}

		private string FormatLineString(string lStr, int indent, int lineLen, string msg)
		{
			string text = "";
			indent += 3;
			int num = lineLen - indent;
			int len = indent;
			for (int i = 0; i < msg.Length; i += num)
			{
				text = ((i + num <= msg.Length) ? (text + "\r\n" + lStr + Asn1Util.GenStr(len, ' ') + "'" + msg.Substring(i, num) + "'") : (text + "\r\n" + lStr + Asn1Util.GenStr(len, ' ') + "'" + msg.Substring(i, msg.Length - i) + "'"));
			}
			return text;
		}

		private string FormatLineHexString(string lStr, int indent, int lineLen, string msg)
		{
			string text = "";
			indent += 3;
			int num = lineLen - indent;
			int len = indent;
			for (int i = 0; i < msg.Length; i += num)
			{
				text = ((i + num <= msg.Length) ? (text + "\r\n" + lStr + Asn1Util.GenStr(len, ' ') + msg.Substring(i, num)) : (text + "\r\n" + lStr + Asn1Util.GenStr(len, ' ') + msg.Substring(i, msg.Length - i)));
			}
			return text;
		}

		public Asn1Node()
		{
			Init();
			dataOffset = 0L;
		}

		public Asn1Node Clone()
		{
			MemoryStream memoryStream = new MemoryStream();
			SaveData(memoryStream);
			memoryStream.Position = 0L;
			Asn1Node asn1Node = new Asn1Node();
			asn1Node.LoadData(memoryStream);
			return asn1Node;
		}

		public bool LoadData(byte[] byteData)
		{
			bool flag = true;
			try
			{
				MemoryStream memoryStream = new MemoryStream(byteData);
				memoryStream.Position = 0L;
				flag = LoadData(memoryStream);
				memoryStream.Close();
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		public static long GetDescendantNodeCount(Asn1Node node)
		{
			long num = 0L;
			num += node.ChildNodeCount;
			for (int i = 0; i < node.ChildNodeCount; i++)
			{
				num += GetDescendantNodeCount(node.GetChildNode(i));
			}
			return num;
		}

		public bool LoadData(Stream xdata)
		{
			bool flag = false;
			try
			{
				RequireRecalculatePar = false;
				return InternalLoadData(xdata);
			}
			finally
			{
				RequireRecalculatePar = true;
				RecalculateTreePar();
			}
		}

		public byte[] GetRawData()
		{
			MemoryStream memoryStream = new MemoryStream();
			SaveData(memoryStream);
			byte[] array = new byte[memoryStream.Length];
			memoryStream.Position = 0L;
			memoryStream.Read(array, 0, (int)memoryStream.Length);
			memoryStream.Close();
			return array;
		}

		public bool SaveData(Stream xdata)
		{
			bool result = true;
			long childNodeCount = ChildNodeCount;
			xdata.WriteByte(tag);
			Asn1Util.DERLengthEncode(xdata, (ulong)dataLength);
			if (tag == 3)
			{
				xdata.WriteByte(unusedBits);
			}
			if (childNodeCount == 0)
			{
				if (data != null)
				{
					xdata.Write(data, 0, data.Length);
				}
			}
			else
			{
				for (int i = 0; i < childNodeCount; i++)
				{
					Asn1Node childNode = GetChildNode(i);
					result = childNode.SaveData(xdata);
				}
			}
			return result;
		}

		public void ClearAll()
		{
			data = null;
			for (int i = 0; i < childNodeList.Count; i++)
			{
				Asn1Node asn1Node = (Asn1Node)childNodeList[i];
				asn1Node.ClearAll();
			}
			childNodeList.Clear();
			RecalculateTreePar();
		}

		public void AddChild(Asn1Node xdata)
		{
			childNodeList.Add(xdata);
			RecalculateTreePar();
		}

		public int InsertChild(Asn1Node xdata, int index)
		{
			childNodeList.Insert(index, xdata);
			RecalculateTreePar();
			return index;
		}

		public int InsertChild(Asn1Node xdata, Asn1Node indexNode)
		{
			int num = childNodeList.IndexOf(indexNode);
			childNodeList.Insert(num, xdata);
			RecalculateTreePar();
			return num;
		}

		public int InsertChildAfter(Asn1Node xdata, Asn1Node indexNode)
		{
			int num = childNodeList.IndexOf(indexNode) + 1;
			childNodeList.Insert(num, xdata);
			RecalculateTreePar();
			return num;
		}

		public int InsertChildAfter(Asn1Node xdata, int index)
		{
			int num = index + 1;
			childNodeList.Insert(num, xdata);
			RecalculateTreePar();
			return num;
		}

		public Asn1Node RemoveChild(int index)
		{
			Asn1Node asn1Node = null;
			if (index < childNodeList.Count - 1)
			{
				asn1Node = (Asn1Node)childNodeList[index + 1];
			}
			childNodeList.RemoveAt(index);
			if (asn1Node == null)
			{
				asn1Node = ((childNodeList.Count <= 0) ? this : ((Asn1Node)childNodeList[childNodeList.Count - 1]));
			}
			RecalculateTreePar();
			return asn1Node;
		}

		public Asn1Node RemoveChild(Asn1Node node)
		{
			Asn1Node asn1Node = null;
			int index = childNodeList.IndexOf(node);
			return RemoveChild(index);
		}

		public Asn1Node GetChildNode(int index)
		{
			Asn1Node result = null;
			if (index < ChildNodeCount)
			{
				result = (Asn1Node)childNodeList[index];
			}
			return result;
		}

		public string GetText(Asn1Node startNode, int lineLen)
		{
			string text = "";
			string text2 = "";
			string text3 = "";
			switch (tag)
			{
			case 3:
				text2 = string.Format("{0,6}|{1,6}|{2,7}|{3} {4} UnusedBits:{5} : ", dataOffset, dataLength, lengthFieldBytes, GetIndentStr(startNode), TagName, unusedBits);
				text3 = Asn1Util.ToHexString(data);
				text = ((text2.Length + text3.Length >= lineLen) ? (text + text2 + FormatLineHexString("      |      |       | ", GetIndentStr(startNode).Length, lineLen, text3 + "\r\n")) : ((text3.Length >= 1) ? (text + text2 + "'" + text3 + "'\r\n") : (text + text2 + "\r\n")));
				break;
			case 6:
			{
				Oid oid = new Oid();
				string text4 = oid.Decode(new MemoryStream(data));
				string text5 = oid.GetOidName(text4);
				text += string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : {5} [{6}]\r\n", dataOffset, dataLength, lengthFieldBytes, GetIndentStr(startNode), TagName, text5, text4);
				break;
			}
			case 13:
			{
				RelativeOid relativeOid = new RelativeOid();
				string text4 = relativeOid.Decode(new MemoryStream(data));
				string text5 = "";
				text += string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : {5} [{6}]\r\n", dataOffset, dataLength, lengthFieldBytes, GetIndentStr(startNode), TagName, text5, text4);
				break;
			}
			case 12:
			case 18:
			case 19:
			case 22:
			case 23:
			case 24:
			case 26:
			case 27:
			case 28:
			case 30:
				text2 = string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : ", dataOffset, dataLength, lengthFieldBytes, GetIndentStr(startNode), TagName);
				if (tag == 12)
				{
					UTF8Encoding uTF8Encoding = new UTF8Encoding();
					text3 = uTF8Encoding.GetString(data);
				}
				else
				{
					text3 = Asn1Util.BytesToString(data);
				}
				text = ((text2.Length + text3.Length >= lineLen) ? (text + text2 + FormatLineString("      |      |       | ", GetIndentStr(startNode).Length, lineLen, text3) + "\r\n") : (text + text2 + "'" + text3 + "'\r\n"));
				break;
			case 2:
				if (data != null && dataLength < 8)
				{
					text += string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : {5}\r\n", dataOffset, dataLength, lengthFieldBytes, GetIndentStr(startNode), TagName, Asn1Util.BytesToLong(data).ToString());
				}
				else
				{
					text2 = string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : ", dataOffset, dataLength, lengthFieldBytes, GetIndentStr(startNode), TagName);
					text += GetHexPrintingStr(startNode, text2, "      |      |       | ", lineLen);
				}
				break;
			default:
				if ((tag & 0x1F) == 6)
				{
					text2 = string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : ", dataOffset, dataLength, lengthFieldBytes, GetIndentStr(startNode), TagName);
					text3 = Asn1Util.BytesToString(data);
					text = ((text2.Length + text3.Length >= lineLen) ? (text + text2 + FormatLineString("      |      |       | ", GetIndentStr(startNode).Length, lineLen, text3) + "\r\n") : (text + text2 + "'" + text3 + "'\r\n"));
				}
				else
				{
					text2 = string.Format("{0,6}|{1,6}|{2,7}|{3} {4} : ", dataOffset, dataLength, lengthFieldBytes, GetIndentStr(startNode), TagName);
					text += GetHexPrintingStr(startNode, text2, "      |      |       | ", lineLen);
				}
				break;
			}
			if (childNodeList.Count >= 0)
			{
				text += GetListStr(startNode, lineLen);
			}
			return text;
		}

		public string GetDataStr(bool pureHexMode)
		{
			string text = "";
			if (pureHexMode)
			{
				return Asn1Util.FormatString(Asn1Util.ToHexString(data), 32, 2);
			}
			switch (tag)
			{
			case 3:
				return Asn1Util.FormatString(Asn1Util.ToHexString(data), 32, 2);
			case 6:
			{
				Oid oid = new Oid();
				return oid.Decode(new MemoryStream(data));
			}
			case 13:
			{
				RelativeOid relativeOid = new RelativeOid();
				return relativeOid.Decode(new MemoryStream(data));
			}
			case 18:
			case 19:
			case 22:
			case 23:
			case 24:
			case 26:
			case 27:
			case 28:
			case 30:
				return Asn1Util.BytesToString(data);
			case 12:
			{
				UTF8Encoding uTF8Encoding = new UTF8Encoding();
				return uTF8Encoding.GetString(data);
			}
			case 2:
				return Asn1Util.FormatString(Asn1Util.ToHexString(data), 32, 2);
			default:
				if ((tag & 0x1F) == 6)
				{
					return Asn1Util.BytesToString(data);
				}
				return Asn1Util.FormatString(Asn1Util.ToHexString(data), 32, 2);
			}
		}

		public string GetLabel(uint mask)
		{
			string text = "";
			string text2 = "";
			string text3 = "";
			text3 = (((mask & 4u) != 0) ? (((mask & 8) == 0) ? string.Format("(0x{0:X6},0x{1:X4})", dataOffset, dataLength) : string.Format("(0x{0:X2},0x{1:X6},0x{2:X4})", tag, dataOffset, dataLength)) : (((mask & 8) == 0) ? string.Format("({0},{1})", dataOffset, dataLength) : string.Format("({0},{1},{2})", tag, dataOffset, dataLength)));
			switch (tag)
			{
			case 3:
				if ((mask & (true ? 1u : 0u)) != 0)
				{
					text += text3;
				}
				text = text + " " + TagName + " UnusedBits: " + unusedBits;
				if ((mask & 2u) != 0)
				{
					text2 = Asn1Util.ToHexString(data);
					text += ((text2.Length > 0) ? (" : '" + text2 + "'") : "");
				}
				break;
			case 6:
			{
				Oid oid = new Oid();
				string text4 = oid.Decode(data);
				string text5 = oid.GetOidName(text4);
				if ((mask & (true ? 1u : 0u)) != 0)
				{
					text += text3;
				}
				text = text + " " + TagName;
				text = text + " : " + text5;
				if ((mask & 2u) != 0)
				{
					text += ((text4.Length > 0) ? (" : '" + text4 + "'") : "");
				}
				break;
			}
			case 13:
			{
				RelativeOid relativeOid = new RelativeOid();
				string text4 = relativeOid.Decode(data);
				string text5 = "";
				if ((mask & (true ? 1u : 0u)) != 0)
				{
					text += text3;
				}
				text = text + " " + TagName;
				text = text + " : " + text5;
				if ((mask & 2u) != 0)
				{
					text += ((text4.Length > 0) ? (" : '" + text4 + "'") : "");
				}
				break;
			}
			case 12:
			case 18:
			case 19:
			case 22:
			case 23:
			case 24:
			case 26:
			case 27:
			case 28:
			case 30:
				if ((mask & (true ? 1u : 0u)) != 0)
				{
					text += text3;
				}
				text = text + " " + TagName;
				if ((mask & 2u) != 0)
				{
					if (tag == 12)
					{
						UTF8Encoding uTF8Encoding = new UTF8Encoding();
						text2 = uTF8Encoding.GetString(data);
					}
					else
					{
						text2 = Asn1Util.BytesToString(data);
					}
					text += ((text2.Length > 0) ? (" : '" + text2 + "'") : "");
				}
				break;
			case 2:
				if ((mask & (true ? 1u : 0u)) != 0)
				{
					text += text3;
				}
				text = text + " " + TagName;
				if ((mask & 2u) != 0)
				{
					text2 = ((data == null || dataLength >= 8) ? Asn1Util.ToHexString(data) : Asn1Util.BytesToLong(data).ToString());
					text += ((text2.Length > 0) ? (" : '" + text2 + "'") : "");
				}
				break;
			default:
				if ((mask & (true ? 1u : 0u)) != 0)
				{
					text += text3;
				}
				text = text + " " + TagName;
				if ((mask & 2u) != 0)
				{
					text2 = (((tag & 0x1F) != 6) ? Asn1Util.ToHexString(data) : Asn1Util.BytesToString(data));
					text += ((text2.Length > 0) ? (" : '" + text2 + "'") : "");
				}
				break;
			}
			if ((mask & 0x10u) != 0)
			{
				text = "(" + path + ") " + text;
			}
			return text;
		}

		public Asn1Node GetDescendantNodeByPath(string nodePath)
		{
			Asn1Node asn1Node = this;
			if (nodePath == null)
			{
				return asn1Node;
			}
			nodePath = nodePath.TrimEnd().TrimStart();
			if (nodePath.Length < 1)
			{
				return asn1Node;
			}
			string[] array = nodePath.Split('/');
			try
			{
				for (int i = 1; i < array.Length; i++)
				{
					asn1Node = asn1Node.GetChildNode(Convert.ToInt32(array[i]));
				}
			}
			catch
			{
				asn1Node = null;
			}
			return asn1Node;
		}

		public static Asn1Node GetDecendantNodeByOid(string oid, Asn1Node startNode)
		{
			Asn1Node asn1Node = null;
			Oid oid2 = new Oid();
			for (int i = 0; i < startNode.ChildNodeCount; i++)
			{
				Asn1Node childNode = startNode.GetChildNode(i);
				int num = childNode.tag & 0x1F;
				if (num == 6 && oid == oid2.Decode(childNode.Data))
				{
					asn1Node = childNode;
					break;
				}
				asn1Node = GetDecendantNodeByOid(oid, childNode);
				if (asn1Node != null)
				{
					break;
				}
			}
			return asn1Node;
		}

		protected void RecalculateTreePar()
		{
			if (requireRecalculatePar)
			{
				Asn1Node asn1Node = this;
				while (asn1Node.ParentNode != null)
				{
					asn1Node = asn1Node.ParentNode;
				}
				ResetBranchDataLength(asn1Node);
				asn1Node.dataOffset = 0L;
				asn1Node.deepness = 0L;
				long subOffset = asn1Node.dataOffset + 1 + asn1Node.lengthFieldBytes;
				ResetChildNodePar(asn1Node, subOffset);
			}
		}

		protected static long ResetBranchDataLength(Asn1Node node)
		{
			long num = 0L;
			long num2 = 0L;
			if (node.ChildNodeCount < 1)
			{
				if (node.data != null)
				{
					num2 += node.data.Length;
				}
			}
			else
			{
				for (int i = 0; i < node.ChildNodeCount; i++)
				{
					num2 += ResetBranchDataLength(node.GetChildNode(i));
				}
			}
			node.dataLength = num2;
			if (node.tag == 3)
			{
				node.dataLength++;
			}
			ResetDataLengthFieldWidth(node);
			return node.dataLength + 1 + node.lengthFieldBytes;
		}

		protected static void ResetDataLengthFieldWidth(Asn1Node node)
		{
			MemoryStream memoryStream = new MemoryStream();
			Asn1Util.DERLengthEncode(memoryStream, (ulong)node.dataLength);
			node.lengthFieldBytes = memoryStream.Length;
			memoryStream.Close();
		}

		protected void ResetChildNodePar(Asn1Node xNode, long subOffset)
		{
			if (xNode.tag == 3)
			{
				subOffset++;
			}
			for (int i = 0; i < xNode.ChildNodeCount; i++)
			{
				Asn1Node childNode = xNode.GetChildNode(i);
				childNode.parentNode = xNode;
				childNode.dataOffset = subOffset;
				childNode.deepness = xNode.deepness + 1;
				childNode.path = xNode.path + "/" + i;
				subOffset += 1 + childNode.lengthFieldBytes;
				ResetChildNodePar(childNode, subOffset);
				subOffset += childNode.dataLength;
			}
		}

		protected string GetListStr(Asn1Node startNode, int lineLen)
		{
			string text = "";
			for (int i = 0; i < childNodeList.Count; i++)
			{
				Asn1Node asn1Node = (Asn1Node)childNodeList[i];
				text += asn1Node.GetText(startNode, lineLen);
			}
			return text;
		}

		protected string GetIndentStr(Asn1Node startNode)
		{
			string text = "";
			long num = 0L;
			if (startNode != null)
			{
				num = startNode.Deepness;
			}
			for (long num2 = 0L; num2 < deepness - num; num2++)
			{
				text += "   ";
			}
			return text;
		}

		protected bool GeneralDecode(Stream xdata)
		{
			bool result = false;
			long num = xdata.Length - xdata.Position;
			tag = (byte)xdata.ReadByte();
			long position = xdata.Position;
			dataLength = Asn1Util.DerLengthDecode(xdata, ref isIndefiniteLength);
			if (dataLength < 0)
			{
				return result;
			}
			long position2 = xdata.Position;
			lengthFieldBytes = position2 - position;
			if (num < dataLength + 1 + lengthFieldBytes)
			{
				return result;
			}
			if ((ParentNode == null || (ParentNode.tag & 0x20) == 0) && ((tag & 0x1F) <= 0 || (tag & 0x1F) > 30))
			{
				return result;
			}
			if (tag == 3)
			{
				if (dataLength < 1)
				{
					return result;
				}
				unusedBits = (byte)xdata.ReadByte();
				data = new byte[dataLength - 1];
				xdata.Read(data, 0, (int)(dataLength - 1));
			}
			else
			{
				data = new byte[dataLength];
				xdata.Read(data, 0, (int)dataLength);
			}
			return true;
		}

		protected bool ListDecode(Stream xdata)
		{
			bool flag = false;
			long position = xdata.Position;
			try
			{
				long num = xdata.Length - xdata.Position;
				tag = (byte)xdata.ReadByte();
				long position2 = xdata.Position;
				dataLength = Asn1Util.DerLengthDecode(xdata, ref isIndefiniteLength);
				if (dataLength < 0 || num < dataLength)
				{
					return flag;
				}
				long position3 = xdata.Position;
				lengthFieldBytes = position3 - position2;
				long num2 = dataOffset + 1 + lengthFieldBytes;
				if (tag == 3)
				{
					unusedBits = (byte)xdata.ReadByte();
					dataLength--;
					num2++;
				}
				if (dataLength <= 0)
				{
					return flag;
				}
				Stream stream = new MemoryStream((int)dataLength);
				byte[] array = new byte[dataLength];
				xdata.Read(array, 0, (int)dataLength);
				if (tag == 3)
				{
					dataLength++;
				}
				stream.Write(array, 0, array.Length);
				stream.Position = 0L;
				while (stream.Position < stream.Length)
				{
					Asn1Node asn1Node = new Asn1Node(this, num2);
					asn1Node.parseEncapsulatedData = parseEncapsulatedData;
					position2 = stream.Position;
					if (!asn1Node.InternalLoadData(stream))
					{
						return flag;
					}
					AddChild(asn1Node);
					position3 = stream.Position;
					num2 += position3 - position2;
				}
				flag = true;
			}
			finally
			{
				if (!flag)
				{
					xdata.Position = position;
					ClearAll();
				}
			}
			return flag;
		}

		protected void SetData(byte[] xdata)
		{
			if (childNodeList.Count > 0)
			{
				throw new Exception("Constructed node can't hold simple data.");
			}
			data = xdata;
			if (data != null)
			{
				dataLength = data.Length;
			}
			else
			{
				dataLength = 0L;
			}
			RecalculateTreePar();
		}

		protected bool InternalLoadData(Stream xdata)
		{
			bool result = true;
			ClearAll();
			long position = xdata.Position;
			byte b = (byte)xdata.ReadByte();
			xdata.Position = position;
			int num = b & 0x1F;
			if ((b & 0x20u) != 0 || (parseEncapsulatedData && (num == 3 || num == 8 || num == 27 || num == 24 || num == 25 || num == 22 || num == 4 || num == 19 || num == 16 || num == 17 || num == 20 || num == 28 || num == 12 || num == 21 || num == 26)))
			{
				if (!ListDecode(xdata) && !GeneralDecode(xdata))
				{
					result = false;
				}
			}
			else if (!GeneralDecode(xdata))
			{
				result = false;
			}
			return result;
		}
	}
}
