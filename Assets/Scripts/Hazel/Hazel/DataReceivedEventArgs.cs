using System;

namespace Hazel
{
	public class DataReceivedEventArgs : EventArgs, IRecyclable
	{
		private static readonly ObjectPool<DataReceivedEventArgs> objectPool = new ObjectPool<DataReceivedEventArgs>(() => new DataReceivedEventArgs());

		public byte[] Bytes { get; private set; }

		public SendOption SendOption { get; private set; }

		public ushort ReliableId { get; private set; }

		internal static DataReceivedEventArgs GetObject()
		{
			return objectPool.GetObject();
		}

		private DataReceivedEventArgs()
		{
		}

		internal void Set(byte[] bytes, SendOption sendOption, ushort reliableId)
		{
			Bytes = bytes;
			SendOption = sendOption;
			ReliableId = reliableId;
		}

		public void Recycle()
		{
			objectPool.PutObject(this);
		}
	}
}
