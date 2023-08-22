using System;

namespace Hazel
{
	public abstract class ConnectionListener : IDisposable
	{
		public event EventHandler<NewConnectionEventArgs> NewConnection;

		public abstract void Start();

		protected void InvokeNewConnection(byte[] bytes, Connection connection)
		{
			NewConnectionEventArgs @object = NewConnectionEventArgs.GetObject();
			@object.Set(bytes, connection);
			EventHandler<NewConnectionEventArgs> newConnection = this.NewConnection;
			if (newConnection != null)
			{
				newConnection(this, @object);
			}
		}

		public virtual void Close()
		{
			Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
		}
	}
}
