using UnityEngine;

public class MapConsole : MonoBehaviour, IUsable
{
	public float usableDistance = 1f;

	public SpriteRenderer Image;

	public float UsableDistance
	{
		get
		{
			return usableDistance;
		}
	}

	public float PercentCool
	{
		get
		{
			return 0f;
		}
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		if ((bool)Image)
		{
			Image.material.SetFloat("_Outline", on ? 1 : 0);
			Image.material.SetColor("_OutlineColor", Color.white);
			Image.material.SetColor("_AddColor", (!mainTarget) ? Color.clear : Color.white);
		}
	}

	public bool CanUse(PlayerControl pc)
	{
		return pc.canMove;
	}

	public void Use()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (CanUse(localPlayer) && Vector2.Distance(localPlayer.transform.position, base.transform.position) <= UsableDistance)
		{
			DestroyableSingleton<HudManager>.Instance.Map.ShowCountOverlay();
		}
	}
}
