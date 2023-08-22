using System.IO;
using System.Text;
using UnityEngine;

public class GameOptionsData
{
	public static readonly float[] KillDistances = new float[3] { 1f, 1.8f, 2.5f };

	public static readonly string[] KillDistanceStrings = new string[3] { "Short", "Normal", "Long" };

	public float PlayerSpeedMod = 1f;

	public float CrewLightMod = 1f;

	public float ImpostorLightMod = 1.5f;

	public float KillCooldown = 15f;

	public int NumCommonTasks = 1;

	public int NumLongTasks = 1;

	public int NumShortTasks = 2;

	public int NumEmergencyMeetings = 1;

	public int NumImpostors = 1;

	public bool GhostsDoTasks = true;

	public int KillDistance = 1;

	public int DiscussionTime = 15;

	public int VotingTime;

	public bool isDefaults = true;

	private static readonly int[] RecommendedKillCooldown = new int[11]
	{
		0, 0, 0, 0, 45, 30, 15, 35, 30, 25,
		20
	};

	private static readonly int[] RecommendedImpostors = new int[11]
	{
		0, 0, 0, 0, 1, 1, 1, 2, 2, 2,
		2
	};

	private static readonly int[] MaxImpostors = new int[11]
	{
		0, 0, 0, 0, 1, 1, 1, 2, 2, 3,
		3
	};

	public void SetRecommendations(int numPlayers)
	{
		numPlayers = Mathf.Clamp(numPlayers, 4, 10);
		PlayerSpeedMod = 1f;
		CrewLightMod = 1f;
		ImpostorLightMod = 1.5f;
		KillCooldown = RecommendedKillCooldown[numPlayers];
		NumCommonTasks = 1;
		NumLongTasks = 1;
		NumShortTasks = 2;
		NumEmergencyMeetings = 1;
		NumImpostors = RecommendedImpostors[numPlayers];
		KillDistance = 1;
		DiscussionTime = 15;
		VotingTime = 0;
		isDefaults = true;
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write(Constants.GetBroadcastVersion());
		writer.Write(PlayerSpeedMod);
		writer.Write(CrewLightMod);
		writer.Write(ImpostorLightMod);
		writer.Write(KillCooldown);
		writer.Write(NumCommonTasks);
		writer.Write(NumLongTasks);
		writer.Write(NumShortTasks);
		writer.Write(NumEmergencyMeetings);
		writer.Write(NumImpostors);
		writer.Write(KillDistance);
		writer.Write(DiscussionTime);
		writer.Write(VotingTime);
		writer.Write(isDefaults);
	}

	public void Deserialize(BinaryReader reader)
	{
		isDefaults = false;
		try
		{
			int num = reader.ReadInt32();
			if (num == Constants.GetBroadcastVersion())
			{
				PlayerSpeedMod = reader.ReadSingle();
				CrewLightMod = reader.ReadSingle();
				ImpostorLightMod = reader.ReadSingle();
				KillCooldown = reader.ReadSingle();
				NumCommonTasks = reader.ReadInt32();
				NumLongTasks = reader.ReadInt32();
				NumShortTasks = reader.ReadInt32();
				NumEmergencyMeetings = reader.ReadInt32();
				NumImpostors = reader.ReadInt32();
				KillDistance = reader.ReadInt32();
				DiscussionTime = reader.ReadInt32();
				VotingTime = reader.ReadInt32();
				isDefaults = reader.ReadBoolean();
			}
		}
		catch
		{
		}
	}

	public byte[] ToBytes()
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				Serialize(binaryWriter);
				binaryWriter.Flush();
				memoryStream.Position = 0L;
				return memoryStream.ToArray();
			}
		}
	}

	public static GameOptionsData FromBytes(byte[] bytes)
	{
		GameOptionsData gameOptionsData = new GameOptionsData();
		using (MemoryStream input = new MemoryStream(bytes))
		{
			using (BinaryReader reader = new BinaryReader(input))
			{
				gameOptionsData.Deserialize(reader);
				return gameOptionsData;
			}
		}
	}

	public override string ToString()
	{
		return ToHudString(10);
	}

	public string ToHudString(int numPlayers)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.AppendLine((!isDefaults) ? "Custom Settings" : "Recommended Settings");
		int num = MaxImpostors[numPlayers];
		stringBuilder.Append(string.Format("Impostors: {0}", NumImpostors));
		if (NumImpostors > num)
		{
			stringBuilder.Append(string.Format(" (Limit: {0})", num));
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Emergency Meetings: " + NumEmergencyMeetings);
		stringBuilder.AppendLine(string.Format("Discussion Time: {0}s", DiscussionTime));
		if (VotingTime > 0)
		{
			stringBuilder.AppendLine(string.Format("Voting Time: {0}s", VotingTime));
		}
		else
		{
			stringBuilder.AppendLine(string.Format("Voting Time: {0}s", "∞"));
		}
		stringBuilder.AppendLine(string.Format("Player Speed: {0}x", PlayerSpeedMod));
		stringBuilder.AppendLine(string.Format("Crew Light: {0}x", CrewLightMod));
		stringBuilder.AppendLine(string.Format("Impostor Light: {0}x", ImpostorLightMod));
		stringBuilder.AppendLine(string.Format("Kill Cooldown: {0}s", KillCooldown));
		stringBuilder.AppendLine(string.Format("Kill Distance: {0}", KillDistanceStrings[KillDistance]));
		stringBuilder.AppendLine("Common Tasks: " + NumCommonTasks);
		stringBuilder.AppendLine("Long Tasks: " + NumLongTasks);
		stringBuilder.Append("Short Tasks: " + NumShortTasks);
		return stringBuilder.ToString();
	}

	public bool Validate(int numPlayers)
	{
		bool result = false;
		if (NumCommonTasks + NumLongTasks + NumShortTasks == 0)
		{
			NumShortTasks = 1;
			result = true;
		}
		int num = MaxImpostors[numPlayers];
		if (NumImpostors > num)
		{
			NumImpostors = num;
			result = true;
		}
		return result;
	}
}
