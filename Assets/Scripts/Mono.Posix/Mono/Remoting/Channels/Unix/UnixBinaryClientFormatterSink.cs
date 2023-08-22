using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Mono.Remoting.Channels.Unix
{
	internal class UnixBinaryClientFormatterSink : IClientFormatterSink, IMessageSink, IClientChannelSink, IChannelSinkBase
	{
		private UnixBinaryCore _binaryCore = UnixBinaryCore.DefaultInstance;

		private IClientChannelSink _nextInChain;

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

		public IClientChannelSink NextChannelSink
		{
			get
			{
				return _nextInChain;
			}
		}

		public IMessageSink NextSink
		{
			get
			{
				return null;
			}
		}

		public IDictionary Properties
		{
			get
			{
				return null;
			}
		}

		public UnixBinaryClientFormatterSink(IClientChannelSink nextSink)
		{
			_nextInChain = nextSink;
		}

		public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
		{
			throw new NotSupportedException("UnixBinaryClientFormatterSink must be the first sink in the IClientChannelSink chain");
		}

		public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
		{
			IMessage msg = (IMessage)_binaryCore.Deserializer.DeserializeMethodResponse(stream, null, (IMethodCallMessage)state);
			sinkStack.DispatchReplyMessage(msg);
		}

		public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
		{
			throw new NotSupportedException();
		}

		public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			throw new NotSupportedException();
		}

		public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
		{
			ITransportHeaders headers = new TransportHeaders();
			Stream stream = _nextInChain.GetRequestStream(msg, headers);
			if (stream == null)
			{
				stream = new MemoryStream();
			}
			_binaryCore.Serializer.Serialize(stream, msg, null);
			if (stream is MemoryStream)
			{
				stream.Position = 0L;
			}
			ClientChannelSinkStack clientChannelSinkStack = new ClientChannelSinkStack(replySink);
			clientChannelSinkStack.Push(this, msg);
			_nextInChain.AsyncProcessRequest(clientChannelSinkStack, msg, headers, stream);
			return null;
		}

		public IMessage SyncProcessMessage(IMessage msg)
		{
			try
			{
				ITransportHeaders transportHeaders = new TransportHeaders();
				transportHeaders["__RequestUri"] = ((IMethodCallMessage)msg).Uri;
				transportHeaders["Content-Type"] = "application/octet-stream";
				Stream stream = _nextInChain.GetRequestStream(msg, transportHeaders);
				if (stream == null)
				{
					stream = new MemoryStream();
				}
				_binaryCore.Serializer.Serialize(stream, msg, null);
				if (stream is MemoryStream)
				{
					stream.Position = 0L;
				}
				ITransportHeaders responseHeaders;
				Stream responseStream;
				_nextInChain.ProcessMessage(msg, transportHeaders, stream, out responseHeaders, out responseStream);
				return (IMessage)_binaryCore.Deserializer.DeserializeMethodResponse(responseStream, null, (IMethodCallMessage)msg);
			}
			catch (Exception e)
			{
				return new ReturnMessage(e, (IMethodCallMessage)msg);
			}
		}
	}
}
