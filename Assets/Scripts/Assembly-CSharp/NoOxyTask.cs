using System.Linq;
using System.Text;
using UnityEngine;

public class NoOxyTask : PlayerTask
{
	public ArrowBehaviour[] Arrows;

	private bool isComplete;

	private ReactorSystemType reactor;

	private bool even;

	public int targetNumber;

	public override int TaskStep
	{
		get
		{
			return reactor.UserCount;
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
		targetNumber = IntRange.Next(0, 99999);
		ShipStatus instance = ShipStatus.Instance;
		reactor = (ReactorSystemType)instance.Systems[SystemTypes.LifeSupp];
		Console[] array = (from c in FindConsoles()
			orderby c.ConsoleId
			select c).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Arrows[i].target = array[i].transform.position;
			Arrows[i].gameObject.SetActive(true);
		}
		DestroyableSingleton<HudManager>.Instance.StartOxyFlash();
	}

	private void FixedUpdate()
	{
		if (isComplete)
		{
			return;
		}
		if (!reactor.IsActive)
		{
			Complete();
			return;
		}
		for (int i = 0; i < Arrows.Length; i++)
		{
			Arrows[i].gameObject.SetActive(!reactor.GetConsoleComplete(i));
		}
	}

	public override bool ValidConsole(Console console)
	{
		return !reactor.GetConsoleComplete(console.ConsoleId) && console.TaskTypes.Contains(TaskTypes.RestoreOxy);
	}

	public override void OnRemove()
	{
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
		sb.Append(color.ToTextColor() + "Oxygen depleted in " + (int)reactor.Countdown);
		sb.AppendLine(" (" + reactor.UserCount + "/" + (byte)2 + ")[]");
		for (int i = 0; i < Arrows.Length; i++)
		{
			Arrows[i].GetComponent<SpriteRenderer>().color = color;
		}
	}
}
