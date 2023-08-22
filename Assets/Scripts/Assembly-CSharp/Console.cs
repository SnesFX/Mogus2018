using Assets.CoreScripts;
using UnityEngine;

public class Console : MonoBehaviour, IUsable
{
	public float usableDistance = 1f;

	public int ConsoleId;

	public bool onlyFromBelow;

	public SystemTypes Room;

	public TaskTypes[] TaskTypes;

	public TaskSet[] ValidTasks;

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
			Image.material.SetColor("_OutlineColor", Color.yellow);
			Image.material.SetColor("_AddColor", (!mainTarget) ? Color.clear : Color.yellow);
		}
	}

	public bool CanUse(PlayerControl pc)
	{
		return !pc.IsImpostor && pc.canMove && (!onlyFromBelow || pc.transform.position.y < base.transform.position.y) && (bool)FindTask(pc);
	}

	private PlayerTask FindTask(PlayerControl pc)
	{
		for (int i = 0; i < pc.myTasks.Count; i++)
		{
			PlayerTask playerTask = pc.myTasks[i];
			if (!playerTask.IsComplete && playerTask.ValidConsole(this))
			{
				return playerTask;
			}
		}
		return null;
	}

	public void Use()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (CanUse(localPlayer))
		{
			PlayerTask playerTask = FindTask(localPlayer);
			if ((bool)playerTask.MinigamePrefab && (PlayerControl.GameOptions.GhostsDoTasks || !localPlayer.IsDead) && Vector2.Distance(localPlayer.transform.position, base.transform.position) <= UsableDistance)
			{
				Minigame minigame = Object.Instantiate(playerTask.MinigamePrefab);
				minigame.transform.SetParent(Camera.main.transform, false);
				minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
				minigame.Console = this;
				minigame.Begin(playerTask);
				DestroyableSingleton<Telemetry>.Instance.WriteUse(localPlayer.PlayerId, (byte)playerTask.TaskType, base.transform.position);
			}
		}
	}
}
