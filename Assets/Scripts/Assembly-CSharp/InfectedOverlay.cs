using System.Linq;
using UnityEngine;

public class InfectedOverlay : MonoBehaviour
{
	public const float SpecialSabDelay = 30f;

	public MapRoom[] rooms;

	private float specialTimer;

	private IActivatable[] specials = new IActivatable[0];

	private IActivatable doors;

	public bool CanUseDoors
	{
		get
		{
			return !specials.Any((IActivatable s) => s.IsActive);
		}
	}

	public bool CanUseSpecial
	{
		get
		{
			return specialTimer <= 0f && !doors.IsActive;
		}
	}

	public void Start()
	{
		for (int i = 0; i < rooms.Length; i++)
		{
			rooms[i].Parent = this;
		}
		specials = new IActivatable[4]
		{
			(IActivatable)ShipStatus.Instance.Systems[SystemTypes.Comms],
			(IActivatable)ShipStatus.Instance.Systems[SystemTypes.Reactor],
			(IActivatable)ShipStatus.Instance.Systems[SystemTypes.LifeSupp],
			(IActivatable)ShipStatus.Instance.Systems[SystemTypes.Electrical]
		};
		doors = (IActivatable)ShipStatus.Instance.Systems[SystemTypes.Doors];
	}

	public void OOBUpdate(float dt)
	{
		if (doors == null)
		{
			return;
		}
		if (!specials.Any((IActivatable s) => s.IsActive))
		{
			specialTimer = Mathf.Max(0f, specialTimer - dt);
			float specialActive = ((!doors.IsActive) ? (specialTimer / 30f) : 1f);
			for (int i = 0; i < rooms.Length; i++)
			{
				rooms[i].SetSpecialActive(specialActive);
			}
		}
		else
		{
			UseSpecial();
		}
		for (int j = 0; j < rooms.Length; j++)
		{
			rooms[j].OOBUpdate(dt);
		}
	}

	public void UseSpecial()
	{
		specialTimer = 30f;
		for (int i = 0; i < rooms.Length; i++)
		{
			rooms[i].SetSpecialActive(specialTimer / 30f);
		}
	}
}
