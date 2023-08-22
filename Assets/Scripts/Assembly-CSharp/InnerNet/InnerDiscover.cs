using System;
using System.Collections;
using Hazel.Udp;
using UnityEngine;

namespace InnerNet
{
	public class InnerDiscover : DestroyableSingleton<InnerDiscover>
	{
		private UdpBroadcastListener listener;

		private UdpBroadcaster sender;

		public int Port = 47777;

		public float Interval = 1f;

		public event Action<BroadcastPacket> OnPacketGet;

		public void StartAsServer(string data)
		{
			bool flag = sender == null;
			if (flag)
			{
				sender = new UdpBroadcaster(Port);
			}
			sender.SetData(data);
			if (flag)
			{
				StartCoroutine(RunServer());
			}
		}

		private IEnumerator RunServer()
		{
			while (sender != null)
			{
				sender.Broadcast();
				for (float timer = 0f; timer < Interval; timer += Time.deltaTime)
				{
					yield return null;
				}
			}
		}

		public void StopServer()
		{
			if (sender != null)
			{
				sender.Dispose();
				sender = null;
			}
		}

		public void StartAsClient()
		{
			if (listener == null)
			{
				listener = new UdpBroadcastListener(Port);
				listener.StartListen();
				StartCoroutine(RunClient());
			}
		}

		private IEnumerator RunClient()
		{
			while (listener != null)
			{
				if (!listener.Running)
				{
					listener.StartListen();
				}
				BroadcastPacket[] pkts = listener.GetPackets();
				for (int i = 0; i < pkts.Length; i++)
				{
					if (this.OnPacketGet != null)
					{
						this.OnPacketGet(pkts[i]);
					}
				}
				yield return null;
			}
		}

		public void StopClient()
		{
			if (listener != null)
			{
				listener.Dispose();
				listener = null;
			}
		}

		public override void OnDestroy()
		{
			StopServer();
			StopClient();
			base.OnDestroy();
		}
	}
}
