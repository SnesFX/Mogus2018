using System.Collections;
using System.Runtime.Remoting.Channels;

namespace Mono.Remoting.Channels.Unix
{
	internal class UnixBinaryClientFormatterSinkProvider : IClientFormatterSinkProvider, IClientChannelSinkProvider
	{
		private IClientChannelSinkProvider next;

		private UnixBinaryCore _binaryCore;

		private static string[] allowedProperties = new string[2] { "includeVersions", "strictBinding" };

		public IClientChannelSinkProvider Next
		{
			get
			{
				return next;
			}
			set
			{
				next = value;
			}
		}

		public UnixBinaryClientFormatterSinkProvider()
		{
			_binaryCore = UnixBinaryCore.DefaultInstance;
		}

		public UnixBinaryClientFormatterSinkProvider(IDictionary properties, ICollection providerData)
		{
			_binaryCore = new UnixBinaryCore(this, properties, allowedProperties);
		}

		public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
		{
			IClientChannelSink nextSink = null;
			if (next != null)
			{
				nextSink = next.CreateSink(channel, url, remoteChannelData);
			}
			return new UnixBinaryClientFormatterSink(nextSink)
			{
				BinaryCore = _binaryCore
			};
		}
	}
}
