using System;
using System.Threading;

namespace Hazel
{
	public abstract class Connection : IDisposable
	{
		public int TestLagMs = -1;

		private volatile ConnectionState state;

		private ManualResetEvent connectWaitLock = new ManualResetEvent(false);

		public ConnectionEndPoint EndPoint { get; protected set; }

		public ConnectionStatistics Statistics { get; protected set; }

		public ConnectionState State
		{
			get
			{
				return state;
			}
			protected set
			{
				state = value;
				if (state == ConnectionState.Connected)
				{
					connectWaitLock.Set();
				}
				else
				{
					connectWaitLock.Reset();
				}
			}
		}

		public event EventHandler<DataReceivedEventArgs> DataReceived;

		public event Action<byte[], int> DataSentRaw;

		public event Action<byte[]> DataReceivedRaw;

		public event EventHandler<DisconnectedEventArgs> Disconnected;

		protected void InvokeDataSentRaw(byte[] data, int length)
		{
			Action<byte[], int> dataSentRaw = this.DataSentRaw;
			if (dataSentRaw != null)
			{
				dataSentRaw(data, length);
			}
		}

		protected void InvokeDataReceivedRaw(byte[] data)
		{
			Action<byte[]> dataReceivedRaw = this.DataReceivedRaw;
			if (dataReceivedRaw != null)
			{
				dataReceivedRaw(data);
			}
		}

		protected Connection()
		{
			Statistics = new ConnectionStatistics();
			State = ConnectionState.NotConnected;
		}

		public abstract void Send(MessageWriter msg);

		public abstract void SendBytes(byte[] bytes, SendOption sendOption = SendOption.None);

		public abstract void SendBytes(byte[] bytes, int offset, int length, SendOption sendOption = SendOption.None);

		public abstract void Connect(byte[] bytes = null, int timeout = 5000);

		public abstract void ConnectAsync(byte[] bytes = null, int timeout = 5000);

		public abstract void SendDisconnect();

		protected void InvokeDataReceived(byte[] bytes, SendOption sendOption, ushort reliableId)
		{
			DataReceivedEventArgs @object = DataReceivedEventArgs.GetObject();
			@object.Set(bytes, sendOption, reliableId);
			EventHandler<DataReceivedEventArgs> dataReceived = this.DataReceived;
			if (dataReceived != null)
			{
				dataReceived(this, @object);
			}
		}

		protected void InvokeDisconnected(Exception e = null)
		{
			DisconnectedEventArgs @object = DisconnectedEventArgs.GetObject();
			@object.Set(e);
			EventHandler<DisconnectedEventArgs> disconnected = this.Disconnected;
			if (disconnected != null)
			{
				disconnected(this, @object);
			}
		}

		protected bool WaitOnConnect(int timeout)
		{
			return connectWaitLock.WaitOne(timeout);
		}

		public virtual void Close()
		{
			Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}
	}
}
