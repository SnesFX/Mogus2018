using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TaskAdderGame : Minigame
{
	public TextRenderer PathText;

	public TaskFolder RootFolder;

	public TaskAddButton TaskPrefab;

	public Transform TaskParent;

	public List<TaskFolder> Heirarchy = new List<TaskFolder>();

	public List<Transform> ActiveItems = new List<Transform>();

	public float folderWidth;

	public float fileWidth;

	public float lineWidth;

	public float lineHeight;

	public override void Begin(PlayerTask t)
	{
		base.Begin(t);
		ShowFolder(RootFolder);
	}

	public void GoToRoot()
	{
		Heirarchy.Clear();
		ShowFolder(RootFolder);
	}

	public void GoUpOne()
	{
		if (Heirarchy.Count > 1)
		{
			TaskFolder taskFolder = Heirarchy[Heirarchy.Count - 2];
			Heirarchy.RemoveAt(Heirarchy.Count - 1);
			Heirarchy.RemoveAt(Heirarchy.Count - 1);
			ShowFolder(taskFolder);
		}
	}

	public void ShowFolder(TaskFolder taskFolder)
	{
		StringBuilder stringBuilder = new StringBuilder(64);
		Heirarchy.Add(taskFolder);
		for (int i = 0; i < Heirarchy.Count; i++)
		{
			stringBuilder.Append(Heirarchy[i].FolderName);
			stringBuilder.Append("\\");
		}
		PathText.Text = stringBuilder.ToString();
		for (int j = 0; j < ActiveItems.Count; j++)
		{
			Transform transform = ActiveItems[j];
			Object.Destroy(transform.gameObject);
		}
		ActiveItems.Clear();
		float xCursor = 0f;
		float yCursor = 0f;
		for (int k = 0; k < taskFolder.SubFolders.Length; k++)
		{
			TaskFolder taskFolder2 = Object.Instantiate(taskFolder.SubFolders[k]);
			taskFolder2.Parent = this;
			taskFolder2.transform.SetParent(TaskParent);
			taskFolder2.transform.localPosition = new Vector3(xCursor, yCursor, 0f);
			taskFolder2.transform.localScale = Vector3.one;
			xCursor += folderWidth;
			if (xCursor > lineWidth)
			{
				xCursor = 0f;
				yCursor += lineHeight;
			}
			ActiveItems.Add(taskFolder2.transform);
		}
		for (int l = 0; l < taskFolder.Children.Length; l++)
		{
			TaskAddButton taskAddButton = Object.Instantiate(TaskPrefab);
			taskAddButton.MyTask = taskFolder.Children[l];
			if (taskAddButton.MyTask.TaskType == TaskTypes.DivertPower && (((DivertPowerTask)taskAddButton.MyTask).TargetSystem == SystemTypes.LowerEngine || ((DivertPowerTask)taskAddButton.MyTask).TargetSystem == SystemTypes.UpperEngine))
			{
				taskAddButton.Text.Text = TaskTypesHelpers.StringNames[(byte)taskAddButton.MyTask.TaskType] + " (" + SystemTypeHelpers.StringNames[(uint)((DivertPowerTask)taskAddButton.MyTask).TargetSystem] + ")";
			}
			else
			{
				taskAddButton.Text.Text = TaskTypesHelpers.StringNames[(byte)taskAddButton.MyTask.TaskType];
			}
			AddFileAsChild(taskAddButton, ref xCursor, ref yCursor);
		}
		if ((bool)taskFolder.InfectedButton)
		{
			TaskAddButton taskAddButton2 = Object.Instantiate(taskFolder.InfectedButton);
			taskAddButton2.Text.Text = "Be_Impostor.exe";
			AddFileAsChild(taskAddButton2, ref xCursor, ref yCursor);
		}
	}

	private void AddFileAsChild(TaskAddButton item, ref float xCursor, ref float yCursor)
	{
		item.transform.SetParent(TaskParent);
		item.transform.localPosition = new Vector3(xCursor, yCursor, 0f);
		item.transform.localScale = Vector3.one;
		xCursor += fileWidth;
		if (xCursor > lineWidth)
		{
			xCursor = 0f;
			yCursor -= lineHeight;
		}
		ActiveItems.Add(item.transform);
	}
}
