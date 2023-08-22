using UnityEngine;

public class MapRoom : MonoBehaviour
{
	public SystemTypes room;

	public SpriteRenderer door;

	public SpriteRenderer special;

	public InfectedOverlay Parent { get; set; }

	public void Start()
	{
		if ((bool)door)
		{
			door.SetCooldownNormalizedUvs();
		}
		if ((bool)special)
		{
			special.SetCooldownNormalizedUvs();
		}
	}

	public void OOBUpdate(float dt)
	{
		if ((bool)door && (bool)ShipStatus.Instance)
		{
			DoorsSystemType doorsSystemType = (DoorsSystemType)ShipStatus.Instance.Systems[SystemTypes.Doors];
			float timer = doorsSystemType.GetTimer(room);
			float value = ((!Parent.CanUseDoors) ? 1f : (timer / 30f));
			door.material.SetFloat("_Percent", value);
		}
	}

	internal void SetSpecialActive(float perc)
	{
		if ((bool)special)
		{
			special.material.SetFloat("_Percent", perc);
		}
	}

	public void SabotageReactor()
	{
		if (Parent.CanUseSpecial)
		{
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 128);
			Parent.UseSpecial();
		}
	}

	public void SabotageComms()
	{
		if (Parent.CanUseSpecial)
		{
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 128);
			Parent.UseSpecial();
		}
	}

	public void SabotageOxygen()
	{
		if (Parent.CanUseSpecial)
		{
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 128);
			Parent.UseSpecial();
		}
	}

	public void SabotageLights()
	{
		if (!Parent.CanUseSpecial)
		{
			return;
		}
		byte b = 4;
		for (int i = 0; i < 5; i++)
		{
			if (BoolRange.Next())
			{
				b = (byte)(b | (byte)(1 << i));
			}
		}
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Electrical, (byte)(b | 0x80));
		Parent.UseSpecial();
	}

	public void SabotageDoors()
	{
		if (Parent.CanUseDoors)
		{
			DoorsSystemType doorsSystemType = (DoorsSystemType)ShipStatus.Instance.Systems[SystemTypes.Doors];
			float timer = doorsSystemType.GetTimer(room);
			if (!(timer > 0f))
			{
				ShipStatus.Instance.RpcCloseDoorsOfType(room);
			}
		}
	}
}
