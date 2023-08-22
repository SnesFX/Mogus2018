using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Mono.Remoting.Channels.Unix
{
	internal class UnixBinaryServerFormatterSink : IServerChannelSink, IChannelSinkBase
	{
		private UnixBinaryCore _binaryCore = UnixBinaryCore.DefaultInstance;

		private IServerChannelSink next_sink;

		private IChannelReceiver receiver;

		internal UnixBinaryCore BinaryCore
		{
			get
			{
				return _binaryCore;
			}
			set
			{
				_binaryCore = value;
			}
		}

		public IServerChannelSink NextChannelSink
		{
			get
			{
				return next_sink;
			}
		}

		public IDictionary Properties
		{
			get
			{
				return null;
			}
		}

		public UnixBinaryServerFormatterSink(IServerChannelSink nextSink, IChannelReceiver receiver)
		{
			next_sink = nextSink;
			this.receiver = receiver;
		}

		public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage message, ITransportHeaders headers, Stream stream)
		{
			ITransportHeaders headers2 = new TransportHeaders();
			if (sinkStack != null)
			{
				stream = sinkStack.GetResponseStream(message, headers2);
			}
			if (stream == null)
			{
				stream = new MemoryStream();
			}
			_binaryCore.Serializer.Serialize(stream, message, null);
			if (stream is MemoryStream)
			{
				stream.Position = 0L;
			}
			sinkStack.AsyncProcessResponse(message, headers2, stream);
		}

		public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
		{
			return null;
		}

		public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			sinkStack.Push(this, null);
			ServerProcessing serverProcessing;
			try
			{
				string text = (string)requestHeaders["__RequestUri"];
				string objectURI;
				receiver.Parse(text, out objectURI);
				if (objectURI == null)
				{
					objectURI = text;
				}
				MethodCallHeaderHandler @object = new MethodCallHeaderHandler(objectURI);
				requestMsg = (IMessage)_binaryCore.Deserializer.Deserialize(requestStream, @object.HandleHeaders);
				serverProcessing = next_sink.ProcessMessage(sinkStack, requestMsg, requestHeaders, null, out responseMsg, out responseHeaders, out responseStream);
			}
			catch (Exception e)
			{
				responseMsg = new ReturnMessage(e, (IMethodCallMessage)requestMsg);
				serverProcessing = ServerProcessing.Complete;
				responseHeaders = null;
				responseStream = null;
			}
			if (serverProcessing == ServerProcessing.Complete)
			{
				for (int i = 0; i < 3; i++)
				{
					responseStream = null;
					responseHeaders = new TransportHeaders();
					if (sinkStack != null)
					{
						responseStream = sinkStack.GetResponseStream(responseMsg, responseHeaders);
					}
					if (responseStream == null)
					{
						responseStream = new MemoryStream();
					}
					try
					{
						_binaryCore.Serializer.Serialize(responseStream, responseMsg);
					}
					catch (Exception ex)
					{
						if (i == 2)
						{
							throw ex;
						}
						responseMsg = new ReturnMessage(ex, (IMethodCallMessage)requestMsg);
						continue;
					}
					break;
				}
				if (responseStream is MemoryStream)
				{
					responseStream.Position = 0L;
				}
				sinkStack.Pop(this);
			}
			return serverProcessing;
		}
	}
}
