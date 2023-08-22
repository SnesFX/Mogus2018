using System;
using System.Collections;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Mono.Remoting.Channels.Unix
{
	public class UnixClientChannel : IChannelSender, IChannel
	{
		private int priority = 1;

		private string name = "unix";

		private IClientChannelSinkProvider _sinkProvider;

		public string ChannelName
		{
			get
			{
				return name;
			}
		}

		public int ChannelPriority
		{
			get
			{
				return priority;
			}
		}

		public UnixClientChannel()
		{
			_sinkProvider = new UnixBinaryClientFormatterSinkProvider();
			_sinkProvider.Next = new UnixClientTransportSinkProvider();
		}

		public UnixClientChannel(IDictionary properties, IClientChannelSinkProvider sinkProvider)
		{
			object obj = properties["name"];
			if (obj != null)
			{
				name = obj as string;
			}
			obj = properties["priority"];
			if (obj != null)
			{
				priority = Convert.ToInt32(obj);
			}
			if (sinkProvider != null)
			{
				_sinkProvider = sinkProvider;
				IClientChannelSinkProvider clientChannelSinkProvider = sinkProvider;
				while (clientChannelSinkProvider.Next != null)
				{
					clientChannelSinkProvider = clientChannelSinkProvider.Next;
				}
				clientChannelSinkProvider.Next = new UnixClientTransportSinkProvider();
			}
			else
			{
				_sinkProvider = new UnixBinaryClientFormatterSinkProvider();
				_sinkProvider.Next = new UnixClientTransportSinkProvider();
			}
		}

		public UnixClientChannel(string name, IClientChannelSinkProvider sinkProvider)
		{
			this.name = name;
			_sinkProvider = sinkProvider;
			IClientChannelSinkProvider clientChannelSinkProvider = sinkProvider;
			while (clientChannelSinkProvider.Next != null)
			{
				clientChannelSinkProvider = clientChannelSinkProvider.Next;
			}
			clientChannelSinkProvider.Next = new UnixClientTransportSinkProvider();
		}

		public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
		{
			if (url != null && Parse(url, out objectURI) != null)
			{
				return (IMessageSink)_sinkProvider.CreateSink(this, url, remoteChannelData);
			}
			if (remoteChannelData != null)
			{
				IChannelDataStore channelDataStore = remoteChannelData as IChannelDataStore;
				if (channelDataStore == null || channelDataStore.ChannelUris.Length == 0)
				{
					objectURI = null;
					return null;
				}
				url = channelDataStore.ChannelUris[0];
			}
			if (Parse(url, out objectURI) == null)
			{
				return null;
			}
			return (IMessageSink)_sinkProvider.CreateSink(this, url, remoteChannelData);
		}

		public string Parse(string url, out string objectURI)
		{
			return UnixChannel.ParseUnixURL(url, out objectURI);
		}
	}
}
