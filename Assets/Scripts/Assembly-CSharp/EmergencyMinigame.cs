using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class EmergencyMinigame : Minigame
{
	public SpriteRenderer ClosedLid;

	public SpriteRenderer OpenLid;

	public Transform meetingButton;

	public TextRenderer StatusText;

	public TextRenderer NumberText;

	public bool ButtonActive = true;

	public AudioClip ButtonSound;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		if (!PlayerControl.LocalPlayer.myTasks.Any(PlayerTask.TaskIsEmergency))
		{
			int remainingEmergencies = PlayerControl.LocalPlayer.RemainingEmergencies;
			StatusText.Text = string.Format("CREWMEMBER {0} HAS\r\n\r\nEMERGENCY MEETINGS LEFT", PlayerControl.LocalPlayer.PlayerName);
			NumberText.Text = remainingEmergencies.ToString();
			if (remainingEmergencies <= 0)
			{
				ClosedLid.gameObject.SetActive(true);
				OpenLid.gameObject.SetActive(false);
				ButtonActive = false;
			}
			else
			{
				ClosedLid.gameObject.SetActive(false);
				OpenLid.gameObject.SetActive(true);
			}
		}
		else
		{
			ButtonActive = false;
			StatusText.Text = "EMERGENCY MEETINGS CANNOT\r\nBE CALLED DURING CRISES";
			NumberText.Text = string.Empty;
			ClosedLid.gameObject.SetActive(true);
			OpenLid.gameObject.SetActive(false);
		}
	}

	public void CallMeeting()
	{
		if (!PlayerControl.LocalPlayer.myTasks.Any(PlayerTask.TaskIsEmergency) && PlayerControl.LocalPlayer.RemainingEmergencies > 0 && ButtonActive)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ButtonSound, false);
			}
			PlayerControl.LocalPlayer.RemainingEmergencies--;
			PlayerControl.LocalPlayer.CmdReportDeadBody(null);
			ButtonActive = false;
		}
	}

	private float easeOutElastic(float t)
	{
		float num = 0.3f;
		return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - num / 4f) * ((float)Math.PI * 2f) / num) + 1f;
	}

	protected override IEnumerator CoAnimateOpen()
	{
		for (float timer2 = 0f; timer2 < 0.2f; timer2 += Time.deltaTime)
		{
			float t = timer2 / 0.2f;
			base.transform.localPosition = new Vector3(0f, Mathf.SmoothStep(-8f, 0f, t), -50f);
			yield return null;
		}
		base.transform.localPosition = new Vector3(0f, 0f, -50f);
		Vector3 meetingPos = meetingButton.localPosition;
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float t2 = timer / 0.1f;
			meetingPos.y = Mathf.Sin((float)Math.PI * t2) * 1f / (t2 * 5f + 4f) - 0.882f;
			meetingButton.localPosition = meetingPos;
			yield return null;
		}
		meetingPos.y = -0.882f;
		meetingButton.localPosition = meetingPos;
	}
}
