using UnityEngine;

public class OptionsConsole : MonoBehaviour, IUsable
{
	public CustomPlayerMenu MenuPrefab;

	public SpriteRenderer Outline;

	public float UsableDistance
	{
		get
		{
			return 1f;
		}
	}

	public float PercentCool
	{
		get
		{
			return 0f;
		}
	}

	public bool CanUse(PlayerControl pc)
	{
		return pc.canMove;
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		if ((bool)Outline)
		{
			Outline.material.SetFloat("_Outline", on ? 1 : 0);
			Outline.material.SetColor("_OutlineColor", Color.white);
			Outline.material.SetColor("_AddColor", (!mainTarget) ? Color.clear : Color.white);
		}
	}

	public void Use()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (CanUse(localPlayer) && (PlayerControl.GameOptions.GhostsDoTasks || !localPlayer.IsDead) && Vector2.Distance(localPlayer.transform.position, base.transform.position) <= UsableDistance)
		{
			CustomPlayerMenu customPlayerMenu = Object.Instantiate(MenuPrefab);
			customPlayerMenu.transform.SetParent(Camera.main.transform, false);
			customPlayerMenu.transform.localPosition = new Vector3(0f, 0f, -20f);
			localPlayer.canMove = false;
			localPlayer.NetTransform.Halt();
		}
	}
}
