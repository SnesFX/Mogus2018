using System;

namespace Hazel
{
	public class DisconnectedEventArgs : EventArgs, IRecyclable
	{
		private static readonly ObjectPool<DisconnectedEventArgs> objectPool = new ObjectPool<DisconnectedEventArgs>(() => new DisconnectedEventArgs());

		public Exception Exception { get; private set; }

		internal static DisconnectedEventArgs GetObject()
		{
			return objectPool.GetObject();
		}

		private DisconnectedEventArgs()
		{
		}

		internal void Set(Exception e)
		{
			Exception = e;
		}

		public void Recycle()
		{
			objectPool.PutObject(this);
		}
	}
}
