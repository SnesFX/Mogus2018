using System.Linq;
using System.Text;
using UnityEngine;

public class ElectricTask : PlayerTask
{
	public ArrowBehaviour Arrow;

	private bool isComplete;

	private SwitchSystem system;

	private bool even;

	public override int TaskStep
	{
		get
		{
			return isComplete ? 1 : 0;
		}
	}

	public override bool IsComplete
	{
		get
		{
			return isComplete;
		}
	}

	public override void Initialize()
	{
		ShipStatus instance = ShipStatus.Instance;
		system = (SwitchSystem)instance.Systems[SystemTypes.Electrical];
		Arrow.target = FindObjectPos().transform.position;
		Arrow.gameObject.SetActive(true);
	}

	private void FixedUpdate()
	{
		if (!isComplete && system.ExpectedSwitches == system.ActualSwitches)
		{
			Complete();
		}
	}

	public override bool ValidConsole(Console console)
	{
		return console.TaskTypes.Contains(TaskTypes.FixLights);
	}

	public override void Complete()
	{
		isComplete = true;
		PlayerControl.LocalPlayer.RemoveTask(this);
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		even = !even;
		Color color = ((!even) ? Color.red : Color.yellow);
		sb.Append(color.ToTextColor() + "Fix lights ");
		sb.AppendLine(" (%" + (int)(system.Level * 100f) + ")[]");
		Arrow.GetComponent<SpriteRenderer>().color = color;
	}
}
