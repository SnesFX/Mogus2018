using System;
using System.Net;

namespace Hazel.Udp
{
	internal sealed class UdpServerConnection : UdpConnection
	{
		private object stateLock = new object();

		public UdpConnectionListener Listener { get; private set; }

		internal UdpServerConnection(UdpConnectionListener listener, EndPoint endPoint, IPMode IPMode)
		{
			Listener = listener;
			base.RemoteEndPoint = endPoint;
			base.EndPoint = new NetworkEndPoint(endPoint);
			base.IPMode = IPMode;
			base.State = ConnectionState.Connected;
		}

		protected override void WriteBytesToConnection(byte[] bytes, int length)
		{
			InvokeDataSentRaw(bytes, length);
			lock (stateLock)
			{
				if (base.State != ConnectionState.Connected)
				{
					throw new InvalidOperationException("Could not send data as this Connection is not connected. Did you disconnect?");
				}
			}
			Listener.SendData(bytes, length, base.RemoteEndPoint);
		}

		protected override void WriteBytesToConnectionSync(byte[] bytes, int length)
		{
			InvokeDataSentRaw(bytes, length);
			lock (stateLock)
			{
				if (base.State != ConnectionState.Connected)
				{
					throw new InvalidOperationException("Could not send data as this Connection is not connected. Did you disconnect?");
				}
			}
			Listener.SendDataSync(bytes, length, base.RemoteEndPoint);
		}

		public override void Connect(byte[] bytes = null, int timeout = 5000)
		{
			throw new HazelException("Cannot manually connect a UdpServerConnection, did you mean to use UdpClientConnection?");
		}

		public override void ConnectAsync(byte[] bytes = null, int timeout = 5000)
		{
			throw new HazelException("Cannot manually connect a UdpServerConnection, did you mean to use UdpClientConnection?");
		}

		protected override void HandleDisconnect(HazelException e = null)
		{
			bool flag = false;
			lock (stateLock)
			{
				if (base.State == ConnectionState.Connected)
				{
					base.State = ConnectionState.Disconnecting;
					flag = true;
				}
			}
			if (flag)
			{
				try
				{
					InvokeDisconnected(e);
				}
				catch
				{
				}
				Dispose();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				bool flag;
				lock (stateLock)
				{
					flag = base.State == ConnectionState.Connected;
				}
				if (flag)
				{
					SendDisconnect();
				}
				Listener.RemoveConnectionTo(base.RemoteEndPoint);
				lock (stateLock)
				{
					base.State = ConnectionState.NotConnected;
				}
			}
			base.Dispose(disposing);
		}
	}
}
