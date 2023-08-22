public class WinningPlayerData
{
	public string Name;

	public bool IsDead;

	public int ColorId;

	public uint HatId;

	public int ClientId;

	public bool IsYou;

	public WinningPlayerData()
	{
	}

	public WinningPlayerData(PlayerControl player)
	{
		IsYou = player == PlayerControl.LocalPlayer;
		Name = player.PlayerName;
		IsDead = player.IsDead;
		ColorId = player.ColorId;
		HatId = player.HatId;
		ClientId = player.OwnerId;
	}
}
