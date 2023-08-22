using System;
using System.Collections;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Mono.Remoting.Channels.Unix
{
	public class UnixChannel : IChannelReceiver, IChannel, IChannelSender
	{
		private UnixClientChannel _clientChannel;

		private UnixServerChannel _serverChannel;

		private string _name = "unix";

		private int _priority = 1;

		public string ChannelName
		{
			get
			{
				return _name;
			}
		}

		public int ChannelPriority
		{
			get
			{
				return _priority;
			}
		}

		public object ChannelData
		{
			get
			{
				if (_serverChannel != null)
				{
					return _serverChannel.ChannelData;
				}
				return null;
			}
		}

		public UnixChannel()
			: this(null)
		{
		}

		public UnixChannel(string path)
		{
			Hashtable hashtable = new Hashtable();
			hashtable["path"] = path;
			Init(hashtable, null, null);
		}

		private void Init(IDictionary properties, IClientChannelSinkProvider clientSink, IServerChannelSinkProvider serverSink)
		{
			_clientChannel = new UnixClientChannel(properties, clientSink);
			if (properties["path"] != null)
			{
				_serverChannel = new UnixServerChannel(properties, serverSink);
			}
			object obj = properties["name"];
			if (obj != null)
			{
				_name = obj as string;
			}
			obj = properties["priority"];
			if (obj != null)
			{
				_priority = Convert.ToInt32(obj);
			}
		}

		public UnixChannel(IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider)
		{
			Init(properties, clientSinkProvider, serverSinkProvider);
		}

		public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
		{
			return _clientChannel.CreateMessageSink(url, remoteChannelData, out objectURI);
		}

		public void StartListening(object data)
		{
			if (_serverChannel != null)
			{
				_serverChannel.StartListening(data);
			}
		}

		public void StopListening(object data)
		{
			if (_serverChannel != null)
			{
				_serverChannel.StopListening(data);
			}
		}

		public string[] GetUrlsForUri(string uri)
		{
			if (_serverChannel != null)
			{
				return _serverChannel.GetUrlsForUri(uri);
			}
			return null;
		}

		public string Parse(string url, out string objectURI)
		{
			return ParseUnixURL(url, out objectURI);
		}

		internal static string ParseUnixURL(string url, out string objectURI)
		{
			objectURI = null;
			if (!url.StartsWith("unix://"))
			{
				return null;
			}
			int num = url.IndexOf('?');
			if (num == -1)
			{
				return url.Substring(7);
			}
			objectURI = url.Substring(num + 1);
			if (objectURI.Length == 0)
			{
				objectURI = null;
			}
			return url.Substring(7, num - 7);
		}
	}
}
