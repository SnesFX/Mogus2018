using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HudOverrideTask : PlayerTask
{
	public const byte TaskStateNew = 0;

	public const byte TaskStateTasked = 1;

	public const byte TaskStateComplete = 2;

	public ArrowBehaviour[] Arrows;

	private bool isComplete;

	private HudOverrideSystemType system;

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
		system = instance.Systems[SystemTypes.Comms] as HudOverrideSystemType;
		List<Vector2> list = FindObjectsPos();
		for (int i = 0; i < list.Count; i++)
		{
			Arrows[i].target = list[i];
			Arrows[i].gameObject.SetActive(true);
		}
	}

	private void FixedUpdate()
	{
		if (!isComplete && system.TaskState == 2)
		{
			Complete();
		}
	}

	public override bool ValidConsole(Console console)
	{
		return console.TaskTypes.Contains(TaskTypes.FixComms);
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
		sb.Append(color.ToTextColor() + "Hud Sabotaged[]");
		for (int i = 0; i < Arrows.Length; i++)
		{
			Arrows[i].GetComponent<SpriteRenderer>().color = color;
		}
	}
}
