using System.Net;

namespace Hazel
{
	public abstract class NetworkConnectionListener : ConnectionListener
	{
		public EndPoint EndPoint { get; protected set; }

		public IPMode IPMode { get; protected set; }
	}
}
