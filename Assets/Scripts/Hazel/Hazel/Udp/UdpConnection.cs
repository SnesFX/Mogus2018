#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Hazel.Udp
{
	public abstract class UdpConnection : NetworkConnection
	{
		private class FragmentedMessage
		{
			public struct Fragment
			{
				public int fragmentID;

				public byte[] data;

				public int offset;

				public Fragment(int fragmentID, byte[] data, int offset)
				{
					this.fragmentID = fragmentID;
					this.data = data;
					this.offset = offset;
				}
			}

			public int noFragments = -1;

			public List<Fragment> received = new List<Fragment>();
		}

		private class Packet : IRecyclable, IDisposable
		{
			private static readonly ObjectPool<Packet> objectPool = new ObjectPool<Packet>(() => new Packet());

			public byte[] Data;

			public Timer Timer;

			public volatile int LastTimeout;

			public Action AckCallback;

			public volatile bool Acknowledged;

			public volatile int Retransmissions;

			public Stopwatch Stopwatch = new Stopwatch();

			internal static Packet GetObject()
			{
				return objectPool.GetObject();
			}

			private Packet()
			{
			}

			internal void Set(byte[] data, Action<Packet> resendAction, int timeout, Action ackCallback)
			{
				Data = data;
				Timer = new Timer(delegate
				{
					resendAction(this);
				}, null, timeout, -1);
				LastTimeout = timeout;
				AckCallback = ackCallback;
				Acknowledged = false;
				Retransmissions = 0;
				Stopwatch.Reset();
				Stopwatch.Start();
			}

			public void Recycle()
			{
				lock (Timer)
				{
					Timer.Dispose();
				}
				objectPool.PutObject(this);
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected void Dispose(bool disposing)
			{
				if (disposing)
				{
					lock (Timer)
					{
						Timer.Dispose();
					}
				}
			}
		}

		public const int FragmentSize = 65500;

		private volatile ushort lastFragmentIDAllocated;

		private Dictionary<ushort, FragmentedMessage> fragmentedMessagesReceived = new Dictionary<ushort, FragmentedMessage>();

		private int keepAliveInterval = 10000;

		private Timer keepAliveTimer;

		private object keepAliveTimerLock = new object();

		private bool keepAliveTimerDisposed;

		private volatile int resendTimeout;

		private volatile ushort lastIDAllocated;

		private Dictionary<ushort, Packet> reliableDataPacketsSent = new Dictionary<ushort, Packet>();

		private HashSet<ushort> reliableDataPacketsMissing = new HashSet<ushort>();

		private volatile ushort reliableReceiveLast;

		private volatile bool hasReceivedSomething;

		private object PingLock = new object();

		public volatile float AveragePingMs = 500f;

		private volatile int disconnectTimeout = 2500;

		public int KeepAliveInterval
		{
			get
			{
				return keepAliveInterval;
			}
			set
			{
				keepAliveInterval = value;
				ResetKeepAliveTimer();
			}
		}

		public int ResendTimeout
		{
			get
			{
				return resendTimeout;
			}
			set
			{
				resendTimeout = value;
			}
		}

		public int DisconnectTimeout
		{
			get
			{
				return disconnectTimeout;
			}
			set
			{
				disconnectTimeout = value;
			}
		}

		protected UdpConnection()
		{
			InitializeKeepAliveTimer();
		}

		protected abstract void WriteBytesToConnection(byte[] bytes, int length);

		protected abstract void WriteBytesToConnectionSync(byte[] bytes, int length);

		public override void Send(MessageWriter msg)
		{
			if (base.State != ConnectionState.Connected)
			{
				throw new InvalidOperationException("Could not send data as this Connection is not connected. Did you disconnect?");
			}
			byte[] array = new byte[msg.Length];
			Buffer.BlockCopy(msg.Buffer, 0, array, 0, msg.Length);
			switch (msg.SendOption)
			{
			case SendOption.Reliable:
				ResetKeepAliveTimer();
				AttachReliableID(array, 1, array.Length);
				WriteBytesToConnection(array, array.Length);
				base.Statistics.LogReliableSend(array.Length - 3, array.Length);
				break;
			case SendOption.FragmentedReliable:
				throw new NotImplementedException("Not yet");
			default:
				WriteBytesToConnection(array, array.Length);
				base.Statistics.LogUnreliableSend(array.Length - 1, array.Length);
				break;
			}
		}

		public override void SendBytes(byte[] bytes, SendOption sendOption = SendOption.None)
		{
			if (base.State != ConnectionState.Connected)
			{
				throw new InvalidOperationException("Could not send data as this Connection is not connected. Did you disconnect?");
			}
			HandleSend(bytes, (byte)sendOption);
		}

		public override void SendBytes(byte[] bytes, int offset, int length, SendOption sendOption = SendOption.None)
		{
			if (base.State != ConnectionState.Connected)
			{
				throw new InvalidOperationException("Could not send data as this Connection is not connected. Did you disconnect?");
			}
			switch (sendOption)
			{
			case SendOption.Reliable:
				ReliableSend((byte)sendOption, bytes, offset, length);
				break;
			case SendOption.FragmentedReliable:
				throw new NotImplementedException();
			default:
				UnreliableSend((byte)sendOption, bytes, offset, length);
				break;
			}
		}

		protected void HandleSend(byte[] data, byte sendOption, Action ackCallback = null)
		{
			switch (sendOption)
			{
			case 1:
			case 8:
				ReliableSend(sendOption, data, ackCallback);
				break;
			case 2:
				FragmentedSend(data);
				break;
			default:
				UnreliableSend(sendOption, data);
				break;
			}
		}

		protected internal void HandleReceive(byte[] buffer)
		{
			InvokeDataReceivedRaw(buffer);
			switch (buffer[0])
			{
			case 1:
				ReliableMessageReceive(buffer);
				break;
			case 10:
				AcknowledgementMessageReceive(buffer);
				break;
			case 8:
			{
				ushort id;
				ProcessReliableReceive(buffer, 1, out id);
				base.Statistics.LogHelloReceive(buffer.Length);
				break;
			}
			case 9:
				HandleDisconnect(new HazelException("The remote sent a disconnect request"));
				break;
			case 2:
				FragmentedStartMessageReceive(buffer);
				break;
			case 11:
				FragmentedMessageReceive(buffer);
				break;
			default:
				InvokeDataReceived(SendOption.None, buffer, 1, 0);
				base.Statistics.LogUnreliableReceive(buffer.Length - 1, buffer.Length);
				break;
			}
		}

		private void UnreliableSend(byte sendOption, byte[] data)
		{
			UnreliableSend(sendOption, data, 0, data.Length);
		}

		private void UnreliableSend(byte sendOption, byte[] data, int offset, int length)
		{
			byte[] array = new byte[length + 1];
			array[0] = sendOption;
			Buffer.BlockCopy(data, offset, array, array.Length - length, length);
			WriteBytesToConnection(array, array.Length);
			base.Statistics.LogUnreliableSend(length, array.Length);
		}

		private void InvokeDataReceived(SendOption sendOption, byte[] buffer, int dataOffset, ushort reliableId)
		{
			byte[] array = new byte[buffer.Length - dataOffset];
			Buffer.BlockCopy(buffer, dataOffset, array, 0, array.Length);
			InvokeDataReceived(array, sendOption, reliableId);
		}

		protected void SendHello(byte[] bytes, Action acknowledgeCallback)
		{
			byte[] array;
			if (bytes == null)
			{
				array = new byte[1];
			}
			else
			{
				array = new byte[bytes.Length + 1];
				Buffer.BlockCopy(bytes, 0, array, 1, bytes.Length);
			}
			HandleSend(array, 8, acknowledgeCallback);
		}

		protected abstract void HandleDisconnect(HazelException e = null);

		public override void SendDisconnect()
		{
			WriteBytesToConnectionSync(new byte[1] { 9 }, 1);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposeKeepAliveTimer();
			}
			base.Dispose(disposing);
		}

		private void FragmentedSend(byte[] data)
		{
			ushort num = ++lastFragmentIDAllocated;
			ushort num2 = 0;
			while ((double)(int)num2 < Math.Ceiling((double)data.Length / 65500.0))
			{
				byte[] array = new byte[Math.Min(data.Length - 65500 * num2, 65500) + 7];
				array[0] = (byte)((num2 == 0) ? 2 : 11);
				array[1] = (byte)((uint)(num >> 8) & 0xFFu);
				array[2] = (byte)num;
				if (num2 == 0)
				{
					ushort num3 = (ushort)Math.Ceiling((double)data.Length / 65500.0);
					array[3] = (byte)((uint)(num3 >> 8) & 0xFFu);
					array[4] = (byte)num3;
				}
				else
				{
					array[3] = (byte)((uint)(num2 >> 8) & 0xFFu);
					array[4] = (byte)num2;
				}
				AttachReliableID(array, 5, array.Length);
				Buffer.BlockCopy(data, 65500 * num2, array, 7, array.Length - 7);
				WriteBytesToConnection(array, array.Length);
				num2 = (ushort)(num2 + 1);
			}
		}

		private FragmentedMessage GetFragmentedMessage(ushort messageId)
		{
			lock (fragmentedMessagesReceived)
			{
				FragmentedMessage fragmentedMessage;
				if (fragmentedMessagesReceived.ContainsKey(messageId))
				{
					fragmentedMessage = fragmentedMessagesReceived[messageId];
				}
				else
				{
					fragmentedMessage = new FragmentedMessage();
					fragmentedMessagesReceived.Add(messageId, fragmentedMessage);
				}
				return fragmentedMessage;
			}
		}

		private void FragmentedStartMessageReceive(byte[] buffer)
		{
			ushort id;
			if (ProcessReliableReceive(buffer, 5, out id))
			{
				ushort messageId = (ushort)((buffer[1] << 8) + buffer[2]);
				ushort noFragments = (ushort)((buffer[3] << 8) + buffer[4]);
				FragmentedMessage fragmentedMessage;
				bool flag;
				lock (fragmentedMessagesReceived)
				{
					fragmentedMessage = GetFragmentedMessage(messageId);
					fragmentedMessage.received.Add(new FragmentedMessage.Fragment(0, buffer, 7));
					fragmentedMessage.noFragments = noFragments;
					flag = fragmentedMessage.noFragments == fragmentedMessage.received.Count;
				}
				if (flag)
				{
					FinalizeFragmentedMessage(fragmentedMessage);
				}
			}
		}

		private void FragmentedMessageReceive(byte[] buffer)
		{
			ushort id;
			if (ProcessReliableReceive(buffer, 5, out id))
			{
				ushort messageId = (ushort)((buffer[1] << 8) + buffer[2]);
				ushort fragmentID = (ushort)((buffer[3] << 8) + buffer[4]);
				FragmentedMessage fragmentedMessage;
				bool flag;
				lock (fragmentedMessagesReceived)
				{
					fragmentedMessage = GetFragmentedMessage(messageId);
					fragmentedMessage.received.Add(new FragmentedMessage.Fragment(fragmentID, buffer, 7));
					flag = fragmentedMessage.noFragments == fragmentedMessage.received.Count;
				}
				if (flag)
				{
					FinalizeFragmentedMessage(fragmentedMessage);
				}
			}
		}

		private void FinalizeFragmentedMessage(FragmentedMessage message)
		{
			IOrderedEnumerable<FragmentedMessage.Fragment> orderedEnumerable = message.received.OrderBy((FragmentedMessage.Fragment x) => x.fragmentID);
			FragmentedMessage.Fragment fragment = orderedEnumerable.Last();
			byte[] array = new byte[(orderedEnumerable.Count() - 1) * 65500 + fragment.data.Length - fragment.offset];
			int num = 0;
			foreach (FragmentedMessage.Fragment item in orderedEnumerable)
			{
				Buffer.BlockCopy(item.data, item.offset, array, num, item.data.Length - item.offset);
				num += item.data.Length - item.offset;
			}
			InvokeDataReceived(array, SendOption.FragmentedReliable, 0);
		}

		private void InitializeKeepAliveTimer()
		{
			lock (keepAliveTimerLock)
			{
				keepAliveTimer = new Timer(delegate
				{
					try
					{
						SendHello(null, null);
						Trace.WriteLine("Keepalive packet sent.");
					}
					catch
					{
						Trace.WriteLine("Keepalive packet failed to send.");
						DisposeKeepAliveTimer();
					}
				}, null, keepAliveInterval, keepAliveInterval);
			}
		}

		private void ResetKeepAliveTimer()
		{
			lock (keepAliveTimerLock)
			{
				keepAliveTimer.Change(keepAliveInterval, keepAliveInterval);
			}
		}

		private void DisposeKeepAliveTimer()
		{
			lock (keepAliveTimerLock)
			{
				if (!keepAliveTimerDisposed)
				{
					keepAliveTimer.Dispose();
				}
				keepAliveTimerDisposed = true;
			}
		}

		private void AttachReliableID(byte[] buffer, int offset, int sendLength, Action ackCallback = null)
		{
			lock (reliableDataPacketsSent)
			{
				ushort id;
				do
				{
					id = ++lastIDAllocated;
				}
				while (reliableDataPacketsSent.ContainsKey(id));
				buffer[offset] = (byte)((uint)(id >> 8) & 0xFFu);
				buffer[offset + 1] = (byte)id;
				Packet @object = Packet.GetObject();
				@object.Set(buffer, delegate(Packet p)
				{
					lock (p.Timer)
					{
						if (!p.Acknowledged)
						{
							p.LastTimeout = (int)Math.Min((float)p.LastTimeout * 1.5f, (float)disconnectTimeout / 2f);
							p.Timer.Change(p.LastTimeout, -1);
							if (p.Stopwatch.ElapsedMilliseconds > disconnectTimeout)
							{
								HandleDisconnect(new HazelException(string.Format("Reliable packet {0} was not ack'd after {1} resends", id, p.Retransmissions)));
								p.Acknowledged = true;
								p.Recycle();
								return;
							}
						}
					}
					try
					{
						WriteBytesToConnection(p.Data, sendLength);
						p.Retransmissions++;
					}
					catch (InvalidOperationException e)
					{
						HandleDisconnect(new HazelException("Could not resend data as connection is no longer connected", e));
					}
					Trace.WriteLine("Resend.");
				}, (resendTimeout > 0) ? resendTimeout : ((int)Math.Max(40f, Math.Min(AveragePingMs * 4f, 750f))), ackCallback);
				reliableDataPacketsSent.Add(id, @object);
			}
		}

		private void ReliableSend(byte sendOption, byte[] data, Action ackCallback = null)
		{
			ReliableSend(sendOption, data, 0, data.Length, ackCallback);
		}

		private void ReliableSend(byte sendOption, byte[] data, int offset, int length, Action ackCallback = null)
		{
			ResetKeepAliveTimer();
			byte[] array = new byte[length + 3];
			array[0] = sendOption;
			AttachReliableID(array, 1, array.Length, ackCallback);
			Buffer.BlockCopy(data, offset, array, array.Length - length, length);
			WriteBytesToConnection(array, array.Length);
			base.Statistics.LogReliableSend(length, array.Length);
		}

		private void ReliableMessageReceive(byte[] buffer)
		{
			ushort id;
			if (ProcessReliableReceive(buffer, 1, out id))
			{
				InvokeDataReceived(SendOption.Reliable, buffer, 3, id);
			}
			base.Statistics.LogReliableReceive(buffer.Length - 3, buffer.Length);
		}

		private bool ProcessReliableReceive(byte[] bytes, int offset, out ushort id)
		{
			id = (ushort)((bytes[offset] << 8) + bytes[offset + 1]);
			SendAck(bytes[offset], bytes[offset + 1]);
			lock (reliableDataPacketsMissing)
			{
				ushort num = (ushort)(reliableReceiveLast - 32768);
				bool flag = ((num >= reliableReceiveLast) ? (id > reliableReceiveLast && id <= num) : (id > reliableReceiveLast || id <= num));
				if (flag || !hasReceivedSomething)
				{
					for (ushort num2 = (ushort)(reliableReceiveLast + 1); num2 < id; num2 = (ushort)(num2 + 1))
					{
						reliableDataPacketsMissing.Add(num2);
					}
					reliableReceiveLast = id;
					hasReceivedSomething = true;
				}
				else if (!reliableDataPacketsMissing.Remove(id))
				{
					return false;
				}
			}
			return true;
		}

		private void AcknowledgementMessageReceive(byte[] bytes)
		{
			ushort key = (ushort)((bytes[1] << 8) + bytes[2]);
			lock (reliableDataPacketsSent)
			{
				Packet value;
				if (reliableDataPacketsSent.TryGetValue(key, out value))
				{
					value.Acknowledged = true;
					if (value.AckCallback != null)
					{
						value.AckCallback();
					}
					value.Stopwatch.Stop();
					lock (PingLock)
					{
						AveragePingMs = Math.Max(10f, AveragePingMs * 0.7f + (float)value.Stopwatch.Elapsed.TotalMilliseconds * 0.3f);
					}
					value.Recycle();
					reliableDataPacketsSent.Remove(key);
				}
			}
			base.Statistics.LogReliableReceive(0, bytes.Length);
		}

		internal void SendAck(byte byte1, byte byte2)
		{
			byte[] array = new byte[3] { 10, byte1, byte2 };
			try
			{
				WriteBytesToConnection(array, array.Length);
			}
			catch (InvalidOperationException)
			{
			}
		}
	}
}
