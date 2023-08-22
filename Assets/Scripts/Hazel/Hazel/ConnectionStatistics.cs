using System.Threading;

namespace Hazel
{
	public class ConnectionStatistics
	{
		private long unreliableMessagesSent;

		private long reliableMessagesSent;

		private long fragmentedMessagesSent;

		private long acknowledgementMessagesSent;

		private long helloMessagesSent;

		private long dataBytesSent;

		private long totalBytesSent;

		private long unreliableMessagesReceived;

		private long reliableMessagesReceived;

		private long fragmentedMessagesReceived;

		private long acknowledgementMessagesReceived;

		private long helloMessagesReceived;

		private long dataBytesReceived;

		private long totalBytesReceived;

		public long MessagesSent
		{
			get
			{
				return UnreliableMessagesSent + ReliableMessagesSent + FragmentedMessagesSent + AcknowledgementMessagesSent + HelloMessagesSent;
			}
		}

		public long UnreliableMessagesSent
		{
			get
			{
				return Interlocked.Read(ref unreliableMessagesSent);
			}
		}

		public long ReliableMessagesSent
		{
			get
			{
				return Interlocked.Read(ref reliableMessagesSent);
			}
		}

		public long FragmentedMessagesSent
		{
			get
			{
				return Interlocked.Read(ref fragmentedMessagesSent);
			}
		}

		public long AcknowledgementMessagesSent
		{
			get
			{
				return Interlocked.Read(ref acknowledgementMessagesSent);
			}
		}

		public long HelloMessagesSent
		{
			get
			{
				return Interlocked.Read(ref helloMessagesSent);
			}
		}

		public long DataBytesSent
		{
			get
			{
				return Interlocked.Read(ref dataBytesSent);
			}
		}

		public long TotalBytesSent
		{
			get
			{
				return Interlocked.Read(ref totalBytesSent);
			}
		}

		public long MessagesReceived
		{
			get
			{
				return UnreliableMessagesReceived + ReliableMessagesReceived + FragmentedMessagesReceived + AcknowledgementMessagesReceived + helloMessagesReceived;
			}
		}

		public long UnreliableMessagesReceived
		{
			get
			{
				return Interlocked.Read(ref unreliableMessagesReceived);
			}
		}

		public long ReliableMessagesReceived
		{
			get
			{
				return Interlocked.Read(ref reliableMessagesReceived);
			}
		}

		public long FragmentedMessagesReceived
		{
			get
			{
				return Interlocked.Read(ref fragmentedMessagesReceived);
			}
		}

		public long AcknowledgementMessagesReceived
		{
			get
			{
				return Interlocked.Read(ref acknowledgementMessagesReceived);
			}
		}

		public long HelloMessagesReceived
		{
			get
			{
				return Interlocked.Read(ref helloMessagesReceived);
			}
		}

		public long DataBytesReceived
		{
			get
			{
				return Interlocked.Read(ref dataBytesReceived);
			}
		}

		public long TotalBytesReceived
		{
			get
			{
				return Interlocked.Read(ref totalBytesReceived);
			}
		}

		internal void LogUnreliableSend(int dataLength, int totalLength)
		{
			Interlocked.Increment(ref unreliableMessagesSent);
			Interlocked.Add(ref dataBytesSent, dataLength);
			Interlocked.Add(ref totalBytesSent, totalLength);
		}

		internal void LogReliableSend(int dataLength, int totalLength)
		{
			Interlocked.Increment(ref reliableMessagesSent);
			Interlocked.Add(ref dataBytesSent, dataLength);
			Interlocked.Add(ref totalBytesSent, totalLength);
		}

		internal void LogFragmentedSend(int dataLength, int totalLength)
		{
			Interlocked.Increment(ref fragmentedMessagesSent);
			Interlocked.Add(ref dataBytesSent, dataLength);
			Interlocked.Add(ref totalBytesSent, totalLength);
		}

		internal void LogAcknowledgementSend(int totalLength)
		{
			Interlocked.Increment(ref acknowledgementMessagesSent);
			Interlocked.Add(ref totalBytesSent, totalLength);
		}

		internal void LogHelloSend(int totalLength)
		{
			Interlocked.Increment(ref helloMessagesSent);
			Interlocked.Add(ref totalBytesSent, totalLength);
		}

		internal void LogUnreliableReceive(int dataLength, int totalLength)
		{
			Interlocked.Increment(ref unreliableMessagesReceived);
			Interlocked.Add(ref dataBytesReceived, dataLength);
			Interlocked.Add(ref totalBytesReceived, totalLength);
		}

		internal void LogReliableReceive(int dataLength, int totalLength)
		{
			Interlocked.Increment(ref reliableMessagesReceived);
			Interlocked.Add(ref dataBytesReceived, dataLength);
			Interlocked.Add(ref totalBytesReceived, totalLength);
		}

		internal void LogFragmentedReceive(int dataLength, int totalLength)
		{
			Interlocked.Increment(ref fragmentedMessagesReceived);
			Interlocked.Add(ref dataBytesReceived, dataLength);
			Interlocked.Add(ref totalBytesReceived, totalLength);
		}

		internal void LogAcknowledgementReceive(int totalLength)
		{
			Interlocked.Increment(ref acknowledgementMessagesReceived);
			Interlocked.Add(ref totalBytesReceived, totalLength);
		}

		internal void LogHelloReceive(int totalLength)
		{
			Interlocked.Increment(ref helloMessagesReceived);
			Interlocked.Add(ref totalBytesReceived, totalLength);
		}
	}
}
