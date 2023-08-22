using System.Linq;
using System.Text;

public class UploadDataTask : NormalPlayerTask
{
	public override bool ValidConsole(Console console)
	{
		return (console.Room == StartAt && console.ValidTasks.Any((TaskSet set) => TaskType == set.taskType && set.taskStep.Contains(taskStep))) || (taskStep == 1 && console.TaskTypes.Contains(TaskType));
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		if (taskStep > 0)
		{
			if (IsComplete)
			{
				sb.Append("[00DD00FF]");
			}
			else
			{
				sb.Append("[FFFF00FF]");
			}
		}
		sb.Append(SystemTypeHelpers.StringNames[(uint)((taskStep != 0) ? SystemTypes.Admin : StartAt)]);
		sb.Append(": ");
		sb.Append((taskStep != 0) ? "Upload" : "Download");
		sb.Append(" Data (");
		sb.Append(taskStep);
		sb.Append("/");
		sb.Append(MaxStep);
		sb.AppendLine(") []");
	}
}
