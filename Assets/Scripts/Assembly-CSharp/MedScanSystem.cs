using Hazel;

public class MedScanSystem : SystemType
{
	public const byte Request = 128;

	public const byte Release = 64;

	public const byte NumMask = 31;

	public sbyte CurrentUser { get; private set; }

	public MedScanSystem()
	{
		CurrentUser = -1;
	}

	public override bool Detoriorate(float deltaTime)
	{
		return false;
	}

	public override void RepairDamage(PlayerControl player, byte amount)
	{
		sbyte b = (sbyte)(amount & 0x1F);
		if ((amount & 0x80u) != 0)
		{
			if (CurrentUser < 0)
			{
				CurrentUser = b;
				PlayerControl @object = GameData.Instance.AllPlayers[CurrentUser].Object;
				if ((bool)@object)
				{
					@object.SetScanner(true);
				}
			}
		}
		else if ((amount & 0x40u) != 0 && CurrentUser == b)
		{
			PlayerControl object2 = GameData.Instance.AllPlayers[CurrentUser].Object;
			if ((bool)object2)
			{
				object2.SetScanner(false);
			}
			CurrentUser = -1;
		}
	}

	public override void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(CurrentUser);
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		sbyte currentUser = reader.ReadSByte();
		if (CurrentUser > -1)
		{
			PlayerControl @object = GameData.Instance.AllPlayers[CurrentUser].Object;
			if ((bool)@object)
			{
				@object.SetScanner(false);
			}
		}
		CurrentUser = currentUser;
		if (CurrentUser > -1)
		{
			PlayerControl object2 = GameData.Instance.AllPlayers[CurrentUser].Object;
			if ((bool)object2)
			{
				object2.SetScanner(true);
			}
		}
	}
}
