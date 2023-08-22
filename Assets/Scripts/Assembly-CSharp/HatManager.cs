using System.Collections.Generic;

public class HatManager : DestroyableSingleton<HatManager>
{
	public HatBehaviour NoneHat;

	public List<HatBehaviour> AllHats = new List<HatBehaviour>();

	public HatBehaviour GetHatById(uint hatId)
	{
		if (hatId >= AllHats.Count)
		{
			return NoneHat;
		}
		return AllHats[(int)hatId];
	}

	public HatBehaviour[] GetUnlockedHats()
	{
		return AllHats.ToArray();
	}

	public uint GetIdFromHat(HatBehaviour hat)
	{
		return (uint)AllHats.IndexOf(hat);
	}
}
