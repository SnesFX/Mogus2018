using System;
using System.Collections.Generic;

namespace Hazel
{
	public sealed class ObjectPool<T> where T : IRecyclable
	{
		private Queue<T> pool = new Queue<T>();

		private Func<T> objectFactory;

		public int Size
		{
			get
			{
				return pool.Count;
			}
		}

		internal ObjectPool(Func<T> objectFactory)
		{
			this.objectFactory = objectFactory;
		}

		internal T GetObject()
		{
			lock (pool)
			{
				if (pool.Count > 0)
				{
					return pool.Dequeue();
				}
			}
			return objectFactory();
		}

		internal void PutObject(T item)
		{
			lock (pool)
			{
				pool.Enqueue(item);
			}
		}
	}
}
