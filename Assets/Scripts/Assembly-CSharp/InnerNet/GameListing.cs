using System;

namespace InnerNet
{
	[Serializable]
	public struct GameListing
	{
		public int GameId;

		public int PlayerCount;

		public int Age;

		public string HostName;

		public GameListing(int id, int count, int age, string host)
		{
			GameId = id;
			PlayerCount = count;
			Age = age;
			HostName = host;
		}
	}
}
