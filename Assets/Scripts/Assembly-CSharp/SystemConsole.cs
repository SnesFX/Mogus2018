using UnityEngine;

public class SystemConsole : MonoBehaviour, IUsable
{
	public float usableDistance = 1f;

	public SpriteRenderer Image;

	public Minigame MinigamePrefab;

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
		return pc.canMove && (!pc.IsDead || !(MinigamePrefab is EmergencyMinigame));
	}

	public void Use()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (CanUse(localPlayer) && (PlayerControl.GameOptions.GhostsDoTasks || !localPlayer.IsDead) && Vector2.Distance(localPlayer.transform.position, base.transform.position) <= UsableDistance)
		{
			Minigame minigame = Object.Instantiate(MinigamePrefab);
			minigame.transform.SetParent(Camera.main.transform, false);
			minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
			minigame.Begin(null);
		}
	}
}
