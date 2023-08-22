namespace InnerNet
{
	public enum JoinFailureReasons : byte
	{
		TooManyPlayers = 1,
		GameStarted = 2,
		GameNotFound = 3,
		HostNotReady = 4,
		IncorrectVersion = 5,
		Banned = 6,
		Kicked = 7
	}
}
