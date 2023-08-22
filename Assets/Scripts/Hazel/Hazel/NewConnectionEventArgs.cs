using System;

namespace Hazel
{
	public class NewConnectionEventArgs : EventArgs, IRecyclable
	{
		private static readonly ObjectPool<NewConnectionEventArgs> objectPool = new ObjectPool<NewConnectionEventArgs>(() => new NewConnectionEventArgs());

		public byte[] HandshakeData { get; private set; }

		public Connection Connection { get; private set; }

		internal static NewConnectionEventArgs GetObject()
		{
			return objectPool.GetObject();
		}

		private NewConnectionEventArgs()
		{
		}

		internal void Set(byte[] bytes, Connection connection)
		{
			HandshakeData = bytes;
			Connection = connection;
		}

		public void Recycle()
		{
			objectPool.PutObject(this);
		}
	}
}
