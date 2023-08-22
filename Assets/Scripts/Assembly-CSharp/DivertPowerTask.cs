using System.Linq;
using System.Text;

public class DivertPowerTask : NormalPlayerTask
{
	public SystemTypes TargetSystem;

	public override bool ValidConsole(Console console)
	{
		return (console.Room == TargetSystem && console.ValidTasks.Any((TaskSet set) => TaskType == set.taskType && set.taskStep.Contains(taskStep))) || (taskStep == 0 && console.TaskTypes.Contains(TaskType));
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
		if (taskStep == 0)
		{
			sb.Append(StartAt);
			sb.Append(": Divert Power to ");
			sb.Append(TargetSystem);
		}
		else
		{
			sb.Append(TargetSystem);
			sb.Append(": Accept diverted power");
		}
		sb.Append(" (");
		sb.Append(taskStep);
		sb.Append("/");
		sb.Append(MaxStep);
		sb.AppendLine(")");
		if (taskStep > 0)
		{
			sb.Append("[]");
		}
	}
}
