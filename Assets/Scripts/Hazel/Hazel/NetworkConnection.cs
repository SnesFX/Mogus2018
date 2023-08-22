using System;
using System.Net;

namespace Hazel
{
	public abstract class NetworkConnection : Connection
	{
		public EndPoint RemoteEndPoint { get; protected set; }

		public IPMode IPMode { get; protected set; }

		public long GetIP4Address()
		{
			if (IPMode == IPMode.IPv4)
			{
				return ((IPEndPoint)RemoteEndPoint).Address.Address;
			}
			byte[] addressBytes = ((IPEndPoint)RemoteEndPoint).Address.GetAddressBytes();
			return BitConverter.ToInt64(addressBytes, addressBytes.Length - 8);
		}
	}
}
