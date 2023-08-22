using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class PlayerTask : MonoBehaviour
{
	public SystemTypes StartAt;

	public TaskTypes TaskType;

	public Minigame MinigamePrefab;

	public bool HasLocation;

	public bool LocationDirty = true;

	public int Index { get; internal set; }

	public uint Id { get; internal set; }

	public PlayerControl Owner { get; internal set; }

	public abstract int TaskStep { get; }

	public abstract bool IsComplete { get; }

	public Vector2 Location
	{
		get
		{
			LocationDirty = false;
			return FindObjectPos().transform.position;
		}
	}

	public abstract void Initialize();

	public virtual void OnRemove()
	{
	}

	public abstract bool ValidConsole(Console console);

	public abstract void Complete();

	public abstract void AppendTaskText(StringBuilder sb);

	internal static bool TaskIsEmergency(PlayerTask arg)
	{
		return arg is NoOxyTask || arg is HudOverrideTask || arg is ReactorTask || arg is ElectricTask;
	}

	protected List<Console> FindConsoles()
	{
		List<Console> list = new List<Console>();
		Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (ValidConsole(allConsoles[i]))
			{
				list.Add(allConsoles[i]);
			}
		}
		return list;
	}

	protected List<Vector2> FindObjectsPos()
	{
		List<Vector2> list = new List<Vector2>();
		Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (ValidConsole(allConsoles[i]))
			{
				list.Add(allConsoles[i].transform.position);
			}
		}
		return list;
	}

	protected Console FindSpecialConsole(Func<Console, bool> func)
	{
		Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (func(allConsoles[i]))
			{
				return allConsoles[i];
			}
		}
		return null;
	}

	protected Console FindObjectPos()
	{
		Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (ValidConsole(allConsoles[i]))
			{
				return allConsoles[i];
			}
		}
		return null;
	}
}
