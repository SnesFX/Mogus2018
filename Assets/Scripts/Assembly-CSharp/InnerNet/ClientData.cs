using System;

namespace InnerNet
{
	[Serializable]
	public class ClientData
	{
		public int Id;

		public bool InScene;

		public PlayerControl Character;

		public ClientData(int id)
		{
			Id = id;
		}
	}
}
