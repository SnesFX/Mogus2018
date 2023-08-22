using UnityEngine;

public class TaskAddButton : MonoBehaviour
{
	public TextRenderer Text;

	public SpriteRenderer Overlay;

	public Sprite CheckImage;

	public Sprite ExImage;

	public PlayerTask MyTask;

	public bool ImpostorTask;

	public void Start()
	{
		if (ImpostorTask)
		{
			Overlay.enabled = PlayerControl.LocalPlayer.IsImpostor;
			Overlay.sprite = CheckImage;
			return;
		}
		PlayerTask playerTask = FindTaskByType();
		if ((bool)playerTask)
		{
			Overlay.enabled = true;
			Overlay.sprite = ((!playerTask.IsComplete) ? ExImage : CheckImage);
		}
		else
		{
			Overlay.enabled = false;
		}
	}

	public void AddTask()
	{
		if (ImpostorTask)
		{
			if (PlayerControl.LocalPlayer.IsImpostor)
			{
				PlayerControl.LocalPlayer.RemoveInfected();
				Overlay.enabled = false;
			}
			else
			{
				PlayerControl.LocalPlayer.RpcSetInfected(new PlayerControl[1] { PlayerControl.LocalPlayer });
				Overlay.enabled = true;
			}
			return;
		}
		PlayerTask playerTask = FindTaskByType();
		if (!playerTask)
		{
			PlayerTask playerTask2 = Object.Instantiate(MyTask, PlayerControl.LocalPlayer.transform);
			playerTask2.Id = PlayerControl.LocalPlayer.myTaskCount++;
			playerTask2.Owner = PlayerControl.LocalPlayer;
			playerTask2.Initialize();
			PlayerControl.LocalPlayer.myTasks.Add(playerTask2);
			GameData.Instance.TutOnlyAddTask(PlayerControl.LocalPlayer.PlayerId, playerTask2.Id);
			Overlay.sprite = ExImage;
			Overlay.enabled = true;
		}
		else
		{
			PlayerControl.LocalPlayer.RemoveTask(playerTask);
			Overlay.enabled = false;
		}
	}

	private PlayerTask FindTaskByType()
	{
		for (int num = PlayerControl.LocalPlayer.myTasks.Count - 1; num > -1; num--)
		{
			PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[num];
			if (playerTask.TaskType == MyTask.TaskType)
			{
				if (playerTask.TaskType == TaskTypes.DivertPower)
				{
					if (((DivertPowerTask)playerTask).TargetSystem == ((DivertPowerTask)MyTask).TargetSystem)
					{
						return playerTask;
					}
				}
				else
				{
					if (playerTask.TaskType != TaskTypes.UploadData)
					{
						return playerTask;
					}
					if (playerTask.StartAt == MyTask.StartAt)
					{
						return playerTask;
					}
				}
			}
		}
		return null;
	}
}
